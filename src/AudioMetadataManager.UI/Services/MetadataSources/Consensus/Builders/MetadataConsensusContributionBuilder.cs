using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Normalization;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Builders;

/// <summary>
/// Convierte candidatos externos ya evaluados en
/// contribuciones individuales para el motor de consenso.
///
/// Cada campo utilizable genera una propuesta independiente,
/// conservando su fuente, confianza y trazabilidad.
/// </summary>
public sealed class MetadataConsensusContributionBuilder
{
    private readonly IMetadataConsensusValueNormalizer
    _valueNormalizer;

    /// <summary>
    /// Crea el constructor con el normalizador predeterminado.
    /// </summary>
    public MetadataConsensusContributionBuilder()
        : this(
            new DefaultMetadataConsensusValueNormalizer())
    {
    }

    /// <summary>
    /// Crea el constructor con un normalizador personalizado.
    /// </summary>
    public MetadataConsensusContributionBuilder(
        IMetadataConsensusValueNormalizer valueNormalizer)
    {
        _valueNormalizer =
            valueNormalizer ??
            throw new ArgumentNullException(
                nameof(valueNormalizer));
    }
    /// <summary>
    /// Construye todas las contribuciones utilizables a partir
    /// de una colección de candidatos evaluados.
    /// </summary>
    public IReadOnlyList<MetadataConsensusContribution> Build(
        IEnumerable<MetadataCandidateEvaluationResult> evaluations)
    {
        ArgumentNullException.ThrowIfNull(
            evaluations);

        List<MetadataConsensusContribution> contributions =
            new();

        foreach (
            MetadataCandidateEvaluationResult evaluation
            in evaluations)
        {
            if (evaluation is null ||
                !evaluation.IsUsable)
            {
                continue;
            }

            AddCandidateContributions(
                contributions,
                evaluation);
        }

        return contributions;
    }

    /// <summary>
    /// Construye las contribuciones de un único candidato.
    /// </summary>
    public IReadOnlyList<MetadataConsensusContribution> Build(
        MetadataCandidateEvaluationResult evaluation)
    {
        ArgumentNullException.ThrowIfNull(
            evaluation);

        List<MetadataConsensusContribution> contributions =
            new();

        if (evaluation.IsUsable)
        {
            AddCandidateContributions(
                contributions,
                evaluation);
        }

        return contributions;
    }

    private void AddCandidateContributions(
        ICollection<MetadataConsensusContribution> contributions,
        MetadataCandidateEvaluationResult evaluation)
    {
        MetadataCandidate candidate =
            evaluation.Candidate;

        AddContribution(
            contributions,
            evaluation,
            MetadataField.Artist,
            candidate.Artist);

        AddContribution(
            contributions,
            evaluation,
            MetadataField.Title,
            candidate.Title);

        AddContribution(
            contributions,
            evaluation,
            MetadataField.Version,
            candidate.Version);

        AddContribution(
            contributions,
            evaluation,
            MetadataField.Album,
            candidate.ReleaseTitle);

        AddContribution(
            contributions,
            evaluation,
            MetadataField.Genre,
            candidate.Genre);

        AddContribution(
            contributions,
            evaluation,
            MetadataField.Label,
            candidate.Label);
    }

    private void AddContribution(
        ICollection<MetadataConsensusContribution> contributions,
        MetadataCandidateEvaluationResult evaluation,
        MetadataField field,
        string? value)
    {
        string normalizedValue =
            _valueNormalizer.Normalize(
                field,
                value);

        if (string.IsNullOrWhiteSpace(
                normalizedValue))
        {
            return;
        }

        MetadataCandidate candidate =
            evaluation.Candidate;

        contributions.Add(
            new MetadataConsensusContribution
            {
                Field =
                    field,

                Value =
                    value!.Trim(),

                NormalizedValue =
                    normalizedValue,

                SourceName =
                    NormalizeText(
                        candidate.SourceName),

                SourceId =
                    NormalizeText(
                        candidate.SourceId),

                SourceRank =
                    candidate.SourceRank,

                CandidateConfidence =
                    evaluation.RankingScore,

                SourceWeight =
                    GetDefaultSourceWeight(
                        candidate.SourceName,
                        field),

                RequiresManualApproval =
                    RequiresManualApproval(
                        candidate.SourceName)
            });
    }

    /// <summary>
    /// Asigna pesos iniciales según la fuente y el campo.
    ///
    /// Estos valores quedarán encapsulados aquí sólo durante esta
    /// primera fase. En el Milestone 8.4 se extraerán a un
    /// proveedor configurable.
    /// </summary>
    private static double GetDefaultSourceWeight(
        string? sourceName,
        MetadataField field)
    {
        string normalizedSource =
            NormalizeText(
                sourceName);

        return normalizedSource.ToUpperInvariant() switch
        {
            "DISCOGS" =>
                GetDiscogsWeight(
                    field),

            "BEATPORT" =>
                GetBeatportWeight(
                    field),

            "SPOTIFY" =>
                GetSpotifyWeight(
                    field),

            "SOUNDCLOUD" =>
                GetSoundCloudWeight(
                    field),

            _ =>
                0.50
        };
    }

    private static double GetDiscogsWeight(
        MetadataField field)
    {
        return field switch
        {
            MetadataField.Artist =>
                0.95,

            MetadataField.Title =>
                0.95,

            MetadataField.Version =>
                0.85,

            MetadataField.Album =>
                1.00,

            MetadataField.Genre =>
                0.90,

            MetadataField.Label =>
                1.00,

            _ =>
                0.75
        };
    }

    private static double GetBeatportWeight(
        MetadataField field)
    {
        return field switch
        {
            MetadataField.Artist =>
                0.95,

            MetadataField.Title =>
                0.95,

            MetadataField.Version =>
                1.00,

            MetadataField.Album =>
                0.90,

            MetadataField.Genre =>
                1.00,

            MetadataField.Label =>
                0.95,

            _ =>
                0.75
        };
    }

    private static double GetSpotifyWeight(
        MetadataField field)
    {
        return field switch
        {
            MetadataField.Artist =>
                0.90,

            MetadataField.Title =>
                0.90,

            MetadataField.Version =>
                0.75,

            MetadataField.Album =>
                0.85,

            MetadataField.Genre =>
                0.55,

            MetadataField.Label =>
                0.60,

            _ =>
                0.70
        };
    }

    private static double GetSoundCloudWeight(
        MetadataField field)
    {
        return field switch
        {
            MetadataField.Artist =>
                0.70,

            MetadataField.Title =>
                0.70,

            MetadataField.Version =>
                0.70,

            MetadataField.Album =>
                0.45,

            MetadataField.Genre =>
                0.50,

            MetadataField.Label =>
                0.40,

            _ =>
                0.50
        };
    }

    private static bool RequiresManualApproval(
        string? sourceName)
    {
        return string.Equals(
            NormalizeText(
                sourceName),
            "SoundCloud",
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
                ? string.Empty
                : value.Trim();
    }
}