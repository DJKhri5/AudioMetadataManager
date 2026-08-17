namespace AudioMetadataManager.UI.Services.Duplicates.Models;

/// <summary>
/// Especifica el criterio de coincidencia que originó la agrupación de duplicados.
/// </summary>
public enum DuplicateMatchKind
{
    /// <summary>
    /// Duplicado exacto a nivel de contenido binario o hash SHA-256.
    /// </summary>
    ExactBinary = 1,

    /// <summary>
    /// Duplicado probable por coincidencia exacta de Artista, Título y Versión de la pista.
    /// </summary>
    ProbableMetadata = 2,

    /// <summary>
    /// Duplicado potencial con coincidencia de Título pero diferente Artista o datos incompletos.
    /// </summary>
    SimilarTitle = 3
}
