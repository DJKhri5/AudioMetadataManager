namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Representa las mediciones obtenidas durante el análisis
/// del silencio exterior de un archivo de audio.
///
/// Esta clase solamente describe datos técnicos.
/// No determina si el silencio es correcto, incorrecto
/// o si el archivo debe ser modificado.
/// </summary>
public class AudioSilenceAnalysisResult
    : AnalysisModuleResult
{
    /// <summary>
    /// Duración técnica completa del archivo decodificado.
    ///
    /// Incluye contenido musical, silencio inicial,
    /// silencio final y cualquier otro contenido existente.
    /// </summary>
    public TimeSpan TechnicalDuration { get; set; }

    /// <summary>
    /// Silencio continuo estimado al principio del archivo.
    /// </summary>
    public TimeSpan LeadingSilence { get; set; }

    /// <summary>
    /// Silencio continuo estimado al final del archivo.
    /// </summary>
    public TimeSpan TrailingSilence { get; set; }

    /// <summary>
    /// Duración comprendida entre el primer y el último
    /// fragmento de audio que supera el umbral técnico
    /// utilizado por el analizador.
    /// </summary>
    public TimeSpan AudibleDuration { get; set; }

    /// <summary>
    /// Umbral en decibelios utilizado para clasificar
    /// una muestra como silencio técnico.
    /// </summary>
    public double SilenceThresholdDb { get; set; }

    /// <summary>
    /// Indica si el resultado necesita revisión manual.
    ///
    /// Esta propiedad se conserva temporalmente para mantener
    /// compatibilidad con el código existente. Más adelante,
    /// la decisión de revisión pertenecerá al motor comparativo.
    /// </summary>
    public bool RequiresManualReview { get; set; }

    /// <summary>
    /// Indica si se detectó silencio técnico al inicio.
    ///
    /// No significa que el silencio sea un problema.
    /// </summary>
    public bool HasLeadingSilence =>
        LeadingSilence > TimeSpan.Zero;

    /// <summary>
    /// Indica si se detectó silencio técnico al final.
    ///
    /// No significa que el silencio sea un problema.
    /// </summary>
    public bool HasTrailingSilence =>
        TrailingSilence > TimeSpan.Zero;

    /// <summary>
    /// Indica si se detectó silencio técnico exterior,
    /// ya sea al inicio o al final del archivo.
    /// </summary>
    public bool HasOuterSilence =>
        HasLeadingSilence ||
        HasTrailingSilence;

    /*
     * PROPIEDADES DE COMPATIBILIDAD TEMPORAL
     *
     * Estas propiedades todavía pueden ser utilizadas por
     * AudioSilenceAnalyzer, SilenceAnalysisStage o por el
     * generador del informe.
     *
     * No las eliminaremos hasta actualizar esos archivos.
     */

    /// <summary>
    /// Propiedad antigua conservada temporalmente.
    /// Ya no debe utilizarse para juzgar el archivo.
    /// </summary>
    [Obsolete(
        "Use HasLeadingSilence. " +
        "El silencio no debe clasificarse como sospechoso.")]
    public bool HasSuspiciousLeadingSilence { get; set; }

    /// <summary>
    /// Propiedad antigua conservada temporalmente.
    /// Ya no debe utilizarse para juzgar el archivo.
    /// </summary>
    [Obsolete(
        "Use HasTrailingSilence. " +
        "El silencio no debe clasificarse como sospechoso.")]
    public bool HasSuspiciousTrailingSilence { get; set; }

    /// <summary>
    /// Propiedad antigua conservada temporalmente.
    /// Será eliminada cuando los demás componentes
    /// utilicen el modelo descriptivo.
    /// </summary>
    [Obsolete(
        "Use HasOuterSilence. " +
        "El silencio no debe clasificarse como sospechoso.")]
    public bool HasSuspiciousSilence =>
        HasSuspiciousLeadingSilence ||
        HasSuspiciousTrailingSilence;

    /// <summary>
    /// Suma del silencio exterior detectado.
    /// </summary>
    public TimeSpan TotalOuterSilence =>
        LeadingSilence + TrailingSilence;

    /// <summary>
    /// Diferencia entre la duración técnica y la duración
    /// audible estimada.
    /// </summary>
    public TimeSpan TechnicalAudibleDifference
    {
        get
        {
            TimeSpan difference =
                TechnicalDuration - AudibleDuration;

            return difference < TimeSpan.Zero
                ? TimeSpan.Zero
                : difference;
        }
    }

    /// <summary>
    /// Porcentaje aproximado del archivo correspondiente
    /// al silencio exterior detectado.
    /// </summary>
    public double OuterSilencePercentage
    {
        get
        {
            if (TechnicalDuration <= TimeSpan.Zero)
            {
                return 0;
            }

            double percentage =
                TotalOuterSilence.TotalSeconds /
                TechnicalDuration.TotalSeconds *
                100;

            return Math.Clamp(
                percentage,
                0,
                100);
        }
    }

    /// <summary>
    /// Indica si existen mediciones válidas para participar
    /// en comparaciones de duración.
    /// </summary>
    public override bool HasComparisonData =>
        AnalysisCompleted &&
        IsReliable &&
        !HasError &&
        TechnicalDuration > TimeSpan.Zero;

    /// <summary>
    /// Duración técnica en formato legible.
    /// </summary>
    public string TechnicalDurationDisplay =>
        FormatDuration(TechnicalDuration);

    /// <summary>
    /// Duración audible estimada en formato legible.
    /// </summary>
    public string AudibleDurationDisplay =>
        FormatDuration(AudibleDuration);

    /// <summary>
    /// Silencio inicial en formato legible.
    /// </summary>
    public string LeadingSilenceDisplay =>
        FormatDuration(LeadingSilence);

    /// <summary>
    /// Silencio final en formato legible.
    /// </summary>
    public string TrailingSilenceDisplay =>
        FormatDuration(TrailingSilence);

    /// <summary>
    /// Silencio exterior total en formato legible.
    /// </summary>
    public string TotalOuterSilenceDisplay =>
        FormatDuration(TotalOuterSilence);

    /// <summary>
    /// Diferencia entre la duración técnica y audible
    /// en formato legible.
    /// </summary>
    public string TechnicalAudibleDifferenceDisplay =>
        FormatDuration(
            TechnicalAudibleDifference);

    /// <summary>
    /// Porcentaje de silencio exterior para la interfaz.
    /// </summary>
    public string OuterSilencePercentageDisplay =>
        $"{OuterSilencePercentage:0.00}%";

    /// <summary>
    /// Formatea una duración incluyendo milisegundos.
    /// </summary>
    private static string FormatDuration(
        TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            duration = TimeSpan.Zero;
        }

        if (duration.TotalHours >= 1)
        {
            return duration.ToString(
                @"h\:mm\:ss\.fff");
        }

        return duration.ToString(
            @"m\:ss\.fff");
    }
}