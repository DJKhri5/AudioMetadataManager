namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Configuration;

/// <summary>
/// Construye la configuración de AcoustID utilizando la clave
/// almacenada localmente de forma segura.
/// </summary>
public static class AcoustIdOptionsFactory
{
    /// <summary>
    /// Crea la configuración predeterminada de AcoustID.
    /// </summary>
    public static AcoustIdOptions CreateDefault()
    {
        AcoustIdApiKeyStore apiKeyStore =
            new();

        string? apiKey =
            apiKeyStore.ReadApiKey();

        return new AcoustIdOptions
        {
            ClientApiKey =
                apiKey
        };
    }
}
