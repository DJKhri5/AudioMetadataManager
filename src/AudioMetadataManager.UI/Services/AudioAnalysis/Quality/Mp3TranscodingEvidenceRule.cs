using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Quality;

/// <summary>
/// Busca una combinación de indicios compatible con una
/// posible transcodificación de un MP3 de bitrate alto.
///
/// Esta regla no afirma el origen exacto del archivo y no
/// identifica por sí sola una fuente como YouTube o Spotify.
///
/// La conclusión se obtiene únicamente cuando coinciden
/// varias limitaciones espectrales independientes.
/// </summary>
public class Mp3TranscodingEvidenceRule :
    IAudioQualityRule
{
    /// <summary>
    /// Bitrate mínimo evaluado por esta primera versión.
    /// </summary>
    private const double MinimumEvaluatedBitrateKbps =
        256.0;

    /// <summary>
    /// Cobertura persistente reducida para un MP3 de
    /// bitrate alto.
    /// </summary>
    private const double PersistentCoverageThreshold =
        0.80;

    /// <summary>
    /// Cobertura de persistencia fuerte considerada reducida.
    /// </summary>
    private const double StrongPersistentCoverageThreshold =
        0.65;

    /// <summary>
    /// Cobertura de la caída superior considerada marcada.
    /// </summary>
    private const double EstimatedCutoffCoverageThreshold =
        0.60;

    /// <summary>
    /// Cantidad mínima de señales coincidentes necesarias
    /// para sugerir una probable transcodificación.
    /// </summary>
    private const int MinimumEvidenceCount =
        2;

    /// <summary>
    /// Nombre legible de la regla.
    /// </summary>
    public string Name =>
        "Evidencia combinada de transcodificación MP3";

    /// <summary>
    /// Se ejecuta después de las reglas generales de
    /// consistencia espectral y bitrate.
    /// </summary>
    public int Order =>
        400;

    /// <summary>
    /// Comprueba si existen datos suficientes para ejecutar
    /// esta regla.
    /// </summary>
    public bool IsApplicable(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioTechnicalFormatInfo? formatInfo =
            context.TechnicalFormatInfo;

        if (formatInfo is null ||
            !formatInfo.IsValid)
        {
            return false;
        }

        if (!string.Equals(
                formatInfo.FileExtension,
                ".mp3",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        double bitrateKbps =
            GetUsableBitrateKbps(
                formatInfo);

        if (bitrateKbps <
            MinimumEvaluatedBitrateKbps)
        {
            return false;
        }

        return context.AnalysisResult
            .SpectrumCutoff
            .HasComparisonData;
    }

    /// <summary>
    /// Evalúa tres señales espectrales independientes:
    ///
    /// - extensión persistente;
    /// - extensión con persistencia fuerte;
    /// - caída superior estimada.
    /// </summary>
    public AudioQualityRuleResult Evaluate(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        if (!IsApplicable(context))
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
                    "No existen datos suficientes para evaluar " +
                    "una posible transcodificación MP3."
            };
        }

        AudioTechnicalFormatInfo formatInfo =
            context.TechnicalFormatInfo!;

        AudioSpectrumCutoffMeasurement measurement =
            context.AnalysisResult.SpectrumCutoff;

        double bitrateKbps =
            GetUsableBitrateKbps(
                formatInfo);

        double persistentCoverage =
            CalculateCoverage(
                measurement.HighestPersistentFrequencyHz,
                measurement.NyquistFrequencyHz);

        double strongPersistentCoverage =
            CalculateCoverage(
                measurement.HighestStrongPersistentFrequencyHz,
                measurement.NyquistFrequencyHz);

        double estimatedCutoffCoverage =
            CalculateCoverage(
                measurement.EstimatedCutoffFrequencyHz,
                measurement.NyquistFrequencyHz);

        List<string> evidence =
            new();

        if (persistentCoverage <
            PersistentCoverageThreshold)
        {
            evidence.Add(
                "extensión persistente reducida");
        }

        if (strongPersistentCoverage <
            StrongPersistentCoverageThreshold)
        {
            evidence.Add(
                "persistencia fuerte limitada");
        }

        if (estimatedCutoffCoverage <
            EstimatedCutoffCoverageThreshold)
        {
            evidence.Add(
                "caída superior marcada");
        }

        if (evidence.Count <
            MinimumEvidenceCount)
        {
            return CreateConsistentResult(
                bitrateKbps,
                persistentCoverage,
                strongPersistentCoverage,
                estimatedCutoffCoverage,
                evidence.Count);
        }

        return CreateSuspiciousResult(
            bitrateKbps,
            persistentCoverage,
            strongPersistentCoverage,
            estimatedCutoffCoverage,
            evidence);
    }

    /// <summary>
    /// Construye el resultado cuando coinciden suficientes
    /// indicios técnicos.
    /// </summary>
    private AudioQualityRuleResult CreateSuspiciousResult(
        double bitrateKbps,
        double persistentCoverage,
        double strongPersistentCoverage,
        double estimatedCutoffCoverage,
        IReadOnlyCollection<string> evidence)
    {
        AudioQualityRuleResult result =
            new()
            {
                RuleName =
                    Name,

                IsApplicable =
                    true,

                SuggestedStatus =
                    AudioQualityAssessmentStatus.LikelyTranscoded,

                Summary =
                    $"Bitrate evaluado: {bitrateKbps:0} kbps · " +
                    $"Cobertura persistente: " +
                    $"{FormatPercentage(persistentCoverage)} · " +
                    $"Cobertura de persistencia fuerte: " +
                    $"{FormatPercentage(strongPersistentCoverage)} · " +
                    $"Cobertura de caída estimada: " +
                    $"{FormatPercentage(estimatedCutoffCoverage)} · " +
                    $"Indicios coincidentes: {evidence.Count} " +
                    $"({string.Join(", ", evidence)}) · " +
                    "El comportamiento es compatible con una " +
                    "posible transcodificación desde una fuente " +
                    "de menor calidad. Se recomienda revisión manual."
            };

        result.AddIssue(
            AudioQualityIssueType.PossibleRecompression);

        result.AddIssue(
            AudioQualityIssueType.DeclaredBitrateMismatch);

        result.AddIssue(
            AudioQualityIssueType.SuspiciousHighFrequencyCutoff);

        return result;
    }

    /// <summary>
    /// Construye el resultado cuando las señales no son
    /// suficientes para sugerir una transcodificación.
    /// </summary>
    private AudioQualityRuleResult CreateConsistentResult(
        double bitrateKbps,
        double persistentCoverage,
        double strongPersistentCoverage,
        double estimatedCutoffCoverage,
        int evidenceCount)
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
                $"Bitrate evaluado: {bitrateKbps:0} kbps · " +
                $"Cobertura persistente: " +
                $"{FormatPercentage(persistentCoverage)} · " +
                $"Cobertura de persistencia fuerte: " +
                $"{FormatPercentage(strongPersistentCoverage)} · " +
                $"Cobertura de caída estimada: " +
                $"{FormatPercentage(estimatedCutoffCoverage)} · " +
                $"Indicios coincidentes: {evidenceCount} · " +
                "No existe evidencia combinada suficiente para " +
                "sugerir una transcodificación."
        };
    }

    /// <summary>
    /// Obtiene el bitrate declarado y utiliza el estimado
    /// solamente cuando el primero no está disponible.
    /// </summary>
    private static double GetUsableBitrateKbps(
        AudioTechnicalFormatInfo formatInfo)
    {
        if (formatInfo.HasDeclaredBitrate)
        {
            return formatInfo.DeclaredBitrateKbps;
        }

        return formatInfo.HasEstimatedAverageBitrate
            ? formatInfo.EstimatedAverageBitrateKbps
            : 0;
    }

    /// <summary>
    /// Calcula la cobertura relativa respecto de Nyquist.
    /// </summary>
    private static double CalculateCoverage(
        double frequencyHz,
        double nyquistFrequencyHz)
    {
        if (!IsPositiveFinite(frequencyHz) ||
            !IsPositiveFinite(nyquistFrequencyHz))
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
    /// Formatea una cobertura como porcentaje.
    /// </summary>
    private static string FormatPercentage(
        double value)
    {
        return
            $"{Math.Clamp(value, 0, 1) * 100.0:0.00}%";
    }

    /// <summary>
    /// Comprueba si un número es positivo y finito.
    /// </summary>
    private static bool IsPositiveFinite(
        double value)
    {
        return value > 0 &&
            !double.IsNaN(value) &&
            !double.IsInfinity(value);
    }
}