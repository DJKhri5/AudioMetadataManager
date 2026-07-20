using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Quality;

/// <summary>
/// Compara el bitrate medio informado por el archivo con la
/// extensión espectral previamente medida.
///
/// Esta regla no abre el archivo, no procesa PCM y no ejecuta
/// una nueva FFT.
///
/// Su conclusión representa una coherencia técnica, no una
/// confirmación definitiva de transcodificación.
/// </summary>
public class DeclaredBitrateConsistencyRule :
    IAudioQualityRule
{
    /// <summary>
    /// Bitrate mínimo a partir del cual una extensión
    /// persistente muy reducida resulta relevante.
    /// </summary>
    private const double HighBitrateThresholdKbps =
        256.0;

    /// <summary>
    /// Bitrate mínimo correspondiente a la categoría
    /// intermedia evaluada por esta primera versión.
    /// </summary>
    private const double MediumBitrateThresholdKbps =
        192.0;

    /// <summary>
    /// Cobertura persistente mínima esperada para un MP3
    /// de bitrate alto.
    /// </summary>
    private const double HighBitratePersistentCoverageMinimum =
        0.78;

    /// <summary>
    /// Cobertura persistente mínima esperada para un MP3
    /// de bitrate medio.
    /// </summary>
    private const double MediumBitratePersistentCoverageMinimum =
        0.70;

    /// <summary>
    /// Cobertura que representa una incompatibilidad marcada
    /// cuando el archivo informa un bitrate alto.
    /// </summary>
    private const double StrongMismatchCoverageThreshold =
        0.68;

    /// <summary>
    /// Nombre legible de la regla.
    /// </summary>
    public string Name =>
        "Coherencia entre bitrate y extensión espectral";

    /// <summary>
    /// Orden de ejecución dentro del motor de calidad.
    /// </summary>
    public int Order =>
        300;

    /// <summary>
    /// La regla se aplica únicamente a archivos MP3 con
    /// información técnica y espectral utilizable.
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

        bool hasUsableBitrate =
            formatInfo.HasDeclaredBitrate ||
            formatInfo.HasEstimatedAverageBitrate;

        if (!hasUsableBitrate)
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

        return
            context.AnalysisResult
                .SpectrumCutoff
                .HasComparisonData;
    }

    /// <summary>
    /// Compara el bitrate medio informado con la frecuencia
    /// persistente más alta observada por el análisis FFT.
    /// </summary>
    public AudioQualityRuleResult Evaluate(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioTechnicalFormatInfo? formatInfo =
            context.TechnicalFormatInfo;

        AudioSpectrumCutoffMeasurement measurement =
            context.AnalysisResult.SpectrumCutoff;

        if (formatInfo is null ||
            !IsApplicable(context))
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
                    "No existen datos suficientes para comparar " +
                    "el bitrate y la extensión espectral."
            };
        }

        bool usesDeclaredBitrate =
            formatInfo.HasDeclaredBitrate;

        double bitrateKbps =
            usesDeclaredBitrate
                ? formatInfo.DeclaredBitrateKbps
                : formatInfo.EstimatedAverageBitrateKbps;

        string bitrateSourceDisplay =
            usesDeclaredBitrate
                ? "Bitrate declarado"
                : "Bitrate medio estimado";

        double persistentCoverage =
            CalculateCoverage(
                measurement.HighestPersistentFrequencyHz,
                measurement.NyquistFrequencyHz);

        if (bitrateKbps < MediumBitrateThresholdKbps)
        {
            return CreateResult(
                AudioQualityAssessmentStatus.Consistent,
                Array.Empty<AudioQualityIssueType>(),
                $"Bitrate medio aproximado: " +
                $"{bitrateKbps:0} kbps · " +
                "La primera versión de la regla no aplica " +
                "restricciones fuertes a bitrates inferiores " +
                "a 192 kbps.");
        }

        if (bitrateKbps >= HighBitrateThresholdKbps)
        {
            if (persistentCoverage <
                StrongMismatchCoverageThreshold)
            {
                return CreateResult(
                    AudioQualityAssessmentStatus.Suspicious,
                    new[]
                    {
                        AudioQualityIssueType.DeclaredBitrateMismatch,
                        AudioQualityIssueType.LimitedSpectralExtension
                    },
                    BuildMismatchSummary(
                        bitrateSourceDisplay,
                        bitrateKbps,
                        persistentCoverage,
                        "La extensión persistente resulta " +
                        "marcadamente reducida para el bitrate medio."));
            }

            if (persistentCoverage <
                HighBitratePersistentCoverageMinimum)
            {
                return CreateResult(
                    AudioQualityAssessmentStatus.SlightlySuspicious,
                    new[]
                    {
                        AudioQualityIssueType.DeclaredBitrateMismatch
                    },
                    BuildMismatchSummary(
                        bitrateSourceDisplay,
                        bitrateKbps,
                        persistentCoverage,
                        "La extensión persistente es inferior a la " +
                        "esperada para un bitrate alto."));
            }

            return CreateResult(
                AudioQualityAssessmentStatus.Consistent,
                Array.Empty<AudioQualityIssueType>(),
                BuildConsistentSummary(
                    bitrateSourceDisplay,
                    bitrateKbps,
                    persistentCoverage));
        }

        if (persistentCoverage <
            MediumBitratePersistentCoverageMinimum)
        {
            return CreateResult(
                AudioQualityAssessmentStatus.SlightlySuspicious,
                new[]
                {
                    AudioQualityIssueType.DeclaredBitrateMismatch
                },
                BuildMismatchSummary(
                    bitrateSourceDisplay,
                    bitrateKbps,
                    persistentCoverage,
                    "La extensión persistente es reducida para " +
                    "el bitrate medio informado."));
        }

        return CreateResult(
            AudioQualityAssessmentStatus.Consistent,
            Array.Empty<AudioQualityIssueType>(),
            BuildConsistentSummary(
                bitrateSourceDisplay,
                bitrateKbps,
                persistentCoverage));
    }

    /// <summary>
    /// Construye un resultado y agrega las incidencias
    /// indicadas sin duplicarlas.
    /// </summary>
    private AudioQualityRuleResult CreateResult(
        AudioQualityAssessmentStatus status,
        IEnumerable<AudioQualityIssueType> issues,
        string summary)
    {
        AudioQualityRuleResult result =
            new()
            {
                RuleName =
                    Name,

                IsApplicable =
                    true,

                SuggestedStatus =
                    status,

                Summary =
                    summary
            };

        foreach (
            AudioQualityIssueType issue
            in issues)
        {
            result.AddIssue(
                issue);
        }

        return result;
    }

    /// <summary>
    /// Calcula la cobertura de Nyquist alcanzada por una
    /// frecuencia observada.
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
    /// Construye el resumen correspondiente a una posible
    /// incoherencia.
    /// </summary>
    private static string BuildMismatchSummary(
    string bitrateSourceDisplay,
    double bitrateKbps,
    double persistentCoverage,
    string conclusion)
    {
        return
            $"{bitrateSourceDisplay}: " +
            $"{bitrateKbps:0} kbps · " +
            $"Cobertura espectral persistente: " +
            $"{persistentCoverage * 100.0:0.00}% · " +
            $"{conclusion}";
    }

    /// <summary>
    /// Construye el resumen correspondiente a mediciones
    /// técnicamente compatibles.
    /// </summary>
    private static string BuildConsistentSummary(
    string bitrateSourceDisplay,
    double bitrateKbps,
    double persistentCoverage)
    {
        return
            $"{bitrateSourceDisplay}: " +
            $"{bitrateKbps:0} kbps · " +
            $"Cobertura espectral persistente: " +
            $"{persistentCoverage * 100.0:0.00}% · " +
            "No se observa una incompatibilidad clara entre " +
            "el bitrate y la extensión espectral.";
    }

    /// <summary>
    /// Comprueba que un valor sea positivo y finito.
    /// </summary>
    private static bool IsPositiveFinite(
        double value)
    {
        return value > 0 &&
            !double.IsNaN(value) &&
            !double.IsInfinity(value);
    }
}