using System.Text.Json;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Dtos;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Mapping;

/// <summary>
/// Deserializa respuestas JSON de búsqueda recibidas
/// desde Discogs.
/// </summary>
public sealed class DiscogsSearchResponseParser
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
        out DiscogsSearchResponseDto? response,
        out string errorMessage)
    {
        response = null;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(json))
        {
            errorMessage =
                "La respuesta de Discogs no contiene datos.";

            return false;
        }

        try
        {
            response =
                JsonSerializer.Deserialize<
                    DiscogsSearchResponseDto>(
                        json,
                        SerializerOptions);

            if (response is null)
            {
                errorMessage =
                    "La respuesta de Discogs produjo un resultado vacío.";

                return false;
            }

            return true;
        }
        catch (JsonException exception)
        {
            errorMessage =
                "La respuesta JSON de Discogs no pudo interpretarse: " +
                exception.Message;

            return false;
        }
        catch (NotSupportedException exception)
        {
            errorMessage =
                "La estructura recibida desde Discogs no es compatible: " +
                exception.Message;

            return false;
        }
    }
}