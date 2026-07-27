namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

/// <summary>
/// Resultado global de la verificación posterior de un archivo.
/// </summary>
public sealed class MetadataVerificationResult
{
    public string FilePath { get; init; } =
        string.Empty;

    public bool FileOpened { get; init; }

    public IReadOnlyList<MetadataFieldVerificationResult>
        FieldResults
    { get; init; } =
            Array.Empty<MetadataFieldVerificationResult>();

    public int PictureCountBefore { get; init; }

    public int PictureCountAfter { get; init; }

    public bool PicturesPreserved =>
        PictureCountBefore == PictureCountAfter;

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public int SuccessfulFieldCount =>
        FieldResults.Count(
            result => result.WasSuccessful);

    public int FailedFieldCount =>
        FieldResults.Count - SuccessfulFieldCount;

    public bool WasSuccessful =>
        FileOpened &&
        FieldResults.Count > 0 &&
        FailedFieldCount == 0 &&
        PicturesPreserved;

    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    "Todos los campos fueron verificados y las " +
                    "imágenes incrustadas permanecieron intactas.";
            }

            return
                $"Verificación terminada. Correctos: " +
                $"{SuccessfulFieldCount}. Fallidos: " +
                $"{FailedFieldCount}.";
        }
    }
}