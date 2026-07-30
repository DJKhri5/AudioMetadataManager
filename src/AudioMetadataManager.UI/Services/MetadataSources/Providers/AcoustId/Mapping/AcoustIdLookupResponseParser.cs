using System.Text.Json;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Dtos;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Mapping;

/// <summary>
/// Deserializa respuestas JSON de identificación recibidas
/// desde AcoustID.
/// </summary>
public sealed class AcoustIdLookupResponseParser
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
        out AcoustIdLookupResponseDto? response,
        out string errorMessage)
    {
        response = null;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(json))
        {
            errorMessage =
                "La respuesta de AcoustID no contiene datos.";

            return false;
        }

        try
        {
            response =
                JsonSerializer.Deserialize<
                    AcoustIdLookupResponseDto>(
                        json,
                        SerializerOptions);

            if (response is null)
            {
                errorMessage =
                    "La respuesta de AcoustID produjo un resultado vacío.";

                return false;
            }

            return true;
        }
        catch (JsonException exception)
        {
            errorMessage =
                "La respuesta JSON de AcoustID no pudo interpretarse: " +
                exception.Message;

            return false;
        }
        catch (NotSupportedException exception)
        {
            errorMessage =
                "La estructura recibida desde AcoustID no es compatible: " +
                exception.Message;

            return false;
        }
    }
}
