using AudioMetadataManager.UI.Services.Security;

namespace AudioMetadataManager.UI.Services.MetadataSources.Configuration;

/// <summary>
/// Administra credenciales de proveedores externos utilizando el Administrador de Credenciales de Windows.
/// </summary>
public sealed class ProviderTokenStore
{
    private readonly string _targetName;
    private readonly ISecretStore _secretStore;

    public ProviderTokenStore(string providerName, ISecretStore? secretStore = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);
        _targetName = $"AudioMetadataManager/{providerName}/ApiKey";
        _secretStore = secretStore ?? new WindowsCredentialStore();
    }

    public bool HasToken => _secretStore.ContainsSecret(_targetName);

    public string? ReadToken() => _secretStore.ReadSecret(_targetName);

    public void SaveToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        _secretStore.SaveSecret(_targetName, token.Trim());
    }

    public void DeleteToken()
    {
        _secretStore.DeleteSecret(_targetName);
    }
}
