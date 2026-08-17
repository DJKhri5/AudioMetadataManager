using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Providers.Spotify;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers;

/// <summary>
/// Proveedor de metadatos para Spotify.
/// </summary>
public sealed class SpotifyMetadataSource : IMetadataSource
{
    private readonly SpotifyMetadataProvider _provider = new();

    public string Name => "Spotify";

    public int Priority => 3;

    public bool IsAvailable => true;

    public bool RequiresManualApproval => false;

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
            ErrorMessage = candidates.Count == 0 ? "No se encontraron coincidencias en Spotify." : string.Empty
        };
    }
}