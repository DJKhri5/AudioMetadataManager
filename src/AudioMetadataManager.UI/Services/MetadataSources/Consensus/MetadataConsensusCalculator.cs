using AudioMetadataManager.UI.Services.MetadataSources.Matching;

namespace AudioMetadataManager.UI.Services.MetadataSources.Consensus;

/// <summary>
/// Calcula el consenso de un campo de metadatos a partir
/// de los valores entregados por varias fuentes externas.
/// </summary>
public class MetadataConsensusCalculator
{
    private readonly TextSimilarityCalculator _textSimilarity =
        new();

    /// <summary>
    /// Umbral mínimo para considerar que dos valores
    /// representan el mismo dato.
    ///
    /// Ejemplo:
    /// "Armin van Buuren"
    /// "Armin Van Buuren"
    /// </summary>
    private const int SimilarityThreshold = 90;

    /// <summary>
    /// Construye el consenso para un campo textual.
    /// </summary>
    /// <param name="fieldName">
    /// Nombre legible del campo, por ejemplo:
    /// Artista, Título, Versión, Género o Sello.
    /// </param>
    /// <param name="valuesBySource">
    /// Valores obtenidos desde las distintas plataformas.
    ///
    /// La clave es la fuente:
    /// Beatport, Discogs, Spotify o SoundCloud.
    ///
    /// El valor es el dato entregado por esa fuente.
    /// </param>
    public MetadataConsensusField Calculate(
        string fieldName,
        IReadOnlyDictionary<string, string?> valuesBySource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentNullException.ThrowIfNull(valuesBySource);

        List<KeyValuePair<string, string>> usableValues =
            GetUsableValues(valuesBySource);

        if (usableValues.Count == 0)
        {
            return CreateEmptyResult(fieldName);
        }

        List<MetadataConsensusValue> groups =
            GroupSimilarValues(usableValues);

        MetadataConsensusValue selectedGroup =
            SelectWinningGroup(groups);

        bool hasTie =
            HasWinningTie(
                groups,
                selectedGroup.VoteCount);

        bool hasConflict =
            DetermineConflict(
                groups,
                selectedGroup,
                hasTie);

        int confidenceScore =
            CalculateConfidenceScore(
                usableValues.Count,
                selectedGroup,
                hasTie,
                hasConflict);

        bool requiresSourceApproval =
            selectedGroup.RequiresSourceApproval;

        bool requiresManualReview =
            DetermineManualReview(
                confidenceScore,
                hasTie,
                hasConflict,
                requiresSourceApproval);

        MetadataConsensusField result = new()
        {
            FieldName = fieldName,

            SelectedValue =
                selectedGroup.Value,

            ConfidenceScore =
                confidenceScore,

            SupportingSources =
                selectedGroup.Sources
                    .OrderBy(source => source)
                    .ToList(),

            AlternativeValues =
                BuildAlternativeValues(
                    usableValues,
                    selectedGroup),

            HasConflict =
                hasConflict,

            RequiresSourceApproval =
                requiresSourceApproval,

            RequiresManualReview =
                requiresManualReview,

            Reason =
                BuildReason(
                    usableValues.Count,
                    selectedGroup,
                    hasTie,
                    hasConflict,
                    requiresSourceApproval,
                    requiresManualReview)
        };

        return result;
    }

    /// <summary>
    /// Elimina valores vacíos y normaliza espacios exteriores.
    /// </summary>
    private static List<KeyValuePair<string, string>>
        GetUsableValues(
            IReadOnlyDictionary<string, string?> valuesBySource)
    {
        return valuesBySource
            .Where(
                pair =>
                    !string.IsNullOrWhiteSpace(pair.Key) &&
                    !string.IsNullOrWhiteSpace(pair.Value))
            .Select(
                pair =>
                    new KeyValuePair<string, string>(
                        pair.Key.Trim(),
                        pair.Value!.Trim()))
            .ToList();
    }

    /// <summary>
    /// Agrupa valores equivalentes aunque existan pequeñas
    /// diferencias de escritura.
    /// </summary>
    private List<MetadataConsensusValue> GroupSimilarValues(
        IReadOnlyList<KeyValuePair<string, string>> values)
    {
        List<MetadataConsensusValue> groups = new();

        foreach (KeyValuePair<string, string> sourceValue in values)
        {
            MetadataConsensusValue? matchingGroup =
                FindSimilarGroup(
                    groups,
                    sourceValue.Value);

            if (matchingGroup == null)
            {
                groups.Add(
                    new MetadataConsensusValue
                    {
                        Value = sourceValue.Value,

                        Sources =
                            new List<string>
                            {
                                sourceValue.Key
                            }
                    });

                continue;
            }

            if (!matchingGroup.Sources.Contains(
                    sourceValue.Key,
                    StringComparer.OrdinalIgnoreCase))
            {
                matchingGroup.Sources.Add(
                    sourceValue.Key);
            }

            /*
             * Conservamos el valor más limpio o más representativo.
             * Por ahora priorizamos el texto más corto cuando ambos
             * son suficientemente similares.
             *
             * Esto evita seleccionar variantes como:
             * "Armin van Buuren Official"
             * cuando existe "Armin van Buuren".
             */
            matchingGroup.Value =
                SelectPreferredDisplayValue(
                    matchingGroup.Value,
                    sourceValue.Value);
        }

        return groups;
    }

    private MetadataConsensusValue? FindSimilarGroup(
        IEnumerable<MetadataConsensusValue> groups,
        string candidateValue)
    {
        MetadataConsensusValue? bestGroup = null;
        int bestScore = 0;

        foreach (MetadataConsensusValue group in groups)
        {
            int score =
                _textSimilarity.Calculate(
                    group.Value,
                    candidateValue);

            if (score < SimilarityThreshold ||
                score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            bestGroup = group;
        }

        return bestGroup;
    }

    /// <summary>
    /// Elige el grupo con mayor cantidad de fuentes.
    ///
    /// En caso de igualdad, se prefiere:
    /// 1. el grupo sin aprobación obligatoria;
    /// 2. el valor más corto;
    /// 3. orden alfabético estable.
    /// </summary>
    private static MetadataConsensusValue SelectWinningGroup(
        IEnumerable<MetadataConsensusValue> groups)
    {
        return groups
            .OrderByDescending(
                group => group.VoteCount)
            .ThenBy(
                group => group.RequiresSourceApproval)
            .ThenBy(
                group => group.Value.Length)
            .ThenBy(
                group => group.Value,
                StringComparer.OrdinalIgnoreCase)
            .First();
    }

    private static bool HasWinningTie(
        IEnumerable<MetadataConsensusValue> groups,
        int winningVoteCount)
    {
        return groups.Count(
            group =>
                group.VoteCount ==
                winningVoteCount) > 1;
    }

    private bool DetermineConflict(
        IReadOnlyList<MetadataConsensusValue> groups,
        MetadataConsensusValue selectedGroup,
        bool hasTie)
    {
        if (hasTie)
        {
            return true;
        }

        foreach (MetadataConsensusValue group in groups)
        {
            if (ReferenceEquals(
                    group,
                    selectedGroup))
            {
                continue;
            }

            int similarity =
                _textSimilarity.Calculate(
                    selectedGroup.Value,
                    group.Value);

            /*
             * Una alternativa claramente diferente se considera
             * conflicto cuando al menos una fuente la respalda.
             */
            if (similarity < 70)
            {
                return true;
            }
        }

        return false;
    }

    private static int CalculateConfidenceScore(
        int totalSourceCount,
        MetadataConsensusValue selectedGroup,
        bool hasTie,
        bool hasConflict)
    {
        if (totalSourceCount <= 0)
        {
            return 0;
        }

        double supportRatio =
            (double)selectedGroup.VoteCount /
            totalSourceCount;

        int score =
            (int)Math.Round(
                supportRatio * 100);

        /*
         * Un solo proveedor no puede entregar consenso completo.
         * Puede ser una buena propuesta, pero sigue sin existir
         * confirmación cruzada.
         */
        if (totalSourceCount == 1)
        {
            score = Math.Min(
                score,
                75);
        }

        if (hasTie)
        {
            score -= 20;
        }

        if (hasConflict)
        {
            score -= 15;
        }

        return Math.Clamp(
            score,
            0,
            100);
    }

    private static bool DetermineManualReview(
        int confidenceScore,
        bool hasTie,
        bool hasConflict,
        bool requiresSourceApproval)
    {
        if (requiresSourceApproval)
        {
            return true;
        }

        if (hasTie ||
            hasConflict)
        {
            return true;
        }

        return confidenceScore < 85;
    }

    private static Dictionary<string, string>
        BuildAlternativeValues(
            IEnumerable<KeyValuePair<string, string>> usableValues,
            MetadataConsensusValue selectedGroup)
    {
        Dictionary<string, string> alternatives =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, string> sourceValue
                 in usableValues)
        {
            bool supportsSelectedValue =
                selectedGroup.Sources.Contains(
                    sourceValue.Key,
                    StringComparer.OrdinalIgnoreCase);

            if (supportsSelectedValue)
            {
                continue;
            }

            alternatives[sourceValue.Key] =
                sourceValue.Value;
        }

        return alternatives;
    }

    private static string SelectPreferredDisplayValue(
        string currentValue,
        string candidateValue)
    {
        if (string.IsNullOrWhiteSpace(currentValue))
        {
            return candidateValue;
        }

        if (string.IsNullOrWhiteSpace(candidateValue))
        {
            return currentValue;
        }

        return candidateValue.Length <
               currentValue.Length
            ? candidateValue
            : currentValue;
    }

    private static MetadataConsensusField CreateEmptyResult(
        string fieldName)
    {
        return new MetadataConsensusField
        {
            FieldName = fieldName,

            SelectedValue =
                string.Empty,

            ConfidenceScore =
                0,

            HasConflict =
                false,

            RequiresSourceApproval =
                false,

            RequiresManualReview =
                true,

            Reason =
                "Ninguna fuente entregó un valor utilizable."
        };
    }

    private static string BuildReason(
        int totalSourceCount,
        MetadataConsensusValue selectedGroup,
        bool hasTie,
        bool hasConflict,
        bool requiresSourceApproval,
        bool requiresManualReview)
    {
        List<string> reasons = new()
        {
            $"{selectedGroup.VoteCount} de " +
            $"{totalSourceCount} fuente(s) respaldan " +
            "el valor seleccionado."
        };

        if (hasTie)
        {
            reasons.Add(
                "Existe un empate entre propuestas.");
        }

        if (hasConflict)
        {
            reasons.Add(
                "Se detectaron valores alternativos incompatibles.");
        }

        if (requiresSourceApproval)
        {
            reasons.Add(
                "SoundCloud participa en el valor seleccionado " +
                "y exige aprobación manual.");
        }

        reasons.Add(
            requiresManualReview
                ? "El campo requiere revisión manual."
                : "El campo alcanza el nivel de confianza requerido.");

        return string.Join(
            " ",
            reasons);
    }
}