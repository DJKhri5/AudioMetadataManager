using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

/// <summary>
/// Resultado de verificar un campo después de guardar y
/// volver a abrir el archivo.
/// </summary>
public sealed class MetadataFieldVerificationResult
{
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    public string ExpectedValue { get; init; } =
        string.Empty;

    public string PersistedValue { get; init; } =
        string.Empty;

    public bool IsSupported { get; init; }

    public bool MatchesExpectedValue { get; init; }

    public string Message { get; init; } =
        string.Empty;

    public bool WasSuccessful =>
        IsSupported &&
        MatchesExpectedValue;

    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    $"{Field}: el valor persistido coincide " +
                    "con el solicitado.";
            }

            if (!IsSupported)
            {
                return
                    $"{Field}: campo no soportado por el " +
                    "verificador actual.";
            }

            return
                $"{Field}: el valor persistido no coincide " +
                "con el solicitado.";
        }
    }
}