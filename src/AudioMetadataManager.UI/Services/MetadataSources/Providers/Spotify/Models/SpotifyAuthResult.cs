namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

/// <summary>
/// Contiene el resultado de solicitar un token de acceso
/// mediante el flujo "Client Credentials" de Spotify.
/// </summary>
public sealed class SpotifyAuthResult
{
    /// <summary>
    /// Estado general de la operación.
    /// </summary>
    public SpotifyProviderStatus Status { get; init; } =
        SpotifyProviderStatus.Unknown;

    /// <summary>
    /// Token obtenido, cuando la operación tuvo éxito.
    /// </summary>
    public SpotifyAccessToken? Token { get; init; }

    /// <summary>
    /// Mensaje descriptivo para interfaz o diagnóstico.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si se obtuvo un token utilizable.
    /// </summary>
    public bool IsSuccess =>
        Status == SpotifyProviderStatus.Success &&
        Token?.HasValue == true;
}
