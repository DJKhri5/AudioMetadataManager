using System.Text.RegularExpressions;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers.SoundCloud;

/// <summary>
/// Proveedor de metadatos especializado en pistas independientes, bootlegs y remixes de SoundCloud.
/// </summary>
public sealed class SoundCloudMetadataProvider
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

        string cleanTitle = title;
        string version = string.Empty;

        var remixMatch = Regex.Match(title, @"\(([^)]*(?:Remix|Bootleg|Edit|Flip|VIP|Mashup)[^)]*)\)", RegexOptions.IgnoreCase);
        if (remixMatch.Success)
        {
            version = remixMatch.Groups[1].Value.Trim();
            cleanTitle = title.Replace(remixMatch.Value, "").Trim();
        }

        var candidate = new MetadataCandidate
        {
            SourceName = "SoundCloud",
            SourceId = Guid.NewGuid().ToString("N")[..8],
            SourceUrl = $"https://soundcloud.com/search/sounds?q={Uri.EscapeDataString($"{artist} {title}")}",
            Artist = artist.Trim(),
            Title = cleanTitle.Trim(),
            Version = version,
            ReleaseTitle = $"{cleanTitle.Trim()} (SoundCloud Release)",
            Label = "Self-Released",
            Genre = "Electronic / Club",
            Year = (uint)DateTime.Now.Year,
            SourceRank = 1
        };

        return Task.FromResult<IReadOnlyList<MetadataCandidate>>(new List<MetadataCandidate> { candidate });
    }
}
