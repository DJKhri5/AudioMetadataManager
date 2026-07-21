using System.Net;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Api;

/// <summary>
/// Contiene una respuesta HTTP normalizada recibida desde
/// Discogs.
///
/// Esta clase evita que las capas superiores dependan
/// directamente de HttpResponseMessage.
/// </summary>
public sealed class DiscogsApiResponse
{
    /// <summary>
    /// Código de estado HTTP recibido.
    /// </summary>
    public HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// Cuerpo de la respuesta como texto.
    /// </summary>
    public string Content { get; init; } =
        string.Empty;

    /// <summary>
    /// Información de límite de solicitudes.
    /// </summary>
    public DiscogsRateLimitInfo RateLimit { get; init; } =
        new();

    /// <summary>
    /// Dirección solicitada.
    /// </summary>
    public Uri? RequestUri { get; init; }

    /// <summary>
    /// Mensaje descriptivo de la operación.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si el servidor devolvió un estado satisfactorio.
    /// </summary>
    public bool IsSuccessStatusCode =>
        (int)StatusCode is >= 200 and <= 299;

    /// <summary>
    /// Indica si existe contenido utilizable.
    /// </summary>
    public bool HasContent =>
        !string.IsNullOrWhiteSpace(Content);
}