namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Configuration;

/// <summary>
/// Contiene la configuración necesaria para utilizar
/// el proveedor de identificación AcoustID.
///
/// No almacena la clave directamente en el código fuente.
/// La clave será proporcionada posteriormente mediante
/// configuración local segura.
/// </summary>
public sealed class AcoustIdOptions
{
    /// <summary>
    /// Dirección base de la API de AcoustID.
    /// </summary>
    public Uri BaseAddress { get; init; } =
        new("https://api.acoustid.org/v2/");

    /// <summary>
    /// Clave de cliente registrada en AcoustID.
    ///
    /// Debe obtenerse desde configuración local segura.
    /// Nunca debe quedar escrita directamente en el repositorio.
    /// </summary>
    public string? ClientApiKey { get; init; }

    /// <summary>
    /// Campos adicionales solicitados a AcoustID.
    /// "recordings" es suficiente para obtener el
    /// identificador MBID de MusicBrainz.
    /// </summary>
    public string MetaFields { get; init; } =
        "recordings";

    /// <summary>
    /// Tiempo máximo permitido para una solicitud HTTP.
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } =
        TimeSpan.FromSeconds(15);

    /// <summary>
    /// Indica si existe una configuración mínima utilizable.
    /// </summary>
    public bool IsValid =>
        BaseAddress.IsAbsoluteUri &&
        !string.IsNullOrWhiteSpace(MetaFields) &&
        RequestTimeout > TimeSpan.Zero;

    /// <summary>
    /// Indica si existe una clave de cliente disponible.
    /// </summary>
    public bool HasClientApiKey =>
        !string.IsNullOrWhiteSpace(ClientApiKey);
}
