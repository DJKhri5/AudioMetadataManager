using AudioMetadataManager.UI.Services.Security;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;

/// <summary>
/// Administra las credenciales de cliente de Spotify utilizando
/// el almacén seguro configurado para la aplicación.
/// </summary>
public sealed class SpotifyCredentialStore
{
    /// <summary>
    /// Identificador estable usado en el Administrador de
    /// credenciales de Windows para el Client ID.
    /// </summary>
    public const string ClientIdTargetName =
        "AudioMetadataManager/Spotify/ClientId";

    /// <summary>
    /// Identificador estable usado en el Administrador de
    /// credenciales de Windows para el Client Secret.
    /// </summary>
    public const string ClientSecretTargetName =
        "AudioMetadataManager/Spotify/ClientSecret";

    private readonly ISecretStore
        _secretStore;

    /// <summary>
    /// Crea el servicio utilizando el Administrador de
    /// credenciales de Windows.
    /// </summary>
    public SpotifyCredentialStore()
        : this(
            new WindowsCredentialStore())
    {
    }

    /// <summary>
    /// Crea el servicio con un almacén personalizado.
    /// </summary>
    public SpotifyCredentialStore(
        ISecretStore secretStore)
    {
        _secretStore =
            secretStore ??
            throw new ArgumentNullException(
                nameof(secretStore));
    }

    /// <summary>
    /// Indica si existen ambas credenciales guardadas.
    /// </summary>
    public bool HasCredentials =>
        _secretStore.ContainsSecret(
            ClientIdTargetName) &&
        _secretStore.ContainsSecret(
            ClientSecretTargetName);

    /// <summary>
    /// Recupera el Client ID guardado.
    /// </summary>
    public string? ReadClientId()
    {
        return _secretStore.ReadSecret(
            ClientIdTargetName);
    }

    /// <summary>
    /// Recupera el Client Secret guardado.
    /// </summary>
    public string? ReadClientSecret()
    {
        return _secretStore.ReadSecret(
            ClientSecretTargetName);
    }

    /// <summary>
    /// Guarda o reemplaza ambas credenciales.
    /// </summary>
    public void SaveCredentials(
        string clientId,
        string clientSecret)
    {
        _secretStore.SaveSecret(
            ClientIdTargetName,
            NormalizeCredential(
                clientId,
                nameof(clientId)));

        _secretStore.SaveSecret(
            ClientSecretTargetName,
            NormalizeCredential(
                clientSecret,
                nameof(clientSecret)));
    }

    /// <summary>
    /// Elimina ambas credenciales guardadas.
    /// </summary>
    public bool DeleteCredentials()
    {
        bool clientIdDeleted =
            _secretStore.DeleteSecret(
                ClientIdTargetName);

        bool clientSecretDeleted =
            _secretStore.DeleteSecret(
                ClientSecretTargetName);

        return clientIdDeleted &&
            clientSecretDeleted;
    }

    private static string NormalizeCredential(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            throw new ArgumentException(
                "La credencial de Spotify no puede estar vacía.",
                parameterName);
        }

        return value.Trim();
    }
}
