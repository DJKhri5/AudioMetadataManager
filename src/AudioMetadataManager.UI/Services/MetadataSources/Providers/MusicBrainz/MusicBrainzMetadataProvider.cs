using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers.MusicBrainz;

/// <summary>
/// Cliente HTTP para consultar la API pública abierta de MusicBrainz.
/// </summary>
public sealed class MusicBrainzMetadataProvider : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    public MusicBrainzMetadataProvider(HttpClient? httpClient = null)
    {
        if (httpClient != null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://musicbrainz.org/ws/2/"),
                Timeout = TimeSpan.FromSeconds(10)
            };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AudioMetadataManager/1.0.0 ( contact@audiometadatamanager.local )");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            _ownsHttpClient = true;
        }
    }

    public async Task<IReadOnlyList<MetadataCandidate>> SearchRecordingsAsync(
        string artist,
        string title,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(artist) && string.IsNullOrWhiteSpace(title))
        {
            return Array.Empty<MetadataCandidate>();
        }

        string luceneQuery;
        if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(title))
        {
            luceneQuery = $"recording:\"{EscapeLucene(title)}\" AND artist:\"{EscapeLucene(artist)}\"";
        }
        else if (!string.IsNullOrWhiteSpace(title))
        {
            luceneQuery = $"recording:\"{EscapeLucene(title)}\"";
        }
        else
        {
            luceneQuery = $"artist:\"{EscapeLucene(artist)}\"";
        }

        string requestUri = $"recording?query={Uri.EscapeDataString(luceneQuery)}&limit=10&fmt=json";

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<MetadataCandidate>();
            }

            var jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var searchResponse = await JsonSerializer.DeserializeAsync<MusicBrainzSearchResponse>(
                jsonStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken).ConfigureAwait(false);

            if (searchResponse?.Recordings == null || searchResponse.Recordings.Count == 0)
            {
                return Array.Empty<MetadataCandidate>();
            }

            var candidates = new List<MetadataCandidate>(searchResponse.Recordings.Count);
            int rank = 1;

            foreach (var rec in searchResponse.Recordings)
            {
                string artistName = rec.ArtistCredit != null && rec.ArtistCredit.Count > 0
                    ? string.Join(" & ", rec.ArtistCredit.Select(a => a.Name).Where(n => !string.IsNullOrWhiteSpace(n)))
                    : artist;

                string releaseTitle = string.Empty;
                string labelName = string.Empty;
                uint year = 0;

                if (rec.Releases != null && rec.Releases.Count > 0)
                {
                    var firstRelease = rec.Releases[0];
                    releaseTitle = firstRelease.Title ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(firstRelease.Date) && firstRelease.Date.Length >= 4 &&
                        uint.TryParse(firstRelease.Date[..4], out uint parsedYear))
                    {
                        year = parsedYear;
                    }

                    if (firstRelease.LabelInfo != null && firstRelease.LabelInfo.Count > 0)
                    {
                        labelName = firstRelease.LabelInfo[0].Label?.Name ?? string.Empty;
                    }
                }

                TimeSpan duration = rec.Length.HasValue && rec.Length.Value > 0
                    ? TimeSpan.FromMilliseconds(rec.Length.Value)
                    : TimeSpan.Zero;

                candidates.Add(new MetadataCandidate
                {
                    SourceName = "MusicBrainz",
                    SourceId = rec.Id ?? string.Empty,
                    SourceUrl = !string.IsNullOrWhiteSpace(rec.Id) ? $"https://musicbrainz.org/recording/{rec.Id}" : string.Empty,
                    Artist = artistName,
                    Title = rec.Title ?? title,
                    ReleaseTitle = releaseTitle,
                    Label = labelName,
                    Year = year,
                    Duration = duration,
                    SourceRank = rank++
                });
            }

            return candidates;
        }
        catch
        {
            // Resiliente ante desconexión de red o errores de API
            return Array.Empty<MetadataCandidate>();
        }
    }

    private static string EscapeLucene(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Caracteres especiales de Lucene
        char[] special = { '+', '-', '&', '|', '!', '(', ')', '{', '}', '[', ']', '^', '"', '~', '*', '?', ':', '\\', '/' };
        var sb = new System.Text.StringBuilder(value.Length);
        foreach (char c in value)
        {
            if (special.Contains(c))
            {
                sb.Append('\\');
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    // Modelos de deserialización interna
    private sealed class MusicBrainzSearchResponse
    {
        public List<MusicBrainzRecording>? Recordings { get; set; }
    }

    private sealed class MusicBrainzRecording
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public int? Length { get; set; }

        [JsonPropertyName("artist-credit")]
        public List<MusicBrainzArtistCredit>? ArtistCredit { get; set; }

        public List<MusicBrainzRelease>? Releases { get; set; }
    }

    private sealed class MusicBrainzArtistCredit
    {
        public string? Name { get; set; }
    }

    private sealed class MusicBrainzRelease
    {
        public string? Title { get; set; }
        public string? Date { get; set; }

        [JsonPropertyName("label-info")]
        public List<MusicBrainzLabelInfo>? LabelInfo { get; set; }
    }

    private sealed class MusicBrainzLabelInfo
    {
        public MusicBrainzLabel? Label { get; set; }
    }

    private sealed class MusicBrainzLabel
    {
        public string? Name { get; set; }
    }
}
