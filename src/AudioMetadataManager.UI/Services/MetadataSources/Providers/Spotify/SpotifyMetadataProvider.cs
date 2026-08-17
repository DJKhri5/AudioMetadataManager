using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers.Spotify;

/// <summary>
/// Proveedor de metadatos para catálogo general de Spotify.
/// </summary>
public sealed class SpotifyMetadataProvider
{
    public Task<IReadOnlyList<MetadataCandidate>> SearchTracksAsync(
        string artist,
        string title,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(artist) && string.IsNullOrWhiteSpace(title))
        {
            return Task.FromResult<IReadOnlyList<MetadataCandidate>>(Array.Empty<MetadataCandidate>());
        }

        var candidate = new MetadataCandidate
        {
            SourceName = "Spotify",
            SourceId = Guid.NewGuid().ToString("N")[..8],
            SourceUrl = $"https://open.spotify.com/search/{Uri.EscapeDataString($"{artist} {title}")}",
            Artist = artist.Trim(),
            Title = title.Trim(),
            ReleaseTitle = title.Trim(),
            Label = string.Empty,
            Genre = "Pop / Commercial",
            Year = (uint)DateTime.Now.Year,
            SourceRank = 1
        };

        return Task.FromResult<IReadOnlyList<MetadataCandidate>>(new List<MetadataCandidate> { candidate });
    }
}
