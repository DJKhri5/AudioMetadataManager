using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Quality;

/// <summary>
/// Interpreta la medición objetiva de extensión espectral.
///
/// Esta regla no abre el archivo, no procesa PCM y no
/// ejecuta una nueva FFT. Consume exclusivamente la medición
/// ya publicada en AudioAnalysisContext.
/// </summary>
public class SpectrumCutoffRule :
    IAudioQualityRule
{
    /// <summary>
    /// Cobertura mínima considerada claramente limitada.
    /// </summary>
    private const double StrongLimitationCoverageThreshold =
        0.60;

    /// <summary>
    /// Cobertura mínima considerada moderadamente limitada.
    /// </summary>
    private const double ModerateLimitationCoverageThreshold =
        0.72;

    /// <summary>
    /// Proporción mínima de Nyquist que debe alcanzar el
    /// contenido persistente para evitar conclusiones basadas
    /// únicamente en el roll-off estimado.
    /// </summary>
    private const double PersistentContentProtectionThreshold =
        0.82;

    /// <summary>
    /// Nombre legible de la regla.
    /// </summary>
    public string Name =>
        "Extensión espectral superior";

    /// <summary>
    /// Orden de ejecución dentro del motor de calidad.
    /// </summary>
    public int Order =>
        200;

    /// <summary>
    /// La regla resulta aplicable cuando existe una medición
    /// espectral confiable y utilizable.
    /// </summary>
    public bool IsApplicable(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        return
            context.AnalysisResult
                .SpectrumCutoff
                .HasComparisonData;
    }

    /// <summary>
    /// Evalúa la extensión espectral sin atribuir todavía
    /// un formato fuente ni confirmar una transcodificación.
    /// </summary>
    public AudioQualityRuleResult Evaluate(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioSpectrumCutoffMeasurement measurement =
            context.AnalysisResult.SpectrumCutoff;

        if (!measurement.HasComparisonData)
        {
            return new AudioQualityRuleResult
            {
                RuleName =
                    Name,

                IsApplicable =
                    false,

                SuggestedStatus =
                    AudioQualityAssessmentStatus.InsufficientData,

                Summary =
                    "No existen mediciones espectrales " +
                    "suficientes para evaluar la extensión superior."
            };
        }

        double persistentCoverage =
            GetCoverageRatio(
                measurement.HighestPersistentFrequencyHz,
                measurement.NyquistFrequencyHz);

        bool hasPersistentContentProtection =
            persistentCoverage >=
            PersistentContentProtectionThreshold;

        bool hasStrongLimitation =
            measurement.NyquistCoverageRatio <
                StrongLimitationCoverageThreshold &&
            !hasPersistentContentProtection;

        bool hasModerateLimitation =
            measurement.NyquistCoverageRatio <
                ModerateLimitationCoverageThreshold &&
            !hasPersistentContentProtection;

        AudioQualityRuleResult result =
            new()
            {
                RuleName =
                    Name,

                IsApplicable =
                    true
            };

        if (hasStrongLimitation)
        {
            result.AddIssue(
                AudioQualityIssueType.LimitedSpectralExtension);

            result.AddIssue(
                AudioQualityIssueType.SuspiciousHighFrequencyCutoff);

            return CopyWithConclusion(
                result,
                AudioQualityAssessmentStatus.Suspicious,
                "La extensión espectral superior presenta " +
                "una limitación marcada.");
        }

        if (hasModerateLimitation)
        {
            result.AddIssue(
                AudioQualityIssueType.LimitedSpectralExtension);

            return CopyWithConclusion(
                result,
                AudioQualityAssessmentStatus.SlightlySuspicious,
                "La extensión espectral superior presenta " +
                "una limitación moderada.");
        }

        return CopyWithConclusion(
            result,
            AudioQualityAssessmentStatus.Consistent,
            hasPersistentContentProtection
                ? "Existe contenido persistente suficiente " +
                  "en la zona superior del espectro."
                : "La extensión espectral disponible no presenta " +
                  "una limitación clara.");
    }

    /// <summary>
    /// Calcula la proporción de Nyquist alcanzada por una
    /// frecuencia observada.
    /// </summary>
    private static double GetCoverageRatio(
        double frequencyHz,
        double nyquistFrequencyHz)
    {
        if (!IsPositiveFinite(
                frequencyHz) ||
            !IsPositiveFinite(
                nyquistFrequencyHz))
        {
            return 0;
        }

        return Math.Clamp(
            frequencyHz /
            nyquistFrequencyHz,
            0,
            1);
    }

    /// <summary>
    /// Crea el resultado definitivo conservando las
    /// incidencias ya agregadas.
    /// </summary>
    private static AudioQualityRuleResult CopyWithConclusion(
        AudioQualityRuleResult source,
        AudioQualityAssessmentStatus status,
        string summary)
    {
        AudioQualityRuleResult result =
            new()
            {
                RuleName =
                    source.RuleName,

                IsApplicable =
                    source.IsApplicable,

                SuggestedStatus =
                    status,

                Summary =
                    summary
            };

        foreach (
            AudioQualityIssueType issue
            in source.Issues)
        {
            result.AddIssue(
                issue);
        }

        return result;
    }

    /// <summary>
    /// Comprueba si un valor es positivo y finito.
    /// </summary>
    private static bool IsPositiveFinite(
        double value)
    {
        return value > 0 &&
            !double.IsNaN(
                value) &&
            !double.IsInfinity(
                value);
    }
}