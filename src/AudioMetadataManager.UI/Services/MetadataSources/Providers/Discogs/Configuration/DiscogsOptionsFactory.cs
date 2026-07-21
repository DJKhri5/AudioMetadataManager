namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;

/// <summary>
/// Construye la configuración de Discogs utilizando el token
/// almacenado localmente de forma segura.
/// </summary>
public static class DiscogsOptionsFactory
{
    /// <summary>
    /// Crea la configuración predeterminada de Discogs.
    /// </summary>
    public static DiscogsOptions CreateDefault()
    {
        DiscogsTokenStore tokenStore =
            new();

        string? token =
            tokenStore.ReadToken();

        return new DiscogsOptions
        {
            UserToken =
                token
        };
    }
}