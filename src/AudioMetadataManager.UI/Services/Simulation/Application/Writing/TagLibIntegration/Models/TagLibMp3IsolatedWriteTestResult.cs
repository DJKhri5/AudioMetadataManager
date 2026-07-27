using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

/// <summary>
/// Resultado completo de una prueba real de escritura MP3
/// realizada exclusivamente sobre una copia aislada.
/// </summary>
public sealed class TagLibMp3IsolatedWriteTestResult
{
    /// <summary>
    /// Ruta del archivo original, que nunca debe modificarse.
    /// </summary>
    public string OriginalFilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta de la copia utilizada para la escritura real.
    /// </summary>
    public string WorkingCopyPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta del respaldo obligatorio de la copia de trabajo.
    /// </summary>
    public string WorkingBackupPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Carpeta temporal completa de la prueba.
    /// </summary>
    public string TestDirectoryPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Género almacenado originalmente en la copia.
    /// </summary>
    public string OriginalGenre { get; init; } =
        string.Empty;

    /// <summary>
    /// Género solicitado para la prueba.
    /// </summary>
    public string RequestedGenre { get; init; } =
        string.Empty;

    /// <summary>
    /// Género leído después de guardar y reabrir la copia.
    /// </summary>
    public string PersistedGenre { get; init; } =
        string.Empty;

    /// <summary>
    /// Cantidad de imágenes en la copia antes de escribir.
    /// </summary>
    public int PictureCountBefore { get; init; }

    /// <summary>
    /// Cantidad de imágenes después de guardar.
    /// </summary>
    public int PictureCountAfter { get; init; }

    /// <summary>
    /// Hash SHA-256 del archivo original antes de la prueba.
    /// </summary>
    public string OriginalHashBefore { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash SHA-256 del archivo original después de la prueba.
    /// </summary>
    public string OriginalHashAfter { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash inicial de la copia de trabajo.
    /// </summary>
    public string WorkingCopyHashBefore { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash de la copia después de escribir.
    /// </summary>
    public string WorkingCopyHashAfter { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash del respaldo de la copia de trabajo.
    /// </summary>
    public string WorkingBackupHash { get; init; } =
        string.Empty;

    /// <summary>
    /// Resultado producido por el escritor real.
    /// </summary>
    public MetadataWriteResult? WriteResult { get; init; }

    /// <summary>
    /// Mensajes generales de la prueba.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si el archivo original quedó intacto.
    /// </summary>
    public bool OriginalFileRemainedUnchanged =>
        !string.IsNullOrWhiteSpace(OriginalHashBefore) &&
        string.Equals(
            OriginalHashBefore,
            OriginalHashAfter,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si el respaldo coincide con la copia antes de
    /// efectuar la escritura.
    /// </summary>
    public bool BackupMatchesInitialWorkingCopy =>
        !string.IsNullOrWhiteSpace(WorkingCopyHashBefore) &&
        string.Equals(
            WorkingCopyHashBefore,
            WorkingBackupHash,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si la copia fue realmente modificada.
    /// </summary>
    public bool WorkingCopyWasModified =>
        !string.IsNullOrWhiteSpace(WorkingCopyHashBefore) &&
        !string.IsNullOrWhiteSpace(WorkingCopyHashAfter) &&
        !string.Equals(
            WorkingCopyHashBefore,
            WorkingCopyHashAfter,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si el género persistido coincide con el solicitado.
    /// </summary>
    public bool GenreWasPersisted =>
        string.Equals(
            Normalize(PersistedGenre),
            Normalize(RequestedGenre),
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si las carátulas se conservaron.
    /// </summary>
    public bool PicturesWerePreserved =>
        PictureCountBefore == PictureCountAfter;

    /// <summary>
    /// Resultado global de la prueba.
    /// </summary>
    public bool WasSuccessful =>
        WriteResult?.WasSuccessful == true &&
        OriginalFileRemainedUnchanged &&
        BackupMatchesInitialWorkingCopy &&
        WorkingCopyWasModified &&
        GenreWasPersisted &&
        PicturesWerePreserved;

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
                    "La escritura real sobre la copia aislada " +
                    "terminó correctamente y el archivo original " +
                    "permaneció intacto.";
            }

            return
                "La prueba aislada no superó todas las " +
                "comprobaciones de seguridad y persistencia.";
        }
    }

    private static string Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }
}