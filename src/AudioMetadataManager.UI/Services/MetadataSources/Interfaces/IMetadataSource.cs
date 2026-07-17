using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Interfaces;

/// <summary>
/// Define el contrato común que deberá cumplir cualquier
/// fuente externa de metadatos musicales.
/// </summary>
public interface IMetadataSource
{
    /// <summary>
    /// Nombre legible de la plataforma.
    /// Ejemplos: Discogs, Beatport, Spotify o SoundCloud.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Prioridad de consulta de la fuente.
    /// Un número menor significa que se consulta antes.
    ///
    /// Orden previsto:
    /// Discogs   = 1
    /// Beatport  = 2
    /// Spotify   = 3
    /// SoundCloud = 4
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Indica si la fuente está configurada y disponible
    /// para realizar búsquedas.
    ///
    /// Por ejemplo, podrá ser falso cuando falte una clave API.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Indica si todos los resultados de esta fuente deben
    /// ser revisados y aprobados manualmente.
    ///
    /// SoundCloud deberá devolver true.
    /// </summary>
    bool RequiresManualApproval { get; }

    /// <summary>
    /// Realiza una búsqueda de metadatos en la plataforma.
    /// </summary>
    /// <param name="request">
    /// Datos obtenidos desde el archivo local, el parser
    /// y sus etiquetas actuales.
    /// </param>
    /// <param name="cancellationToken">
    /// Permite cancelar la consulta cuando el usuario detiene
    /// el análisis o cierra la aplicación.
    /// </param>
    /// <returns>
    /// Resultado completo de la búsqueda, incluidos candidatos,
    /// errores, consulta utilizada y tiempo de respuesta.
    /// </returns>
    Task<MetadataSearchResult> SearchAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default);
}