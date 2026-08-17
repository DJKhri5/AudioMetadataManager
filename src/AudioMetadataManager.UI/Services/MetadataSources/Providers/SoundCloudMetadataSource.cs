using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Providers.SoundCloud;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers;

/// <summary>
/// Proveedor de metadatos para SoundCloud especializado en bootlegs y remixes independientes.
/// Todos sus resultados requieren aprobación manual debido a la naturaleza abierta de la plataforma.
/// </summary>
public sealed class SoundCloudMetadataSource : IMetadataSource
{
    private readonly SoundCloudMetadataProvider _provider = new();

    public string Name => "SoundCloud";

    public int Priority => 4;

    public bool IsAvailable => true;

    public bool RequiresManualApproval => true;

    public async Task<MetadataSearchResult> SearchAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string artist = !string.IsNullOrWhiteSpace(request.ParsedArtist)
            ? request.ParsedArtist
            : request.TaggedArtist;

        string title = !string.IsNullOrWhiteSpace(request.ParsedTitle)
            ? request.ParsedTitle
            : request.TaggedTitle;

        var candidates = await _provider.SearchTracksAsync(artist, title, cancellationToken).ConfigureAwait(false);

        return new MetadataSearchResult
        {
            SourceName = Name,
            QueryUsed = request.PrimaryQuery,
            WasSuccessful = candidates.Count > 0,
            Candidates = candidates.ToList(),
            ErrorMessage = candidates.Count == 0 ? "No se encontraron coincidencias en SoundCloud." : string.Empty
        };
    }
}