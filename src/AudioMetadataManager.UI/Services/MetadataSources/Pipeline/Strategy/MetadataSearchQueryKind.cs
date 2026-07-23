namespace AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Strategy;

/// <summary>
/// Identifica el origen y el nivel de precisión de una
/// consulta generada por la estrategia de búsqueda.
/// </summary>
public enum MetadataSearchQueryKind
{
    /// <summary>
    /// La consulta todavía no ha sido clasificada.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Consulta principal basada en artista, título y versión
    /// obtenidos desde el nombre del archivo.
    /// </summary>
    ParsedIdentityWithVersion = 1,

    /// <summary>
    /// Consulta basada en artista y título interpretados,
    /// omitiendo la versión.
    /// </summary>
    ParsedIdentityWithoutVersion = 2,

    /// <summary>
    /// Consulta alternativa basada en las etiquetas actuales
    /// del archivo.
    /// </summary>
    TaggedIdentity = 3,

    /// <summary>
    /// Consulta amplia basada únicamente en el título
    /// interpretado.
    ///
    /// Debe utilizarse como último recurso debido a su menor
    /// precisión.
    /// </summary>
    ParsedTitleOnly = 4,

    /// <summary>
    /// Consulta amplia basada únicamente en el título
    /// almacenado en las etiquetas.
    /// </summary>
    TaggedTitleOnly = 5
}