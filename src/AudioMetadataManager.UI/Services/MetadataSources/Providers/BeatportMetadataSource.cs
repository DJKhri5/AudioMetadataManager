using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers;

/// <summary>
/// Proveedor de metadatos para Beatport.
/// La conexión real se implementará en una fase posterior.
/// </summary>
public class BeatportMetadataSource : IMetadataSource
{
    public string Name => "Beatport";

    public int Priority => 2;

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
                "Beatport todavía no está configurado."
        };

        return Task.FromResult(result);
    }
}