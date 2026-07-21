namespace AudioMetadataManager.UI.Services.MetadataSources.Models;

/// <summary>
/// Identifica de forma segura y estable los campos de metadatos
/// utilizados por los motores de comparación, confianza y consenso.
///
/// Evita depender de textos como "Artist", "Title" o "Genre"
/// dentro de la lógica de negocio.
/// </summary>
public enum MetadataField
{
    /// <summary>
    /// Campo no identificado o todavía no asignado.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Artista principal o conjunto de artistas.
    /// </summary>
    Artist = 1,

    /// <summary>
    /// Título principal de la pista.
    /// </summary>
    Title = 2,

    /// <summary>
    /// Versión, mezcla o remix de la pista.
    /// </summary>
    Version = 3,

    /// <summary>
    /// Álbum o publicación asociada.
    /// </summary>
    Album = 4,

    /// <summary>
    /// Sello discográfico.
    /// </summary>
    Label = 5,

    /// <summary>
    /// Género musical.
    /// </summary>
    Genre = 6
}