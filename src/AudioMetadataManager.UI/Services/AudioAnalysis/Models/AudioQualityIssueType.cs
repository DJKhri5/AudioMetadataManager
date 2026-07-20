namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Identifica una incoherencia técnica detectada durante
/// la evaluación de calidad del archivo.
///
/// Un mismo archivo puede presentar más de un tipo de
/// incoherencia simultáneamente.
/// </summary>
public enum AudioQualityIssueType
{
    /// <summary>
    /// No se detectó ninguna incoherencia técnica.
    /// </summary>
    None = 0,

    /// <summary>
    /// El bitrate declarado resulta poco coherente con las
    /// mediciones técnicas disponibles.
    /// </summary>
    DeclaredBitrateMismatch = 1,

    /// <summary>
    /// La extensión espectral observada es inferior a la
    /// esperable para las características declaradas.
    /// </summary>
    LimitedSpectralExtension = 2,

    /// <summary>
    /// El comportamiento espectral sugiere que un archivo
    /// sin pérdida podría provenir de una fuente con pérdida.
    /// </summary>
    PossibleLossySource = 3,

    /// <summary>
    /// Se observan indicios compatibles con una recompresión
    /// o una codificación con pérdida realizada más de una vez.
    /// </summary>
    PossibleRecompression = 4,

    /// <summary>
    /// El contenedor, el códec o los metadatos técnicos no
    /// coinciden de forma coherente entre sí.
    /// </summary>
    TechnicalMetadataMismatch = 5,

    /// <summary>
    /// Se detecta un corte superior o una caída espectral
    /// potencialmente artificial.
    /// </summary>
    SuspiciousHighFrequencyCutoff = 6,

    /// <summary>
    /// No existen datos suficientes para determinar con
    /// precisión el tipo de incoherencia.
    /// </summary>
    InsufficientEvidence = 7
}