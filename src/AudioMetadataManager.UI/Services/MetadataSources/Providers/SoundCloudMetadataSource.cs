using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers;

/// <summary>
/// Proveedor de metadatos para SoundCloud.
/// Todos sus resultados requerirán aprobación manual.
/// </summary>
public class SoundCloudMetadataSource : IMetadataSource
{
    public string Name => "SoundCloud";

    public int Priority => 4;

    public bool IsAvailable => false;

    public bool RequiresManualApproval => true;

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
                "SoundCloud todavía no está configurado."
        };

        return Task.FromResult(result);
    }
}