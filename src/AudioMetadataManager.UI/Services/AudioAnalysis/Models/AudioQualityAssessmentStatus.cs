namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Identifica el estado general obtenido por el motor
/// de evaluación técnica del audio.
///
/// Este estado no afirma de manera absoluta el origen del
/// archivo. Resume la coherencia encontrada entre el formato
/// declarado y las mediciones técnicas disponibles.
/// </summary>
public enum AudioQualityAssessmentStatus
{
    /// <summary>
    /// No existen datos suficientes para realizar una
    /// evaluación técnica confiable.
    /// </summary>
    InsufficientData = 0,

    /// <summary>
    /// El formato o el tipo de evaluación solicitado no
    /// resulta aplicable al archivo analizado.
    /// </summary>
    NotApplicable = 1,

    /// <summary>
    /// Las mediciones disponibles son coherentes con las
    /// características técnicas declaradas por el archivo.
    /// </summary>
    Consistent = 2,

    /// <summary>
    /// Existen pequeñas incoherencias, pero no bastan para
    /// considerar probable una transcodificación.
    /// </summary>
    SlightlySuspicious = 3,

    /// <summary>
    /// Varias mediciones técnicas resultan poco coherentes
    /// con las características declaradas.
    /// </summary>
    Suspicious = 4,

    /// <summary>
    /// Las mediciones presentan una incompatibilidad marcada
    /// con las características declaradas y son compatibles
    /// con una posible transcodificación desde una fuente de
    /// menor calidad.
    /// </summary>
    LikelyTranscoded = 5
}