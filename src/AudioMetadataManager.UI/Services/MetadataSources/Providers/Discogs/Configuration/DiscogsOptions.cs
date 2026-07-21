namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;

/// <summary>
/// Contiene la configuración necesaria para utilizar
/// el proveedor de metadatos Discogs.
///
/// No almacena el token directamente en el código fuente.
/// El token será proporcionado posteriormente mediante
/// configuración local segura.
/// </summary>
public sealed class DiscogsOptions
{
    /// <summary>
    /// Dirección base de la API de Discogs.
    /// </summary>
    public Uri BaseAddress { get; init; } =
        new("https://api.discogs.com/");

    /// <summary>
    /// Nombre identificador de la aplicación enviado mediante
    /// el encabezado User-Agent.
    /// </summary>
    public string ApplicationName { get; init; } =
        "AudioMetadataManager";

    /// <summary>
    /// Versión de la aplicación enviada mediante User-Agent.
    /// </summary>
    public string ApplicationVersion { get; init; } =
        "0.2";

    /// <summary>
    /// Token personal de acceso a Discogs.
    ///
    /// Debe obtenerse desde configuración local segura.
    /// Nunca debe quedar escrito directamente en el repositorio.
    /// </summary>
    public string? UserToken { get; init; }

    /// <summary>
    /// Cantidad máxima inicial de candidatos solicitados
    /// durante una búsqueda.
    /// </summary>
    public int ResultsPerPage { get; init; } = 20;

    /// <summary>
    /// Tiempo máximo permitido para una solicitud HTTP.
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } =
        TimeSpan.FromSeconds(30);

    /// <summary>
    /// User-Agent completo que identificará a la aplicación.
    /// </summary>
    public string UserAgent =>
        $"{NormalizeUserAgentPart(ApplicationName)}/" +
        $"{NormalizeUserAgentPart(ApplicationVersion)}";

    /// <summary>
    /// Indica si existe una configuración mínima utilizable.
    /// </summary>
    public bool IsValid =>
        BaseAddress.IsAbsoluteUri &&
        !string.IsNullOrWhiteSpace(ApplicationName) &&
        !string.IsNullOrWhiteSpace(ApplicationVersion) &&
        ResultsPerPage is > 0 and <= 100 &&
        RequestTimeout > TimeSpan.Zero;

    /// <summary>
    /// Indica si existe un token disponible.
    /// </summary>
    public bool HasUserToken =>
        !string.IsNullOrWhiteSpace(UserToken);

    private static string NormalizeUserAgentPart(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        return value
            .Trim()
            .Replace(" ", string.Empty);
    }
}