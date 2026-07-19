namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Identifica características tonales descriptivas obtenidas
/// a partir del perfil espectral y del balance tonal.
///
/// Estos valores describen el audio analizado.
/// No representan por sí mismos una evaluación de calidad
/// ni una recomendación de ecualización.
/// </summary>
public enum AudioToneCharacter
{
    /// <summary>
    /// No existen datos suficientes para asignar una
    /// caracterización tonal confiable.
    /// </summary>
    InsufficientData = 0,

    /// <summary>
    /// No existe un predominio tonal suficientemente marcado
    /// entre las regiones principales.
    /// </summary>
    Balanced = 1,

    /// <summary>
    /// La región baja presenta una presencia destacada
    /// respecto de las regiones media y alta.
    /// </summary>
    BassDominant = 2,

    /// <summary>
    /// La región media presenta una presencia destacada
    /// respecto de las regiones baja y alta.
    /// </summary>
    MidrangeDominant = 3,

    /// <summary>
    /// La región alta presenta una presencia destacada
    /// respecto de las regiones baja y media.
    /// </summary>
    TrebleDominant = 4,

    /// <summary>
    /// El contenido de altas frecuencias presenta una
    /// presencia reducida respecto del resto del espectro.
    /// </summary>
    Dark = 5,

    /// <summary>
    /// El contenido de altas frecuencias presenta una
    /// presencia elevada y persistente.
    /// </summary>
    Bright = 6,

    /// <summary>
    /// Existe una presencia relevante en graves y medios
    /// bajos sin una extensión alta especialmente marcada.
    /// </summary>
    Warm = 7,

    /// <summary>
    /// La región baja presenta poca participación respecto
    /// de las regiones media y alta.
    /// </summary>
    Thin = 8
}