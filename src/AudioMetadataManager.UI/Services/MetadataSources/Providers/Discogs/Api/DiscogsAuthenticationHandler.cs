using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Api;

/// <summary>
/// Agrega a cada solicitud los encabezados comunes y la
/// autenticación configurada para Discogs.
/// </summary>
public sealed class DiscogsAuthenticationHandler
    : DelegatingHandler
{
    private readonly DiscogsOptions _options;

    public DiscogsAuthenticationHandler(
        DiscogsOptions options,
        HttpMessageHandler innerHandler)
        : base(
            innerHandler ??
            throw new ArgumentNullException(
                nameof(innerHandler)))
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        AddUserAgent(
            request);

        AddAuthentication(
            request);

        request.Headers.Accept.Clear();

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));

        return base.SendAsync(
            request,
            cancellationToken);
    }

    private void AddUserAgent(
        HttpRequestMessage request)
    {
        request.Headers.UserAgent.Clear();

        request.Headers.UserAgent.ParseAdd(
            _options.UserAgent);
    }

    private void AddAuthentication(
        HttpRequestMessage request)
    {
        if (!_options.HasUserToken)
        {
            return;
        }

        string token =
            _options.UserToken!.Trim();

        request.Headers.TryAddWithoutValidation(
            "Authorization",
            $"Discogs token={token}");
    }
}