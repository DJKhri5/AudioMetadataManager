using System.Net.Http;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Api;

/// <summary>
/// Construye direcciones y solicitudes HTTP para la API
/// de búsqueda de Spotify.
///
/// No envía solicitudes ni interpreta respuestas.
/// </summary>
public sealed class SpotifyApiRequestBuilder
{
    private readonly SpotifyOptions
        _options;

    public SpotifyApiRequestBuilder(
        SpotifyOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));
    }

    /// <summary>
    /// Construye una solicitud GET para buscar pistas en el
    /// catálogo de Spotify.
    /// </summary>
    public HttpRequestMessage BuildSearchRequest(
        SpotifySearchRequest request,
        string accessToken)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        Uri requestUri =
            BuildSearchUri(
                request);

        HttpRequestMessage httpRequest =
            new(
                HttpMethod.Get,
                requestUri);

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers
                .AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

        return httpRequest;
    }

    /// <summary>
    /// Construye la dirección completa del endpoint
    /// de búsqueda.
    /// </summary>
    public Uri BuildSearchUri(
        SpotifySearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        string query =
            BuildQueryText(
                request);

        int resultsPerPage =
            request.ResultsPerPage ??
            _options.ResultsPerPage;

        List<KeyValuePair<string, string>> parameters =
            new()
            {
                new KeyValuePair<string, string>(
                    "q",
                    query),

                new KeyValuePair<string, string>(
                    "type",
                    "track"),

                new KeyValuePair<string, string>(
                    "limit",
                    Math.Clamp(
                        resultsPerPage,
                        1,
                        50).ToString(
                        System.Globalization
                            .CultureInfo.InvariantCulture))
            };

        string queryString =
            string.Join(
                "&",
                parameters.Select(
                    parameter =>
                        $"{Uri.EscapeDataString(parameter.Key)}=" +
                        $"{Uri.EscapeDataString(parameter.Value)}"));

        Uri endpoint =
            new(
                _options.ApiBaseAddress,
                "search");

        UriBuilder builder =
            new(endpoint)
            {
                Query = queryString
            };

        return builder.Uri;
    }

    private static string BuildQueryText(
        SpotifySearchRequest request)
    {
        List<string> parts =
            new();

        if (!string.IsNullOrWhiteSpace(
                request.Artist))
        {
            parts.Add(
                $"artist:{request.Artist.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(
                request.Title))
        {
            parts.Add(
                $"track:{request.Title.Trim()}");
        }

        if (parts.Count == 0 &&
            !string.IsNullOrWhiteSpace(
                request.Album))
        {
            parts.Add(
                request.Album.Trim());
        }

        return string.Join(
            " ",
            parts);
    }
}
