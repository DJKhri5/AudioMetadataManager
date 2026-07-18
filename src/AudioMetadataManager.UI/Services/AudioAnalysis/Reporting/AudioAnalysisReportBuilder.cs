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

        AddSilenceSection(
            lines,
            analysisResult.Silence);

        AddEnvelopeSection(
            lines,
            analysisResult.Envelope);

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