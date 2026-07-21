using AudioMetadataManager.UI.Services.Security;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;

/// <summary>
/// Administra el token de Discogs utilizando el almacén
/// seguro configurado para la aplicación.
/// </summary>
public sealed class DiscogsTokenStore
{
    /// <summary>
    /// Identificador estable usado en el Administrador de
    /// credenciales de Windows.
    /// </summary>
    public const string CredentialTargetName =
        "AudioMetadataManager/Discogs/UserToken";

    private readonly ISecretStore
        _secretStore;

    /// <summary>
    /// Crea el servicio utilizando el Administrador de
    /// credenciales de Windows.
    /// </summary>
    public DiscogsTokenStore()
        : this(
            new WindowsCredentialStore())
    {
    }

    /// <summary>
    /// Crea el servicio con un almacén personalizado.
    /// </summary>
    public DiscogsTokenStore(
        ISecretStore secretStore)
    {
        _secretStore =
            secretStore ??
            throw new ArgumentNullException(
                nameof(secretStore));
    }

    /// <summary>
    /// Indica si existe un token guardado.
    /// </summary>
    public bool HasToken =>
        _secretStore.ContainsSecret(
            CredentialTargetName);

    /// <summary>
    /// Recupera el token guardado.
    /// </summary>
    public string? ReadToken()
    {
        return _secretStore.ReadSecret(
            CredentialTargetName);
    }

    /// <summary>
    /// Guarda o reemplaza el token.
    /// </summary>
    public void SaveToken(
        string token)
    {
        string normalizedToken =
            NormalizeToken(
                token);

        _secretStore.SaveSecret(
            CredentialTargetName,
            normalizedToken);
    }

    /// <summary>
    /// Elimina el token guardado.
    /// </summary>
    public bool DeleteToken()
    {
        return _secretStore.DeleteSecret(
            CredentialTargetName);
    }

    private static string NormalizeToken(
        string token)
    {
        if (string.IsNullOrWhiteSpace(
                token))
        {
            throw new ArgumentException(
                "El token de Discogs no puede estar vacío.",
                nameof(token));
        }

        return token.Trim();
    }
}