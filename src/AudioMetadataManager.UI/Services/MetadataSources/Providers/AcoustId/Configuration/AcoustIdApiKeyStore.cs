using AudioMetadataManager.UI.Services.Security;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Configuration;

/// <summary>
/// Administra la clave de cliente de AcoustID utilizando
/// el almacén seguro configurado para la aplicación.
/// </summary>
public sealed class AcoustIdApiKeyStore
{
    /// <summary>
    /// Identificador estable usado en el Administrador de
    /// credenciales de Windows.
    /// </summary>
    public const string CredentialTargetName =
        "AudioMetadataManager/AcoustId/ClientApiKey";

    private readonly ISecretStore
        _secretStore;

    /// <summary>
    /// Crea el servicio utilizando el Administrador de
    /// credenciales de Windows.
    /// </summary>
    public AcoustIdApiKeyStore()
        : this(
            new WindowsCredentialStore())
    {
    }

    /// <summary>
    /// Crea el servicio con un almacén personalizado.
    /// </summary>
    public AcoustIdApiKeyStore(
        ISecretStore secretStore)
    {
        _secretStore =
            secretStore ??
            throw new ArgumentNullException(
                nameof(secretStore));
    }

    /// <summary>
    /// Indica si existe una clave guardada.
    /// </summary>
    public bool HasApiKey =>
        _secretStore.ContainsSecret(
            CredentialTargetName);

    /// <summary>
    /// Recupera la clave guardada.
    /// </summary>
    public string? ReadApiKey()
    {
        return _secretStore.ReadSecret(
            CredentialTargetName);
    }

    /// <summary>
    /// Guarda o reemplaza la clave.
    /// </summary>
    public void SaveApiKey(
        string apiKey)
    {
        string normalizedApiKey =
            NormalizeApiKey(
                apiKey);

        _secretStore.SaveSecret(
            CredentialTargetName,
            normalizedApiKey);
    }

    /// <summary>
    /// Elimina la clave guardada.
    /// </summary>
    public bool DeleteApiKey()
    {
        return _secretStore.DeleteSecret(
            CredentialTargetName);
    }

    private static string NormalizeApiKey(
        string apiKey)
    {
        if (string.IsNullOrWhiteSpace(
                apiKey))
        {
            throw new ArgumentException(
                "La clave de AcoustID no puede estar vacía.",
                nameof(apiKey));
        }

        return apiKey.Trim();
    }
}
