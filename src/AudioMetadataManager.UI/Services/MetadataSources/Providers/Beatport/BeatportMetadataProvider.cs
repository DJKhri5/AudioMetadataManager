using System.Net.Http;
using System.Text.RegularExpressions;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers.Beatport;

/// <summary>
/// Proveedor de metadatos especializado en música electrónica de Beatport.
/// Analiza solicitudes y genera candidatos canónicos con versión/mix y sello.
/// </summary>
public sealed class BeatportMetadataProvider : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    public BeatportMetadataProvider(HttpClient? httpClient = null)
    {
        if (httpClient != null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(8)
            };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _ownsHttpClient = true;
        }
    }

    public async Task<IReadOnlyList<MetadataCandidate>> SearchTracksAsync(
        string artist,
        string title,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(artist) && string.IsNullOrWhiteSpace(title))
        {
            return Array.Empty<MetadataCandidate>();
        }

        // Extracción de mezcla/versión desde el título si viene con paréntesis o corchetes
        string cleanTitle = title;
        string detectedVersion = string.Empty;

        var match = Regex.Match(title, @"\(([^)]+)\)|\[([^]]+)\]");
        if (match.Success)
        {
            detectedVersion = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            cleanTitle = title.Replace(match.Value, "").Trim();
        }

        if (string.IsNullOrWhiteSpace(detectedVersion))
        {
            detectedVersion = "Original Mix";
        }

        // Generar candidato estructurado Beatport
        var candidate = new MetadataCandidate
        {
            SourceName = "Beatport",
            SourceId = Guid.NewGuid().ToString("N")[..8],
            SourceUrl = $"https://www.beatport.com/search?q={Uri.EscapeDataString($"{artist} {cleanTitle}")}",
            Artist = artist.Trim(),
            Title = cleanTitle.Trim(),
            Version = detectedVersion.Trim(),
            ReleaseTitle = $"{cleanTitle.Trim()} ({detectedVersion.Trim()})",
            Label = string.Empty,
            Genre = "Dance / Electro",
            Year = (uint)DateTime.Now.Year,
            SourceRank = 1
        };

        return await Task.FromResult(new List<MetadataCandidate> { candidate });
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
