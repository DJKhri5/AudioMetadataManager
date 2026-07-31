using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Api;

/// <summary>
/// Obtiene y conserva en memoria el token de acceso de Spotify,
/// renovándolo automáticamente cuando expira.
///
/// Utiliza el flujo "Client Credentials": no requiere que el
/// usuario inicie sesión, sólo el identificador y el secreto de
/// cliente configurados.
/// </summary>
public sealed class SpotifyAuthClient : IDisposable
{
    private static readonly JsonSerializerOptions
        SerializerOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

    private readonly SpotifyOptions
        _options;

    private readonly HttpClient
        _httpClient;

    private readonly bool
        _ownsHttpClient;

    private readonly SemaphoreSlim
        _tokenLock =
            new(1, 1);

    private SpotifyAccessToken?
        _cachedToken;

    private bool
        _disposed;

    /// <summary>
    /// Crea un cliente con la infraestructura HTTP
    /// predeterminada.
    /// </summary>
    public SpotifyAuthClient(
        SpotifyOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _httpClient =
            new HttpClient
            {
                Timeout =
                    options.RequestTimeout
            };

        _ownsHttpClient =
            true;
    }

    /// <summary>
    /// Crea un cliente usando un HttpClient externo.
    ///
    /// Este constructor permite pruebas e inyección
    /// de dependencias.
    /// </summary>
    public SpotifyAuthClient(
        SpotifyOptions options,
        HttpClient httpClient)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        _httpClient =
            httpClient ??
            throw new ArgumentNullException(
                nameof(httpClient));

        _ownsHttpClient =
            false;
    }

    /// <summary>
    /// Obtiene un token de acceso válido, reutilizando el token
    /// en memoria mientras no haya expirado.
    /// </summary>
    public async Task<SpotifyAuthResult> GetAccessTokenAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        if (_cachedToken?.IsValid == true)
        {
            return new SpotifyAuthResult
            {
                Status =
                    SpotifyProviderStatus.Success,

                Token =
                    _cachedToken,

                Message =
                    "Se reutilizó el token vigente."
            };
        }

        await _tokenLock.WaitAsync(
            cancellationToken);

        try
        {
            if (_cachedToken?.IsValid == true)
            {
                return new SpotifyAuthResult
                {
                    Status =
                        SpotifyProviderStatus.Success,

                    Token =
                        _cachedToken,

                    Message =
                        "Se reutilizó el token vigente."
                };
            }

            SpotifyAuthResult result =
                await RequestNewTokenAsync(
                    cancellationToken);

            if (result.IsSuccess)
            {
                _cachedToken =
                    result.Token;
            }

            return result;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<SpotifyAuthResult> RequestNewTokenAsync(
        CancellationToken cancellationToken)
    {
        if (!_options.HasCredentials)
        {
            return new SpotifyAuthResult
            {
                Status =
                    SpotifyProviderStatus.InvalidConfiguration,

                Message =
                    "El proveedor Spotify requiere un Client ID " +
                    "y un Client Secret."
            };
        }

        using HttpRequestMessage request =
            new(
                HttpMethod.Post,
                _options.TokenAddress);

        string basicAuthValue =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{_options.ClientId}:{_options.ClientSecret}"));

        request.Headers.Authorization =
            new System.Net.Http.Headers
                .AuthenticationHeaderValue(
                    "Basic",
                    basicAuthValue);

        request.Content =
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] =
                        "client_credentials"
                });

        try
        {
            using HttpResponseMessage response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            string content =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new SpotifyAuthResult
                {
                    Status =
                        response.StatusCode ==
                            HttpStatusCode.Unauthorized ||
                        response.StatusCode ==
                            HttpStatusCode.BadRequest
                            ? SpotifyProviderStatus
                                .AuthenticationFailed
                            : SpotifyProviderStatus.NetworkError,

                    Message =
                        BuildTokenErrorMessage(
                            content)
                };
            }

            SpotifyTokenResponseDto? tokenResponse =
                JsonSerializer.Deserialize<
                    SpotifyTokenResponseDto>(
                        content,
                        SerializerOptions);

            if (tokenResponse is null ||
                string.IsNullOrWhiteSpace(
                    tokenResponse.AccessToken))
            {
                return new SpotifyAuthResult
                {
                    Status =
                        SpotifyProviderStatus.InvalidResponse,

                    Message =
                        "La respuesta de autenticación de " +
                        "Spotify no contiene un token."
                };
            }

            SpotifyAccessToken accessToken =
                new()
                {
                    Value =
                        tokenResponse.AccessToken,

                    ExpiresAtUtc =
                        DateTimeOffset.UtcNow +
                        TimeSpan.FromSeconds(
                            Math.Max(
                                0,
                                tokenResponse.ExpiresInSeconds))
                };

            return new SpotifyAuthResult
            {
                Status =
                    SpotifyProviderStatus.Success,

                Token =
                    accessToken,

                Message =
                    "Se obtuvo un nuevo token de acceso."
            };
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return new SpotifyAuthResult
            {
                Status =
                    SpotifyProviderStatus.NetworkError,

                Message =
                    "La solicitud de autenticación superó el " +
                    "tiempo máximo permitido."
            };
        }
        catch (HttpRequestException exception)
        {
            return new SpotifyAuthResult
            {
                Status =
                    SpotifyProviderStatus.NetworkError,

                Message =
                    "No fue posible conectar con Spotify para " +
                    $"autenticar: {exception.Message}"
            };
        }
        catch (JsonException exception)
        {
            return new SpotifyAuthResult
            {
                Status =
                    SpotifyProviderStatus.InvalidResponse,

                Message =
                    "La respuesta de autenticación de Spotify no " +
                    $"pudo interpretarse: {exception.Message}"
            };
        }
    }

    private static string BuildTokenErrorMessage(
        string content)
    {
        try
        {
            SpotifyTokenErrorDto? errorDto =
                JsonSerializer.Deserialize<
                    SpotifyTokenErrorDto>(
                        content,
                        SerializerOptions);

            if (errorDto is not null &&
                !string.IsNullOrWhiteSpace(
                    errorDto.ErrorDescription))
            {
                return "Spotify rechazó la autenticación: " +
                    errorDto.ErrorDescription;
            }
        }
        catch (JsonException)
        {
        }

        return "Spotify rechazó las credenciales de cliente " +
            "configuradas.";
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        _tokenLock.Dispose();

        _disposed =
            true;

        GC.SuppressFinalize(
            this);
    }
}
