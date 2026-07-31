namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

/// <summary>
/// Representa un token de acceso obtenido mediante el flujo
/// "Client Credentials" de Spotify.
/// </summary>
public sealed class SpotifyAccessToken
{
    /// <summary>
    /// Valor del token, listo para usarse en el encabezado
    /// Authorization.
    /// </summary>
    public string Value { get; init; } =
        string.Empty;

    /// <summary>
    /// Momento UTC en que el token deja de ser válido.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; init; }

    /// <summary>
    /// Indica si el token contiene un valor utilizable.
    /// </summary>
    public bool HasValue =>
        !string.IsNullOrWhiteSpace(
            Value);

    /// <summary>
    /// Indica si el token sigue siendo válido.
    ///
    /// Se reserva un margen de seguridad de 30 segundos antes
    /// del vencimiento real informado por Spotify, para evitar
    /// usar un token que expire durante una solicitud en curso.
    /// </summary>
    public bool IsValid =>
        HasValue &&
        DateTimeOffset.UtcNow <
            ExpiresAtUtc - TimeSpan.FromSeconds(30);
}
