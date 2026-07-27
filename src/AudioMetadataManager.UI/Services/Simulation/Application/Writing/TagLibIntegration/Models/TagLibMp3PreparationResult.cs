namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

/// <summary>
/// Contiene el resultado completo de preparar cambios MP3
/// exclusivamente en memoria.
///
/// Ningún resultado de esta clase implica que se haya ejecutado
/// TagLib.File.Save().
/// </summary>
public sealed class TagLibMp3PreparationResult
{
    /// <summary>
    /// Ruta del archivo inspeccionado.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si TagLibSharp pudo abrir el archivo.
    /// </summary>
    public bool FileOpened { get; init; }

    /// <summary>
    /// Resultados individuales.
    /// </summary>
    public IReadOnlyList<TagLibMp3FieldPreparationResult>
        FieldResults
    { get; init; } =
            Array.Empty<TagLibMp3FieldPreparationResult>();

    /// <summary>
    /// Cantidad de campos preparados correctamente.
    /// </summary>
    public int SuccessfulFieldCount =>
        FieldResults.Count(
            result => result.WasSuccessful);

    /// <summary>
    /// Cantidad de campos que no pudieron prepararse.
    /// </summary>
    public int FailedFieldCount =>
        FieldResults.Count - SuccessfulFieldCount;

    /// <summary>
    /// Cantidad de imágenes antes de preparar los cambios.
    /// </summary>
    public int PictureCountBefore { get; init; }

    /// <summary>
    /// Cantidad de imágenes después de preparar los cambios.
    /// </summary>
    public int PictureCountAfter { get; init; }

    /// <summary>
    /// Indica si se conservaron las imágenes en memoria.
    /// </summary>
    public bool PicturesPreserved =>
        PictureCountBefore == PictureCountAfter;

    /// <summary>
    /// Indica expresamente que no se ejecutó Save().
    /// </summary>
    public bool SaveWasExecuted { get; init; }

    /// <summary>
    /// Indica si al volver a abrir el archivo sus valores físicos
    /// continuaban iguales a los originales.
    /// </summary>
    public bool PhysicalFileRemainedUnchanged { get; init; }

    /// <summary>
    /// Mensajes generales.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Resultado global.
    /// </summary>
    public bool WasSuccessful =>
        FileOpened &&
        FieldResults.Count > 0 &&
        FailedFieldCount == 0 &&
        PicturesPreserved &&
        !SaveWasExecuted &&
        PhysicalFileRemainedUnchanged;

    /// <summary>
    /// Resumen compacto.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    "Los cambios fueron preparados en memoria y " +
                    "el archivo físico permaneció intacto.";
            }

            return
                $"Preparación terminada. Correctos: " +
                $"{SuccessfulFieldCount}. Fallidos: " +
                $"{FailedFieldCount}. Archivo intacto: " +
                $"{(PhysicalFileRemainedUnchanged ? "Sí" : "No")}.";
        }
    }
}