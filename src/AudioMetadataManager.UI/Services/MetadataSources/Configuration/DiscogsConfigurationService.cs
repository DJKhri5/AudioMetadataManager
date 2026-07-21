using System.Net;
using AudioMetadataManager.UI.Services.MetadataSources
    .Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Api;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;

/// <summary>
/// Proporciona una API interna para administrar la
/// configuración de Discogs.
///
/// La interfaz de usuario no necesita conocer el almacén de
/// credenciales ni cómo se construye DiscogsOptions.
/// </summary>
public sealed class DiscogsConfigurationService
    : IMetadataSourceConfigurationService
{
    private readonly DiscogsTokenStore
        _tokenStore;

    public DiscogsConfigurationService()
        : this(
            new DiscogsTokenStore())
    {
    }

    public DiscogsConfigurationService(
        DiscogsTokenStore tokenStore)
    {
        _tokenStore =
            tokenStore ??
            throw new ArgumentNullException(
                nameof(tokenStore));
    }

    public string SourceName =>
        "Discogs";

    /// <inheritdoc />
    public MetadataSourceConfigurationResult GetStatus()
    {
        try
        {
            bool hasToken =
                _tokenStore.HasToken;

            if (!hasToken)
            {
                return MetadataSourceConfigurationResult.Success(
                    SourceName,
                    MetadataSourceConfigurationState.NotConfigured,
                    "Discogs no tiene un token configurado.");
            }

            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.Configured,
                "Existe un token de Discogs guardado de forma segura.");
        }
        catch (Exception exception)
        {
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                "No fue posible consultar la configuración de Discogs: " +
                exception.Message);
        }
    }

    /// <inheritdoc />
    public MetadataSourceConfigurationResult SaveCredential(
        string credential)
    {
        if (string.IsNullOrWhiteSpace(
                credential))
        {
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.NotConfigured,
                "Introduce un token de Discogs antes de guardarlo.");
        }

        try
        {
            _tokenStore.SaveToken(
                credential);

            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.Configured,
                "El token de Discogs fue guardado de forma segura.");
        }
        catch (Exception exception)
        {
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                "No fue posible guardar el token de Discogs: " +
                exception.Message);
        }
    }

    /// <inheritdoc />
    public MetadataSourceConfigurationResult DeleteCredential()
    {
        try
        {
            _tokenStore.DeleteToken();

            return MetadataSourceConfigurationResult.Success(
                SourceName,
                MetadataSourceConfigurationState.NotConfigured,
                "El token de Discogs fue eliminado.");
        }
        catch (Exception exception)
        {
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                "No fue posible eliminar el token de Discogs: " +
                exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task<MetadataSourceConfigurationResult>
        TestConnectionAsync(
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? token;

        try
        {
            token =
                _tokenStore.ReadToken();
        }
        catch (Exception exception)
        {
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.Error,
                "No fue posible leer el token de Discogs: " +
                exception.Message);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return MetadataSourceConfigurationResult.Failure(
                SourceName,
                MetadataSourceConfigurationState.NotConfigured,
                "Guarda un token antes de probar la conexión.");
        }

        DiscogsOptions options =
            new()
            {
                UserToken =
                    token
            };

        using DiscogsApiClient apiClient =
            new(
                options);

        DiscogsApiResponse response =
            await apiClient.GetIdentityAsync(
                cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.OK =>
                MetadataSourceConfigurationResult.Success(
                    SourceName,
                    MetadataSourceConfigurationState.ConnectionVerified,
                    BuildVerifiedMessage(
                        response)),

            HttpStatusCode.Unauthorized =>
                MetadataSourceConfigurationResult.Failure(
                    SourceName,
                    MetadataSourceConfigurationState.AuthenticationFailed,
                    "Discogs rechazó el token configurado. " +
                    "Comprueba que siga activo y vuelve a guardarlo."),

            HttpStatusCode.Forbidden =>
                MetadataSourceConfigurationResult.Failure(
                    SourceName,
                    MetadataSourceConfigurationState.AuthenticationFailed,
                    "Discogs no autorizó el acceso con el token configurado."),

            HttpStatusCode.TooManyRequests =>
                MetadataSourceConfigurationResult.Failure(
                    SourceName,
                    MetadataSourceConfigurationState.Error,
                    "Discogs limitó temporalmente las solicitudes. " +
                    "Espera unos minutos antes de volver a intentarlo."),

            HttpStatusCode.RequestTimeout =>
                MetadataSourceConfigurationResult.Failure(
                    SourceName,
                    MetadataSourceConfigurationState.Error,
                    "La comprobación de Discogs superó el tiempo máximo permitido."),

            HttpStatusCode.ServiceUnavailable =>
                MetadataSourceConfigurationResult.Failure(
                    SourceName,
                    MetadataSourceConfigurationState.Error,
                    response.Message),

            _ =>
                MetadataSourceConfigurationResult.Failure(
                    SourceName,
                    MetadataSourceConfigurationState.Error,
                    response.Message)
        };
    }
    /// <summary>
    /// Construye un mensaje seguro para una conexión validada.
    ///
    /// No muestra el contenido JSON ni ningún dato sensible.
    /// </summary>
    private static string BuildVerifiedMessage(
        DiscogsApiResponse response)
    {
        string rateLimitText =
            response.RateLimit.Remaining.HasValue
                ? $" Solicitudes restantes: " +
                  $"{response.RateLimit.Remaining.Value}."
                : string.Empty;

        return
            "Discogs confirmó correctamente el token configurado." +
            rateLimitText;
    }
}