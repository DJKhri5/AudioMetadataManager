using AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;
using AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Reporting;

/// <summary>
/// Convierte un AudioAnalysisResult en un informe legible.
///
/// Esta clase no analiza archivos ni modifica audio.
/// Su única responsabilidad es presentar los resultados
/// producidos por AudioAnalysisEngine.
/// </summary>
public class AudioAnalysisReportBuilder
{
    /// <summary>
    /// Construye el informe técnico completo.
    /// </summary>
    public string Build(
        AudioAnalysisResult analysisResult)
    {
        ArgumentNullException.ThrowIfNull(
            analysisResult);

        List<string> lines = new();

        AddHeader(
            lines,
            analysisResult);

        AddGlobalStatus(
            lines,
            analysisResult);

        AddTechnicalFormatSection(
            lines,
            analysisResult.TechnicalFormat);

        AddSilenceSection(
            lines,
            analysisResult.Silence);

        AddEnvelopeSection(
            lines,
            analysisResult.Envelope);

        AddSpectrumSection(
            lines,
            analysisResult.Spectrum);

        AddSpectrumCutoffSection(
            lines,
            analysisResult.SpectrumCutoff);

        AddToneProfileSection(
            lines,
            analysisResult.ToneProfile);

        AddToneBalanceSection(
            lines,
            analysisResult.ToneBalanceProfile);

        AddToneCharacterSection(
            lines,
            analysisResult.ToneCharacterResult);

        AddQualitySection(
            lines,
            analysisResult.Quality);

        AddWarningsSection(
            lines,
            analysisResult.Warnings);

        AddSummarySection(
            lines,
            analysisResult);

        lines.Add(string.Empty);
        lines.Add("=== Fin del informe técnico ===");

        return string.Join(
            Environment.NewLine,
            lines);
    }

    /// <summary>
    /// Agrega la identificación principal del archivo.
    /// </summary>
    private static void AddHeader(
        List<string> lines,
        AudioAnalysisResult analysisResult)
    {
        lines.Add(
            "=== Informe técnico del AudioAnalysisEngine ===");

        lines.Add(string.Empty);

        lines.Add(
            $"Archivo: " +
            $"{DisplayValue(analysisResult.FileName)}");

        lines.Add(
            $"Ruta: " +
            $"{DisplayValue(analysisResult.FilePath)}");

        lines.Add(
            $"Inicio: " +
            $"{analysisResult.StartedAt:yyyy-MM-dd HH:mm:ss.fff}");

        lines.Add(
            $"Fin: " +
            $"{FormatCompletedAt(analysisResult.CompletedAt)}");

        lines.Add(
            $"Tiempo empleado: " +
            $"{analysisResult.ElapsedTimeDisplay}");
    }

    /// <summary>
    /// Agrega el estado general del análisis.
    /// </summary>
    private static void AddGlobalStatus(
        List<string> lines,
        AudioAnalysisResult analysisResult)
    {
        lines.Add(string.Empty);
        lines.Add("--- Estado general ---");
        lines.Add(string.Empty);

        lines.Add(
            $"Estado: " +
            $"{analysisResult.StatusDisplay}");

        lines.Add(
            $"Análisis completado: " +
            $"{ToSpanish(analysisResult.AnalysisCompleted)}");

        lines.Add(
            $"Cancelado: " +
            $"{ToSpanish(analysisResult.WasCancelled)}");

        lines.Add(
            $"Error fatal: " +
            $"{ToSpanish(analysisResult.HasFatalError)}");

        lines.Add(
            $"Revisión manual: " +
            $"{ToSpanish(analysisResult.RequiresManualReview)}");

        lines.Add(
            $"Problemas detectados: " +
            $"{ToSpanish(analysisResult.HasProblems)}");

        if (analysisResult.HasFatalError)
        {
            lines.Add(
                $"Detalle del error: " +
                $"{DisplayValue(analysisResult.ErrorMessage)}");
        }
    }

    /// <summary>
    /// Agrega las propiedades técnicas declaradas o identificadas
    /// desde el archivo y su contenedor.
    /// </summary>
    private static void AddTechnicalFormatSection(
        List<string> lines,
        AudioTechnicalFormatInfo technicalFormat)
    {
        lines.Add(string.Empty);
        lines.Add("--- Información técnica del archivo ---");
        lines.Add(string.Empty);

        lines.Add(
            "Información disponible: " +
            $"{ToSpanish(technicalFormat.IsValid)}");

        lines.Add(
            "Extensión: " +
            $"{DisplayValue(technicalFormat.FileExtension)}");

        lines.Add(
            "Contenedor: " +
            $"{DisplayValue(technicalFormat.ContainerName)}");

        lines.Add(
            "Códec: " +
            $"{DisplayValue(technicalFormat.CodecName)}");

        lines.Add(
            "Bitrate declarado: " +
            $"{technicalFormat.DeclaredBitrateDisplay}");

        lines.Add(
            "Bitrate medio estimado: " +
            $"{technicalFormat.EstimatedAverageBitrateDisplay}");

        lines.Add(
            "Frecuencia de muestreo declarada: " +
            $"{FormatIntegerValue(
                technicalFormat.DeclaredSampleRate,
                "Hz")}");

        lines.Add(
            "Canales declarados: " +
            $"{FormatIntegerValue(
                technicalFormat.DeclaredChannels,
                "canal(es)")}");

        lines.Add(
            "Profundidad de bits: " +
            $"{FormatIntegerValue(
                technicalFormat.BitsPerSample,
                "bits")}");

        lines.Add(
            "Formato con pérdida: " +
            $"{ToSpanish(technicalFormat.IsLossy)}");

        lines.Add(
            "Formato sin pérdida: " +
            $"{ToSpanish(technicalFormat.IsLossless)}");

        lines.Add(
            "Resumen: " +
            $"{DisplayValue(technicalFormat.Summary)}");
    }

    /// <summary>
    /// Agrega la sección del análisis de silencio exterior.
    /// </summary>
    private static void AddSilenceSection(
        List<string> lines,
        AudioSilenceAnalysisResult silence)
    {
        lines.Add(string.Empty);
        lines.Add("--- Análisis de silencio exterior ---");
        lines.Add(string.Empty);

        lines.Add(
            $"Estado: " +
            $"{silence.StatusDisplay}");

        lines.Add(
            $"Análisis completado: " +
            $"{ToSpanish(silence.AnalysisCompleted)}");

        lines.Add(
            $"Resultado confiable: " +
            $"{ToSpanish(silence.IsReliable)}");

        lines.Add(
            $"Umbral utilizado: " +
            $"{silence.SilenceThresholdDb:0.##} dBFS");

        lines.Add(
            $"Duración técnica: " +
            $"{silence.TechnicalDurationDisplay}");

        lines.Add(
            $"Duración audible estimada: " +
            $"{silence.AudibleDurationDisplay}");

        lines.Add(
            $"Silencio inicial: " +
            $"{silence.LeadingSilenceDisplay}");

        lines.Add(
            $"Silencio final: " +
            $"{silence.TrailingSilenceDisplay}");

        lines.Add(
            $"Silencio exterior total: " +
            $"{silence.TotalOuterSilenceDisplay}");

        lines.Add(
            $"Porcentaje de silencio exterior: " +
            $"{silence.OuterSilencePercentageDisplay}");

        lines.Add(
            $"Silencio detectado al inicio: " +
            $"{ToSpanish(
                silence.HasLeadingSilence)}");

        lines.Add(
            $"Silencio detectado al final: " +
            $"{ToSpanish(
                silence.HasTrailingSilence)}");

        lines.Add(
            $"Datos disponibles para comparación: " +
            $"{ToSpanish(
                silence.HasComparisonData)}");

        lines.Add(
            $"Resumen: " +
            $"{DisplayValue(silence.Summary)}");

        if (silence.HasError)
        {
            lines.Add(
                $"Error: " +
                $"{DisplayValue(silence.ErrorMessage)}");
        }
    }

    /// <summary>
    /// Agrega la sección del análisis de envolvente energética.
    /// </summary>
    private static void AddEnvelopeSection(
        List<string> lines,
        AudioEnvelopeAnalysisResult envelope)
    {
        lines.Add(string.Empty);
        lines.Add("--- Análisis de envolvente energética ---");
        lines.Add(string.Empty);

        lines.Add(
            $"Estado: " +
            $"{envelope.StatusDisplay}");

        lines.Add(
            $"Análisis completado: " +
            $"{ToSpanish(envelope.AnalysisCompleted)}");

        lines.Add(
            $"Resultado confiable: " +
            $"{ToSpanish(envelope.IsReliable)}");

        lines.Add(
            $"Duración técnica: " +
            $"{envelope.TechnicalDurationDisplay}");

        lines.Add(
            $"Inicio musical estimado: " +
            $"{envelope.EstimatedMusicalStartDisplay}");

        lines.Add(
            $"Final musical estimado: " +
            $"{envelope.EstimatedMusicalEndDisplay}");

        lines.Add(
            $"Duración musical estimada: " +
            $"{envelope.EstimatedMusicalDurationDisplay}");

        lines.Add(
            $"Energía media: " +
            $"{envelope.AverageEnergyDisplay}");

        lines.Add(
            $"Energía máxima: " +
            $"{envelope.PeakEnergyDisplay}");

        lines.Add(
            $"Energía mínima útil: " +
            $"{envelope.MinimumEnergyDisplay}");

        lines.Add(
            $"Ventanas procesadas: " +
            $"{envelope.ProcessedWindowCount}");

        lines.Add(
            $"Duración de ventana: " +
            $"{FormatDuration(envelope.WindowDuration)}");

        lines.Add(
            $"Posible fade-in: " +
            $"{ToSpanish(envelope.HasPossibleFadeIn)}");

        lines.Add(
            $"Posible fade-out: " +
            $"{ToSpanish(envelope.HasPossibleFadeOut)}");

        lines.Add(
            $"Posible cola de reverberación: " +
            $"{ToSpanish(envelope.HasPossibleReverbTail)}");

        lines.Add(
            $"Datos disponibles para comparación: " +
            $"{ToSpanish(envelope.HasComparisonData)}");

        lines.Add(
            $"Resumen: " +
            $"{DisplayValue(envelope.Summary)}");

        if (envelope.HasError)
        {
            lines.Add(
                $"Error: " +
                $"{DisplayValue(envelope.ErrorMessage)}");
        }
    }

    /// <summary>
    /// Agrega la sección del análisis espectral.
    /// </summary>
    private static void AddSpectrumSection(
        List<string> lines,
        AudioSpectrumAnalysisResult spectrum)
    {
        lines.Add(string.Empty);
        lines.Add("--- Análisis espectral FFT ---");
        lines.Add(string.Empty);

        lines.Add(
            $"Estado: " +
            $"{spectrum.StatusDisplay}");

        lines.Add(
            $"Análisis completado: " +
            $"{ToSpanish(spectrum.AnalysisCompleted)}");

        lines.Add(
            $"Resultado confiable: " +
            $"{ToSpanish(spectrum.IsReliable)}");

        lines.Add(
            $"Duración técnica: " +
            $"{spectrum.TechnicalDurationDisplay}");

        lines.Add(
            $"Frecuencia de muestreo: " +
            $"{spectrum.SampleRate} Hz");

        lines.Add(
            $"Frecuencia de Nyquist: " +
            $"{spectrum.NyquistFrequencyDisplay}");

        lines.Add(
            $"Tamaño FFT: " +
            $"{spectrum.FftSize}");

        lines.Add(
            $"Duración de ventana FFT: " +
            $"{FormatDuration(spectrum.WindowDuration)}");

        lines.Add(
            $"Resolución frecuencial: " +
            $"{spectrum.FrequencyResolutionDisplay}");

        lines.Add(
            $"Ventanas FFT procesadas: " +
            $"{spectrum.ProcessedWindowCount}");

        lines.Add(
            $"Frecuencia significativa más alta: " +
            $"{spectrum.HighestSignificantFrequencyDisplay}");

        lines.Add(
            $"Frecuencia persistente más alta: " +
            $"{spectrum.HighestPersistentFrequencyDisplay}");

        lines.Add(
            $"Frecuencia con persistencia fuerte: " +
            $"{spectrum.HighestStrongPersistentFrequencyDisplay}");

        lines.Add(
            $"Caída superior estimada: " +
            $"{spectrum.EstimatedHighFrequencyRolloffDisplay}");

        lines.Add(
            $"Energía espectral media: " +
            $"{spectrum.AverageSpectrumEnergyDisplay}");

        lines.Add(
            $"Energía espectral máxima: " +
            $"{spectrum.PeakSpectrumEnergyDisplay}");

        lines.Add(
            $"Datos disponibles para comparación: " +
            $"{ToSpanish(spectrum.HasComparisonData)}");

        lines.Add(
            $"Resumen: " +
            $"{DisplayValue(spectrum.Summary)}");

        if (spectrum.HasError)
        {
            lines.Add(
                $"Error: " +
                $"{DisplayValue(spectrum.ErrorMessage)}");
        }
    }

    /// <summary>
    /// Formatea una frecuencia para mostrarla en Hz o kHz.
    /// </summary>
    private static string FormatFrequencyValue(
        double frequencyHz)
    {
        if (frequencyHz <= 0 ||
            double.IsNaN(
                frequencyHz) ||
            double.IsInfinity(
                frequencyHz))
        {
            return "Sin información";
        }

        if (frequencyHz >= 1000)
        {
            return
                $"{frequencyHz / 1000.0:0.00} kHz";
        }

        return
            $"{frequencyHz:0.00} Hz";
    }

    /// <summary>
    /// Agrega la medición objetiva de extensión y caída
    /// superior del espectro.
    /// </summary>
    private static void AddSpectrumCutoffSection(
        List<string> lines,
        AudioSpectrumCutoffMeasurement cutoffMeasurement)
    {
        lines.Add(string.Empty);
        lines.Add(
            "--- Medición de corte espectral ---");
        lines.Add(string.Empty);

        lines.Add(
            "Medición completada: " +
            $"{ToSpanish(
                cutoffMeasurement.MeasurementCompleted)}");

        lines.Add(
            "Resultado confiable: " +
            $"{ToSpanish(
                cutoffMeasurement.IsReliable)}");

        lines.Add(
            "Frecuencia de Nyquist: " +
            $"{FormatFrequencyValue(
                cutoffMeasurement.NyquistFrequencyHz)}");

        lines.Add(
            "Frecuencia significativa más alta: " +
            $"{FormatFrequencyValue(
                cutoffMeasurement.HighestSignificantFrequencyHz)}");

        lines.Add(
            "Frecuencia persistente más alta: " +
            $"{cutoffMeasurement.HighestPersistentFrequencyDisplay}");

        lines.Add(
            "Frecuencia con persistencia fuerte: " +
            $"{cutoffMeasurement.HighestStrongPersistentFrequencyDisplay}");

        lines.Add(
            "Caída superior estimada: " +
            $"{cutoffMeasurement.EstimatedCutoffFrequencyDisplay}");

        lines.Add(
            "Distancia respecto de Nyquist: " +
            $"{FormatFrequencyValue(
                cutoffMeasurement.CutoffDistanceFromNyquistHz)}");

        lines.Add(
            "Cobertura de Nyquist: " +
            $"{cutoffMeasurement.NyquistCoverageDisplay}");

        lines.Add(
            "Datos disponibles para comparación: " +
            $"{ToSpanish(
                cutoffMeasurement.HasComparisonData)}");
    }

    /// <summary>
    /// Agrega el perfil tonal derivado del espectro FFT.
    /// </summary>
    private static void AddToneProfileSection(
        List<string> lines,
        AudioToneProfile toneProfile)
    {
        lines.Add(string.Empty);
        lines.Add("--- Perfil tonal por bandas ---");
        lines.Add(string.Empty);

        lines.Add(
            $"Perfil válido: " +
            $"{ToSpanish(toneProfile.IsValid)}");

        lines.Add(
            $"Bandas disponibles: " +
            $"{toneProfile.BandCount}");

        lines.Add(
            $"Distribución energética normalizada: " +
            $"{ToSpanish(
                toneProfile.HasNormalizedEnergyDistribution)}");

        lines.Add(
            $"Suma de participación energética: " +
            $"{toneProfile.TotalEnergyRatioSumDisplay}");

        lines.Add(
            $"Banda dominante por energía: " +
            $"{toneProfile.DominantEnergyBandDisplay}");

        lines.Add(
            $"Banda más persistente: " +
            $"{toneProfile.MostPersistentBandDisplay}");

        if (!toneProfile.IsValid)
        {
            lines.Add(
                "No existen mediciones tonales válidas.");

            return;
        }

        foreach (
            AudioFrequencyBandMeasurement measurement
            in toneProfile.Measurements)
        {
            lines.Add(string.Empty);

            lines.Add(
                $"[{measurement.DisplayName}]");

            lines.Add(
                $"Rango: " +
                $"{measurement.FrequencyRangeDisplay}");

            lines.Add(
                $"Bins FFT utilizados: " +
                $"{measurement.BinCount}");

            lines.Add(
                $"Magnitud media: " +
                $"{measurement.AverageMagnitudeDisplay}");

            lines.Add(
                $"Magnitud máxima: " +
                $"{measurement.PeakMagnitudeDisplay}");

            lines.Add(
                $"Frecuencia dominante: " +
                $"{measurement.DominantFrequencyDisplay}");

            lines.Add(
                $"Persistencia media: " +
                $"{measurement.AveragePersistenceDisplay}");

            lines.Add(
                $"Persistencia máxima: " +
                $"{measurement.PeakPersistenceDisplay}");

            lines.Add(
                $"Participación energética: " +
                $"{measurement.TotalEnergyRatioDisplay}");
        }
    }

    /// <summary>
    /// Agrega el balance tonal general derivado del perfil
    /// energético por bandas.
    /// </summary>
    private static void AddToneBalanceSection(
        List<string> lines,
        AudioToneBalanceProfile toneBalanceProfile)
    {
        lines.Add(string.Empty);
        lines.Add("--- Balance tonal general ---");
        lines.Add(string.Empty);

        lines.Add(
            $"Perfil válido: " +
            $"{ToSpanish(toneBalanceProfile.IsValid)}");

        lines.Add(
            $"Región baja: " +
            $"{toneBalanceProfile.LowFrequencyEnergyDisplay}");

        lines.Add(
            $"Región media: " +
            $"{toneBalanceProfile.MidFrequencyEnergyDisplay}");

        lines.Add(
            $"Región alta: " +
            $"{toneBalanceProfile.HighFrequencyEnergyDisplay}");

        lines.Add(
            $"Suma energética: " +
            $"{toneBalanceProfile.TotalEnergyRatioDisplay}");

        lines.Add(
            $"Región dominante: " +
            $"{toneBalanceProfile.DominantRegionDisplay}");

        lines.Add(
            $"Relación bajas/medias: " +
            $"{toneBalanceProfile.LowToMidEnergyRatioDisplay}");

        lines.Add(
            $"Relación altas/medias: " +
            $"{toneBalanceProfile.HighToMidEnergyRatioDisplay}");

        lines.Add(
            $"Relación bajas/altas: " +
            $"{toneBalanceProfile.LowToHighEnergyRatioDisplay}");
    }

    /// <summary>
    /// Agrega la caracterización tonal simplificada.
    /// </summary>
    private static void AddToneCharacterSection(
        List<string> lines,
        AudioToneCharacterResult toneCharacterResult)
    {
        lines.Add(string.Empty);
        lines.Add("--- Caracterización tonal ---");
        lines.Add(string.Empty);

        lines.Add(
            $"Caracterización disponible: " +
            $"{ToSpanish(toneCharacterResult.IsValid)}");

        lines.Add(
            $"Carácter principal: " +
            $"{AudioToneCharacterCalculator.GetDisplayName(
                toneCharacterResult.PrimaryCharacter)}");

        if (!toneCharacterResult.HasSecondaryCharacters)
        {
            lines.Add(
                "Caracteres secundarios: Ninguno");

            return;
        }

        string secondaryCharacters =
            string.Join(
                ", ",
                toneCharacterResult.SecondaryCharacters.Select(
                    AudioToneCharacterCalculator.GetDisplayName));

        lines.Add(
            $"Caracteres secundarios: " +
            $"{secondaryCharacters}");
    }

    /// <summary>
    /// Agrega la evaluación técnica producida por el motor
    /// de reglas de calidad.
    /// </summary>
    private static void AddQualitySection(
        List<string> lines,
        AudioQualityAnalysisResult quality)
    {
        lines.Add(string.Empty);
        lines.Add("--- Evaluación técnica de calidad ---");
        lines.Add(string.Empty);

        lines.Add(
            "Evaluación completada: " +
            $"{ToSpanish(quality.AnalysisCompleted)}");

        lines.Add(
            "Evaluación aplicable: " +
            $"{ToSpanish(quality.IsApplicable)}");

        lines.Add(
            "Estado técnico: " +
            $"{AudioQualityAnalyzer.GetStatusDisplayName(
                quality.Status)}");

        lines.Add(
            "Incoherencias detectadas: " +
            $"{quality.IssueCount}");

        if (!quality.HasIssues)
        {
            lines.Add(
                "Tipos de incoherencia: Ninguna");
        }
        else
        {
            string issues =
                string.Join(
                    ", ",
                    quality.Issues.Select(
                        GetQualityIssueDisplayName));

            lines.Add(
                "Tipos de incoherencia: " +
                $"{issues}");
        }

        lines.Add(
            "Resumen: " +
            $"{DisplayValue(quality.Summary)}");

        if (quality.HasError)
        {
            lines.Add(
                "Error: " +
                $"{DisplayValue(quality.ErrorMessage)}");
        }
    }

    /// <summary>
    /// Agrega las advertencias generales registradas
    /// por AudioAnalysisEngine.
    /// </summary>
    private static void AddWarningsSection(
        List<string> lines,
        IReadOnlyCollection<string> warnings)
    {
        lines.Add(string.Empty);
        lines.Add("--- Advertencias generales ---");
        lines.Add(string.Empty);

        if (warnings.Count == 0)
        {
            lines.Add(
                "No se registraron advertencias.");

            return;
        }

        foreach (string warning in warnings)
        {
            if (string.IsNullOrWhiteSpace(warning))
            {
                continue;
            }

            lines.Add(
                $"- {warning.Trim()}");
        }
    }

    /// <summary>
    /// Convierte un tipo de incoherencia técnica en un texto
    /// legible para el informe.
    /// </summary>
    private static string GetQualityIssueDisplayName(
        AudioQualityIssueType issue)
    {
        return issue switch
        {
            AudioQualityIssueType.DeclaredBitrateMismatch =>
                "Bitrate declarado poco coherente",

            AudioQualityIssueType.LimitedSpectralExtension =>
                "Extensión espectral limitada",

            AudioQualityIssueType.PossibleLossySource =>
                "Posible fuente con pérdida",

            AudioQualityIssueType.PossibleRecompression =>
                "Posible recompresión",

            AudioQualityIssueType.TechnicalMetadataMismatch =>
                "Incoherencia entre mediciones técnicas",

            AudioQualityIssueType.SuspiciousHighFrequencyCutoff =>
                "Corte superior potencialmente artificial",

            AudioQualityIssueType.InsufficientEvidence =>
                "Evidencia insuficiente",

            _ =>
                "Sin incoherencias"
        };
    }

    /// <summary>
    /// Agrega la conclusión general del motor.
    /// </summary>
    private static void AddSummarySection(
        List<string> lines,
        AudioAnalysisResult analysisResult)
    {
        lines.Add(string.Empty);
        lines.Add("--- Resumen general ---");
        lines.Add(string.Empty);

        lines.Add(
            analysisResult.SummaryDisplay);
    }

    /// <summary>
    /// Formatea la fecha de finalización.
    /// </summary>
    private static string FormatCompletedAt(
        DateTime? completedAt)
    {
        return completedAt.HasValue
            ? completedAt.Value.ToString(
                "yyyy-MM-dd HH:mm:ss.fff")
            : "Pendiente";
    }

    /// <summary>
    /// Evita mostrar valores vacíos.
    /// </summary>
    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Sin información"
            : value.Trim();
    }

    /// <summary>
    /// Formatea una duración con precisión de milisegundos.
    /// </summary>
    private static string FormatDuration(
        TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = TimeSpan.Zero;
        }

        if (value.TotalHours >= 1)
        {
            return value.ToString(
                @"h\:mm\:ss\.fff");
        }

        return value.ToString(
            @"m\:ss\.fff");
    }

    /// <summary>
    /// Formatea un valor entero positivo con su unidad.
    /// </summary>
    private static string FormatIntegerValue(
        int value,
        string unit)
    {
        if (value <= 0)
        {
            return "Sin información";
        }

        return $"{value} {unit}";
    }

    /// <summary>
    /// Convierte un valor lógico a texto en español.
    /// </summary>
    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}