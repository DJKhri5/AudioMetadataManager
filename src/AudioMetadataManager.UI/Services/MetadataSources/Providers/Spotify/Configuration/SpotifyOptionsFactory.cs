namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;

/// <summary>
/// Construye la configuración de Spotify utilizando las
/// credenciales almacenadas localmente de forma segura.
/// </summary>
public static class SpotifyOptionsFactory
{
    /// <summary>
    /// Crea la configuración predeterminada de Spotify.
    /// </summary>
    public static SpotifyOptions CreateDefault()
    {
        SpotifyCredentialStore credentialStore =
            new();

        return new SpotifyOptions
        {
            ClientId =
                credentialStore.ReadClientId(),

            ClientSecret =
                credentialStore.ReadClientSecret()
        };
    }
}
