using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;
using System.Globalization;
using System.Net.Http;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Api;

/// <summary>
/// Construye direcciones y solicitudes HTTP para la API
/// de Discogs.
///
/// No envía solicitudes ni interpreta respuestas.
/// </summary>
public sealed class DiscogsApiRequestBuilder
{
    /// <summary>
    /// Construye una solicitud autenticada para comprobar la
    /// identidad asociada al token configurado.
    ///
    /// El token será agregado posteriormente por
    /// DiscogsAuthenticationHandler.
    /// </summary>
    public HttpRequestMessage BuildIdentityRequest()
    {
        Uri requestUri =
            new(
                _options.BaseAddress,
                "oauth/identity");

        return new HttpRequestMessage(
            HttpMethod.Get,
            requestUri);
    }

    private readonly DiscogsOptions _options;

    public DiscogsApiRequestBuilder(
        DiscogsOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));
    }

    /// <summary>
    /// Construye una solicitud GET para buscar publicaciones
    /// en la base de datos de Discogs.
    /// </summary>
    public HttpRequestMessage BuildDatabaseSearchRequest(
        DiscogsSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        Uri requestUri =
            BuildDatabaseSearchUri(
                request);

        return new HttpRequestMessage(
            HttpMethod.Get,
            requestUri);
    }

    /// <summary>
    /// Construye la dirección completa del endpoint
    /// de búsqueda.
    /// </summary>
    public Uri BuildDatabaseSearchUri(
        DiscogsSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        List<KeyValuePair<string, string>> parameters =
            new();

        AddIfAvailable(
            parameters,
            "artist",
            request.Artist);

        AddIfAvailable(
            parameters,
            "track",
            BuildTrackSearchValue(
                request.Title,
                request.Version));

        AddIfAvailable(
            parameters,
            "release_title",
            request.Album);

        if (request.Year.HasValue &&
            request.Year.Value > 0)
        {
            parameters.Add(
                new KeyValuePair<string, string>(
                    "year",
                    request.Year.Value.ToString(
                        CultureInfo.InvariantCulture)));
        }

        parameters.Add(
            new KeyValuePair<string, string>(
                "type",
                "release"));

        parameters.Add(
            new KeyValuePair<string, string>(
                "page",
                Math.Max(
                    1,
                    request.Page)
                .ToString(
                    CultureInfo.InvariantCulture)));

        int resultsPerPage =
            request.ResultsPerPage ??
            _options.ResultsPerPage;

        parameters.Add(
            new KeyValuePair<string, string>(
                "per_page",
                Math.Clamp(
                    resultsPerPage,
                    1,
                    100)
                .ToString(
                    CultureInfo.InvariantCulture)));

        string query =
            string.Join(
                "&",
                parameters.Select(
                    parameter =>
                        $"{Uri.EscapeDataString(parameter.Key)}=" +
                        $"{Uri.EscapeDataString(parameter.Value)}"));

        Uri endpoint =
            new(
                _options.BaseAddress,
                "database/search");

        UriBuilder builder =
            new(endpoint)
            {
                Query = query
            };

        return builder.Uri;
    }

    private static string? BuildTrackSearchValue(
        string? title,
        string? version)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        string normalizedTitle =
            title.Trim();

        if (string.IsNullOrWhiteSpace(version))
        {
            return normalizedTitle;
        }

        return
            $"{normalizedTitle} ({version.Trim()})";
    }

    private static void AddIfAvailable(
        ICollection<KeyValuePair<string, string>> parameters,
        string name,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        parameters.Add(
            new KeyValuePair<string, string>(
                name,
                value.Trim()));
    }
}