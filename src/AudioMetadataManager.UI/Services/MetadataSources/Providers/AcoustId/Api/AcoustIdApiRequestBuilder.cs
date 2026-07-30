using System.Globalization;
using System.Net.Http;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Api;

/// <summary>
/// Construye direcciones y solicitudes HTTP para la API
/// de AcoustID.
///
/// No envía solicitudes ni interpreta respuestas.
/// </summary>
public sealed class AcoustIdApiRequestBuilder
{
    private readonly AcoustIdOptions
        _options;

    public AcoustIdApiRequestBuilder(
        AcoustIdOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));
    }

    /// <summary>
    /// Construye una solicitud GET para identificar una
    /// huella acústica.
    /// </summary>
    public HttpRequestMessage BuildLookupRequest(
        AcoustIdLookupRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        Uri requestUri =
            BuildLookupUri(
                request);

        return new HttpRequestMessage(
            HttpMethod.Get,
            requestUri);
    }

    /// <summary>
    /// Construye la dirección completa del endpoint
    /// de identificación.
    /// </summary>
    public Uri BuildLookupUri(
        AcoustIdLookupRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        List<KeyValuePair<string, string>> parameters =
            new()
            {
                new KeyValuePair<string, string>(
                    "client",
                    _options.ClientApiKey ??
                        string.Empty),

                new KeyValuePair<string, string>(
                    "fingerprint",
                    request.Fingerprint),

                new KeyValuePair<string, string>(
                    "duration",
                    request.DurationSeconds.ToString(
                        CultureInfo.InvariantCulture)),

                new KeyValuePair<string, string>(
                    "meta",
                    _options.MetaFields)
            };

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
                "lookup");

        UriBuilder builder =
            new(endpoint)
            {
                Query = query
            };

        return builder.Uri;
    }
}
