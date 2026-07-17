using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers;

/// <summary>
/// Proveedor de metadatos para Discogs.
/// La conexión real se implementará en una fase posterior.
/// </summary>
public class DiscogsMetadataSource : IMetadataSource
{
    public string Name => "Discogs";

    public int Priority => 1;

    public bool IsAvailable => false;

    public bool RequiresManualApproval => false;

    public Task<MetadataSearchResult> SearchAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        MetadataSearchResult result = new()
        {
            SourceName = Name,
            QueryUsed = request.PrimaryQuery,
            WasSuccessful = false,
            ErrorMessage =
                "Discogs todavía no está configurado."
        };

        return Task.FromResult(result);
    }
}