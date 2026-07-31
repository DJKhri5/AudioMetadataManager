namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;

/// <summary>
/// Contiene la configuración necesaria para utilizar
/// el proveedor de metadatos Spotify.
///
/// Spotify utiliza el flujo "Client Credentials" para búsquedas
/// de catálogo público: no requiere que el usuario inicie sesión,
/// sólo un identificador y un secreto de cliente registrados en
/// el panel de desarrolladores de Spotify.
///
/// No almacena las credenciales directamente en el código fuente.
/// Deben obtenerse desde configuración local segura.
/// </summary>
public sealed class SpotifyOptions
{
    /// <summary>
    /// Dirección base de la API pública de Spotify.
    /// </summary>
    public Uri ApiBaseAddress { get; init; } =
        new("https://api.spotify.com/v1/");

    /// <summary>
    /// Dirección del punto de emisión de tokens de acceso.
    /// </summary>
    public Uri TokenAddress { get; init; } =
        new("https://accounts.spotify.com/api/token");

    /// <summary>
    /// Identificador de cliente registrado en el panel de
    /// desarrolladores de Spotify.
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    /// Secreto de cliente asociado al identificador.
    ///
    /// Nunca debe quedar escrito directamente en el repositorio.
    /// </summary>
    public string? ClientSecret { get; init; }

    /// <summary>
    /// Cantidad máxima de candidatos solicitados durante
    /// una búsqueda.
    ///
    /// La documentación oficial permite hasta 50, pero se
    /// comprobó de forma empírica que una app nueva en
    /// "Development mode" recibe HTTP 400 ("Invalid limit") con
    /// cualquier valor mayor a 10. El valor predeterminado se
    /// mantiene en 10 para que la aplicación funcione sin
    /// configuración adicional; puede aumentarse una vez que la
    /// app de Spotify salga de modo desarrollo (Extended Quota
    /// Mode).
    /// </summary>
    public int ResultsPerPage { get; init; } =
        10;

    /// <summary>
    /// Tiempo máximo permitido para una solicitud HTTP, tanto
    /// para autenticación como para búsqueda.
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } =
        TimeSpan.FromSeconds(15);

    /// <summary>
    /// Indica si existe una configuración mínima utilizable.
    /// </summary>
    public bool IsValid =>
        ApiBaseAddress.IsAbsoluteUri &&
        TokenAddress.IsAbsoluteUri &&
        ResultsPerPage is > 0 and <= 50 &&
        RequestTimeout > TimeSpan.Zero;

    /// <summary>
    /// Indica si existen credenciales de cliente disponibles.
    /// </summary>
    public bool HasCredentials =>
        !string.IsNullOrWhiteSpace(ClientId) &&
        !string.IsNullOrWhiteSpace(ClientSecret);
}
