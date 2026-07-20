using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Quality;

/// <summary>
/// Comprueba la coherencia básica entre las mediciones
/// técnicas ya producidas por el motor de análisis.
///
/// Esta regla no abre el archivo, no procesa PCM y no
/// ejecuta una nueva FFT.
/// </summary>
public class MetadataConsistencyRule :
    IAudioQualityRule
{
    /// <summary>
    /// Diferencia máxima aceptada entre las duraciones
    /// técnicas producidas por distintos módulos.
    /// </summary>
    private static readonly TimeSpan
        MaximumDurationDifference =
            TimeSpan.FromMilliseconds(
                100);

    /// <summary>
    /// Nombre legible de la regla.
    /// </summary>
    public string Name =>
        "Consistencia de mediciones técnicas";

    /// <summary>
    /// Orden de ejecución dentro del motor de calidad.
    /// </summary>
    public int Order =>
        100;

    /// <summary>
    /// La regla resulta aplicable cuando existe al menos
    /// un resultado técnico completado.
    /// </summary>
    public bool IsApplicable(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioAnalysisResult analysisResult =
            context.AnalysisResult;

        return
            analysisResult.Silence.AnalysisCompleted ||
            analysisResult.Envelope.AnalysisCompleted ||
            analysisResult.Spectrum.AnalysisCompleted;
    }

    /// <summary>
    /// Comprueba que los módulos técnicos hayan finalizado
    /// correctamente y que sus duraciones sean coherentes.
    /// </summary>
    public AudioQualityRuleResult Evaluate(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioAnalysisResult analysisResult =
            context.AnalysisResult;

        AudioQualityRuleResult result =
            new()
            {
                RuleName =
                    Name,

                IsApplicable =
                    true,

                SuggestedStatus =
                    AudioQualityAssessmentStatus.Consistent
            };

        List<TimeSpan> technicalDurations =
            new();

        bool hasIncompleteModules =
            false;

        CollectSilenceResult(
            analysisResult.Silence,
            technicalDurations,
            ref hasIncompleteModules);

        CollectEnvelopeResult(
            analysisResult.Envelope,
            technicalDurations,
            ref hasIncompleteModules);

        CollectSpectrumResult(
            analysisResult.Spectrum,
            technicalDurations,
            ref hasIncompleteModules);

        if (hasIncompleteModules)
        {
            result.AddIssue(
                AudioQualityIssueType.TechnicalMetadataMismatch);
        }

        bool hasDurationMismatch =
            HasDurationMismatch(
                technicalDurations);

        if (hasDurationMismatch)
        {
            result.AddIssue(
                AudioQualityIssueType.TechnicalMetadataMismatch);
        }

        if (!result.HasIssues)
        {
            return new AudioQualityRuleResult
            {
                RuleName =
                    Name,

                IsApplicable =
                    true,

                SuggestedStatus =
                    AudioQualityAssessmentStatus.Consistent,

                Summary =
                    "Las mediciones técnicas disponibles " +
                    "son coherentes entre sí."
            };
        }

        return new AudioQualityRuleResultBuilder(
            result)
            .WithStatus(
                hasDurationMismatch
                    ? AudioQualityAssessmentStatus.Suspicious
                    : AudioQualityAssessmentStatus.SlightlySuspicious)
            .WithSummary(
                BuildSummary(
                    hasIncompleteModules,
                    hasDurationMismatch))
            .Build();
    }

    /// <summary>
    /// Incorpora el resultado de silencio exterior.
    /// </summary>
    private static void CollectSilenceResult(
        AudioSilenceAnalysisResult silence,
        ICollection<TimeSpan> durations,
        ref bool hasIncompleteModules)
    {
        if (!silence.AnalysisCompleted)
        {
            hasIncompleteModules =
                true;

            return;
        }

        if (silence.TechnicalDuration >
            TimeSpan.Zero)
        {
            durations.Add(
                silence.TechnicalDuration);
        }

        if (silence.HasError)
        {
            hasIncompleteModules =
                true;
        }
    }

    /// <summary>
    /// Incorpora el resultado de envolvente energética.
    /// </summary>
    private static void CollectEnvelopeResult(
        AudioEnvelopeAnalysisResult envelope,
        ICollection<TimeSpan> durations,
        ref bool hasIncompleteModules)
    {
        if (!envelope.AnalysisCompleted)
        {
            hasIncompleteModules =
                true;

            return;
        }

        if (envelope.TechnicalDuration >
            TimeSpan.Zero)
        {
            durations.Add(
                envelope.TechnicalDuration);
        }

        if (envelope.HasError)
        {
            hasIncompleteModules =
                true;
        }
    }

    /// <summary>
    /// Incorpora el resultado espectral.
    /// </summary>
    private static void CollectSpectrumResult(
        AudioSpectrumAnalysisResult spectrum,
        ICollection<TimeSpan> durations,
        ref bool hasIncompleteModules)
    {
        if (!spectrum.AnalysisCompleted)
        {
            hasIncompleteModules =
                true;

            return;
        }

        if (spectrum.TechnicalDuration >
            TimeSpan.Zero)
        {
            durations.Add(
                spectrum.TechnicalDuration);
        }

        if (spectrum.HasError)
        {
            hasIncompleteModules =
                true;
        }
    }

    /// <summary>
    /// Comprueba si las duraciones técnicas presentan una
    /// diferencia superior al margen permitido.
    /// </summary>
    private static bool HasDurationMismatch(
        IReadOnlyCollection<TimeSpan> durations)
    {
        if (durations.Count < 2)
        {
            return false;
        }

        TimeSpan minimum =
            durations.Min();

        TimeSpan maximum =
            durations.Max();

        return
            maximum -
            minimum >
            MaximumDurationDifference;
    }

    /// <summary>
    /// Construye un resumen de las incoherencias encontradas.
    /// </summary>
    private static string BuildSummary(
        bool hasIncompleteModules,
        bool hasDurationMismatch)
    {
        List<string> details =
            new();

        if (hasIncompleteModules)
        {
            details.Add(
                "Existen módulos técnicos incompletos " +
                "o con errores");
        }

        if (hasDurationMismatch)
        {
            details.Add(
                "Las duraciones técnicas superan el " +
                "margen de diferencia permitido");
        }

        return string.Join(
            " · ",
            details);
    }

    /// <summary>
    /// Constructor interno utilizado para completar un
    /// resultado parcial conservando sus incidencias.
    /// </summary>
    private sealed class AudioQualityRuleResultBuilder
    {
        private readonly AudioQualityRuleResult _source;

        private AudioQualityAssessmentStatus _status;

        private string _summary =
            string.Empty;

        public AudioQualityRuleResultBuilder(
            AudioQualityRuleResult source)
        {
            _source =
                source ??
                throw new ArgumentNullException(
                    nameof(source));

            _status =
                source.SuggestedStatus;
        }

        public AudioQualityRuleResultBuilder WithStatus(
            AudioQualityAssessmentStatus status)
        {
            _status =
                status;

            return this;
        }

        public AudioQualityRuleResultBuilder WithSummary(
            string summary)
        {
            _summary =
                summary ??
                string.Empty;

            return this;
        }

        public AudioQualityRuleResult Build()
        {
            AudioQualityRuleResult result =
                new()
                {
                    RuleName =
                        _source.RuleName,

                    IsApplicable =
                        _source.IsApplicable,

                    SuggestedStatus =
                        _status,

                    Summary =
                        _summary
                };

            foreach (
                AudioQualityIssueType issue
                in _source.Issues)
            {
                result.AddIssue(
                    issue);
            }

            return result;
        }
    }
}