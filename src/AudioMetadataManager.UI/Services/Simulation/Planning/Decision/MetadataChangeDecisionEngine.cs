using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;
using AudioMetadataManager.UI.Services.Parsing;
using ConsensusResult =
    AudioMetadataManager.UI.Services.MetadataSources
        .Consensus.Models.MetadataConsensusResult;

namespace AudioMetadataManager.UI.Services.Simulation
    .Planning.Decision;

/// <summary>
/// Convierte un resultado de consenso en un plan seguro de
/// modificaciones.
///
/// Este motor no escribe etiquetas ni modifica archivos.
/// Sólo genera decisiones auditables para el modo simulación.
/// </summary>
public sealed class MetadataChangeDecisionEngine
{
    private readonly MetadataChangeDecisionOptions
        _options;

    private readonly VersionParser
    _versionParser;

    /// <summary>
    /// Crea el motor con la política y los servicios
    /// predeterminados.
    /// </summary>
    public MetadataChangeDecisionEngine()
        : this(
            new MetadataChangeDecisionOptions(),
            new VersionParser())
    {
    }

    /// <summary>
    /// Crea el motor con una política personalizada y el parser
    /// de versiones predeterminado.
    /// </summary>
    public MetadataChangeDecisionEngine(
        MetadataChangeDecisionOptions options)
        : this(
            options,
            new VersionParser())
    {
    }

    /// <summary>
    /// Crea el motor con todos sus componentes personalizados.
    /// </summary>
    public MetadataChangeDecisionEngine(
        MetadataChangeDecisionOptions options,
        VersionParser versionParser)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _versionParser =
            versionParser ??
            throw new ArgumentNullException(
                nameof(versionParser));

        if (!_options.IsValid)
        {
            throw new ArgumentException(
                "La configuración del motor de decisiones " +
                "contiene valores no válidos.",
                nameof(options));
        }
    }

    /// <summary>
    /// Construye un plan de modificaciones para un archivo.
    /// </summary>
    public MetadataChangePlan BuildPlan(
        AudioFile audioFile,
        ConsensusResult consensusResult)
    {
        ArgumentNullException.ThrowIfNull(
            audioFile);

        ArgumentNullException.ThrowIfNull(
            consensusResult);

        IReadOnlyList<MetadataChangeProposal> proposals =
            consensusResult.Fields
                .Select(
                    field =>
                        BuildProposal(
                            audioFile,
                            field))
                .ToArray();

        MetadataChangePlanStatus status =
            DeterminePlanStatus(
                proposals);

        IReadOnlyList<string> reasons =
            BuildReasons(
                proposals,
                status);

        return new MetadataChangePlan
        {
            FilePath =
                audioFile.FullPath,

            FileName =
                audioFile.FileName,

            Proposals =
                proposals,

            Status =
                status,

            Reasons =
                reasons,

            ArtworkUrl =
                consensusResult.ArtworkUrl,

            ArtworkSourceName =
                consensusResult.ArtworkSourceName
        };
    }

    private MetadataChangeProposal BuildProposal(
        AudioFile audioFile,
        MetadataConsensusFieldResult fieldResult)
    {
        string currentValue =
            ReadCurrentValue(
                audioFile,
                fieldResult.Field);

        string proposedValue =
            NormalizeText(
                fieldResult.SelectedValue);

        bool sourceRequiresManualApproval =
            fieldResult.Contributions.Any(
                contribution =>
                    contribution.RequiresManualApproval);

        MetadataChangeDecision decision =
            DetermineDecision(
                fieldResult,
                currentValue,
                proposedValue,
                sourceRequiresManualApproval);

        IReadOnlyList<string> supportingSources =
            fieldResult.WinningContributions
                .Select(
                    contribution =>
                        contribution.SourceName)
                .Where(
                    sourceName =>
                        !string.IsNullOrWhiteSpace(
                            sourceName))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    sourceName =>
                        sourceName,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        return new MetadataChangeProposal
        {
            Field =
                fieldResult.Field,

            CurrentValue =
                currentValue,

            ProposedValue =
                proposedValue,

            ProposedNormalizedValue =
                fieldResult.SelectedNormalizedValue,

            ConsensusConfidence =
                fieldResult.Confidence,

            SupportingSourceCount =
                supportingSources.Count,

            SupportingSources =
                supportingSources,

            Decision =
                decision,

            Explanation =
                BuildExplanation(
                    fieldResult,
                    decision,
                    currentValue,
                    proposedValue),

            SourceRequiresManualApproval =
                sourceRequiresManualApproval
        };
    }

    private MetadataChangeDecision DetermineDecision(
        MetadataConsensusFieldResult fieldResult,
        string currentValue,
        string proposedValue,
        bool sourceRequiresManualApproval)
    {
        if (fieldResult.Status ==
            MetadataConsensusStatus.Conflict)
        {
            return MetadataChangeDecision.Conflict;
        }

        if (fieldResult.Status is
            MetadataConsensusStatus.NoInformation or
            MetadataConsensusStatus.NotApplicable)
        {
            return
                MetadataChangeDecision.InsufficientEvidence;
        }

        if (string.IsNullOrWhiteSpace(
                proposedValue))
        {
            return
                MetadataChangeDecision.InsufficientEvidence;
        }

        if (AreEquivalent(
                currentValue,
                proposedValue))
        {
            return
                MetadataChangeDecision.NoChangeRequired;
        }

        if (sourceRequiresManualApproval)
        {
            return
                MetadataChangeDecision.ManualReviewRequired;
        }

        bool singleSource =
            fieldResult.Status ==
                MetadataConsensusStatus.SingleSource ||
            fieldResult.ContributingSourceCount < 2;

        if (singleSource &&
            _options.RequireManualReviewForSingleSource)
        {
            return
                fieldResult.Confidence >=
                    _options.ManualReviewConfidenceThreshold
                    ? MetadataChangeDecision
                        .ManualReviewRequired
                    : MetadataChangeDecision
                        .InsufficientEvidence;
        }

        bool criticalField =
            IsCriticalField(
                fieldResult.Field);

        bool hasEnoughSources =
            fieldResult.ContributingSourceCount >=
            _options.MinimumSourcesForAutomaticApply;

        bool automaticConfidence =
            fieldResult.Confidence >=
            _options.AutomaticApplyConfidenceThreshold;

        bool criticalFieldIsSafe =
            !criticalField ||
            !_options.RequireMultipleSourcesForCriticalFields ||
            hasEnoughSources;

        if (automaticConfidence &&
            hasEnoughSources &&
            criticalFieldIsSafe)
        {
            return MetadataChangeDecision
                .EligibleForAutomaticApply;
        }

        if (fieldResult.Confidence >=
            _options.ManualReviewConfidenceThreshold)
        {
            return MetadataChangeDecision
                .ManualReviewRequired;
        }

        return MetadataChangeDecision
            .InsufficientEvidence;
    }

    private static MetadataChangePlanStatus DeterminePlanStatus(
        IReadOnlyList<MetadataChangeProposal> proposals)
    {
        MetadataChangeProposal[] actualChanges =
            proposals
                .Where(
                    proposal =>
                        proposal.HasActualChange)
                .ToArray();

        if (actualChanges.Length == 0)
        {
            return
                MetadataChangePlanStatus.NoChangesRequired;
        }

        bool hasConflicts =
            actualChanges.Any(
                proposal =>
                    proposal.Decision ==
                    MetadataChangeDecision.Conflict);

        if (hasConflicts)
        {
            return
                MetadataChangePlanStatus.BlockedByConflicts;
        }

        int automaticCount =
            actualChanges.Count(
                proposal =>
                    proposal.Decision ==
                    MetadataChangeDecision
                        .EligibleForAutomaticApply);

        int manualCount =
            actualChanges.Count(
                proposal =>
                    proposal.RequiresManualReview);

        if (automaticCount > 0 &&
            manualCount > 0)
        {
            return
                MetadataChangePlanStatus.PartiallyReady;
        }

        if (automaticCount > 0)
        {
            return
                MetadataChangePlanStatus.ReadyForSimulation;
        }

        if (manualCount > 0)
        {
            return
                MetadataChangePlanStatus.ManualReviewRequired;
        }

        return
            MetadataChangePlanStatus.InsufficientEvidence;
    }

    private static IReadOnlyList<string> BuildReasons(
        IReadOnlyList<MetadataChangeProposal> proposals,
        MetadataChangePlanStatus status)
    {
        List<string> reasons =
            new();

        reasons.Add(
            $"Se evaluaron {proposals.Count} campo(s).");

        reasons.Add(
            $"Cambios reales detectados: " +
            $"{proposals.Count(
                proposal =>
                    proposal.HasActualChange)}.");

        reasons.Add(
            $"Cambios automáticos posibles: " +
            $"{proposals.Count(
                proposal =>
                    proposal.IsAutomaticApplyEligible)}.");

        reasons.Add(
            $"Cambios con revisión manual: " +
            $"{proposals.Count(
                proposal =>
                    proposal.RequiresManualReview)}.");

        reasons.Add(
            $"Conflictos detectados: " +
            $"{proposals.Count(
                proposal =>
                    proposal.Decision ==
                    MetadataChangeDecision.Conflict)}.");

        reasons.Add(
            $"Estado global del plan: {status}.");

        return reasons;
    }

    private static string BuildExplanation(
        MetadataConsensusFieldResult fieldResult,
        MetadataChangeDecision decision,
        string currentValue,
        string proposedValue)
    {
        return decision switch
        {
            MetadataChangeDecision.NoChangeRequired =>
                "El valor actual ya coincide con la propuesta " +
                "seleccionada por el consenso.",

            MetadataChangeDecision.EligibleForAutomaticApply =>
                "La propuesta alcanzó la confianza y el número " +
                "de fuentes requeridos para continuar hacia la " +
                "simulación automática.",

            MetadataChangeDecision.ManualReviewRequired =>
                "La propuesta es utilizable, pero todavía " +
                "requiere aprobación manual antes de aplicarse.",

            MetadataChangeDecision.InsufficientEvidence =>
                "No existe evidencia suficiente para reemplazar " +
                "el valor actual de forma segura.",

            MetadataChangeDecision.Conflict =>
                "El consenso detectó valores incompatibles y no " +
                "seleccionó una propuesta segura.",

            MetadataChangeDecision.Rejected =>
                "La propuesta fue descartada por una regla de " +
                "seguridad.",

            _ =>
                $"Valor actual: {DisplayValue(currentValue)}. " +
                $"Valor propuesto: {DisplayValue(proposedValue)}. " +
                $"Estado del consenso: {fieldResult.Status}."
        };
    }

    /// <summary>
    /// Obtiene el valor local equivalente al campo evaluado.
    ///
    /// El título etiquetado se procesa con VersionParser para
    /// separar correctamente el título base de su versión.
    /// </summary>
    private string ReadCurrentValue(
        AudioFile audioFile,
        MetadataField field)
    {
        string taggedTitle =
            NormalizeText(
                audioFile.Title);

        (string parsedTitle, string parsedVersion) =
            _versionParser.Parse(
                taggedTitle);

        return field switch
        {
            MetadataField.Artist =>
                NormalizeText(
                    audioFile.Artist),

            MetadataField.Title =>
                NormalizeText(
                    parsedTitle),

            MetadataField.Version =>
                NormalizeText(
                    parsedVersion),

            MetadataField.Album =>
                NormalizeText(
                    audioFile.Album),

            MetadataField.Genre =>
                NormalizeText(
                    audioFile.Genre),

            MetadataField.Label =>
                string.Empty,

            _ =>
                string.Empty
        };
    }

    private static bool IsCriticalField(
        MetadataField field)
    {
        return field is
            MetadataField.Artist or
            MetadataField.Title or
            MetadataField.Version;
    }

    private static bool AreEquivalent(
        string? firstValue,
        string? secondValue)
    {
        return string.Equals(
            NormalizeText(
                firstValue),
            NormalizeText(
                secondValue),
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

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
                value)
            ? "(sin información)"
            : value.Trim();
    }
}