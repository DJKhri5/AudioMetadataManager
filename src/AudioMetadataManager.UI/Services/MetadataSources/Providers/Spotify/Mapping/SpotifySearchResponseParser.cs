using System.Text.Json;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Mapping;

/// <summary>
/// Deserializa respuestas JSON de búsqueda recibidas
/// desde Spotify.
/// </summary>
public sealed class SpotifySearchResponseParser
{
    private static readonly JsonSerializerOptions
        SerializerOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

    /// <summary>
    /// Intenta interpretar una respuesta JSON.
    /// </summary>
    public bool TryParse(
        string json,
        out SpotifySearchResponseDto? response,
        out string errorMessage)
    {
        response = null;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(json))
        {
            errorMessage =
                "La respuesta de Spotify no contiene datos.";

            return false;
        }

        try
        {
            response =
                JsonSerializer.Deserialize<
                    SpotifySearchResponseDto>(
                        json,
                        SerializerOptions);

            if (response is null)
            {
                errorMessage =
                    "La respuesta de Spotify produjo un resultado " +
                    "vacío.";

                return false;
            }

            return true;
        }
        catch (JsonException exception)
        {
            errorMessage =
                "La respuesta JSON de Spotify no pudo " +
                $"interpretarse: {exception.Message}";

            return false;
        }
        catch (NotSupportedException exception)
        {
            errorMessage =
                "La estructura recibida desde Spotify no es " +
                $"compatible: {exception.Message}";

            return false;
        }
    }

    /// <summary>
    /// Intenta interpretar un cuerpo de error de la API de
    /// búsqueda.
    /// </summary>
    public bool TryParseError(
        string json,
        out SpotifyApiErrorResponseDto? errorResponse)
    {
        errorResponse = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            errorResponse =
                JsonSerializer.Deserialize<
                    SpotifyApiErrorResponseDto>(
                        json,
                        SerializerOptions);

            return errorResponse?.Error is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
