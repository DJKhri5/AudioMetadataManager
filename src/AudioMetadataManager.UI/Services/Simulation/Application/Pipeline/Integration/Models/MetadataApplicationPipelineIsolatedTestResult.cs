using System.IO;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Integration.Models;

/// <summary>
/// Contiene el resultado completo de una prueba end-to-end del
/// pipeline de aplicación, ejecutada exclusivamente sobre una
/// copia aislada.
///
/// Conserva instantáneas de las verificaciones realizadas antes
/// de eliminar el entorno temporal.
/// </summary>
public sealed class
    MetadataApplicationPipelineIsolatedTestResult
{
    /// <summary>
    /// Cantidad de etapas obligatorias esperadas.
    /// </summary>
    public const int ExpectedStageCount = 4;

    /// <summary>
    /// Ruta del archivo original, que nunca debe modificarse.
    /// </summary>
    public string OriginalFilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta de la copia sobre la que se ejecutó el pipeline.
    /// </summary>
    public string WorkingCopyPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta del respaldo creado por el harness de aislamiento.
    /// </summary>
    public string WorkingBackupPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta del respaldo obligatorio creado por el pipeline.
    ///
    /// Puede dejar de existir después de limpiar la prueba.
    /// </summary>
    public string PipelineBackupFilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Carpeta temporal completa utilizada por la prueba.
    /// </summary>
    public string TestDirectoryPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Género existente antes de ejecutar el pipeline.
    /// </summary>
    public string OriginalGenre { get; init; } =
        string.Empty;

    /// <summary>
    /// Género solicitado por la prueba.
    /// </summary>
    public string RequestedGenre { get; init; } =
        string.Empty;

    /// <summary>
    /// Género recuperado durante la verificación posterior.
    /// </summary>
    public string PersistedGenre { get; init; } =
        string.Empty;

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
    /// Hash de la copia aislada antes de aplicar los cambios.
    /// </summary>
    public string WorkingCopyHashBefore { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash de la copia después de aplicar los cambios.
    /// </summary>
    public string WorkingCopyHashAfter { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash del respaldo creado por el harness.
    /// </summary>
    public string WorkingBackupHash { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash del respaldo obligatorio creado por el pipeline.
    /// </summary>
    public string PipelineBackupHash { get; init; } =
        string.Empty;

    /// <summary>
    /// Cantidad de imágenes observada antes de escribir.
    /// </summary>
    public int PictureCountBefore { get; init; }

    /// <summary>
    /// Cantidad de imágenes observada después de escribir.
    /// </summary>
    public int PictureCountAfter { get; init; }

    /// <summary>
    /// Resultado completo producido por el ejecutor.
    ///
    /// Se conserva para diagnóstico, pero el éxito final no debe
    /// volver a consultar la existencia física del respaldo.
    /// </summary>
    public MetadataApplicationPipelineExecutionResult?
        PipelineExecutionResult
    { get; init; }

    /// <summary>
    /// Indica si el entorno aislado fue preparado correctamente.
    /// </summary>
    public bool EnvironmentWasPrepared { get; init; }

    /// <summary>
    /// Cantidad de etapas registradas en la ejecución.
    /// </summary>
    public int RegisteredStageCount { get; init; }

    /// <summary>
    /// Cantidad de etapas que llegaron a ejecutarse.
    /// </summary>
    public int ExecutedStageCount { get; init; }

    /// <summary>
    /// Instantánea del resultado general del ejecutor.
    /// </summary>
    public bool PipelineExecutionWasSuccessful { get; init; }

    /// <summary>
    /// Instantánea del resultado de la escritura.
    /// </summary>
    public bool WriteWasSuccessful { get; init; }

    /// <summary>
    /// Instantánea del resultado global de verificación.
    /// </summary>
    public bool VerificationWasSuccessful { get; init; }

    /// <summary>
    /// Instantánea del resultado individual de Genre.
    /// </summary>
    public bool GenreVerificationWasSuccessful { get; init; }

    /// <summary>
    /// Instantánea del éxito del respaldo antes de limpiar.
    /// </summary>
    public bool PipelineBackupWasSuccessfulBeforeCleanup
    { get; init; }

    /// <summary>
    /// Indica si se intentó eliminar el entorno temporal.
    /// </summary>
    public bool CleanupWasAttempted { get; init; }

    /// <summary>
    /// Indica si la carpeta temporal dejó de existir.
    ///
    /// Este valor debe capturarse después del intento de limpieza.
    /// </summary>
    public bool TestDirectoryWasRemoved { get; init; }

    /// <summary>
    /// Momento UTC en que comenzó la prueba.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC en que terminó la prueba.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }

    /// <summary>
    /// Duración total de la prueba, incluida la limpieza.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Mensaje principal de error o excepción.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Tipo de excepción capturada, si corresponde.
    /// </summary>
    public string ExceptionType { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes de diagnóstico producidos por la prueba.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si las cuatro etapas obligatorias fueron recorridas.
    /// </summary>
    public bool AllExpectedStagesWereExecuted =>
        RegisteredStageCount == ExpectedStageCount &&
        ExecutedStageCount == ExpectedStageCount;

    /// <summary>
    /// Indica si el archivo original permaneció intacto.
    /// </summary>
    public bool OriginalFileRemainedUnchanged =>
        HashesMatch(
            OriginalHashBefore,
            OriginalHashAfter);

    /// <summary>
    /// Indica si el respaldo del harness representa la copia
    /// aislada antes de escribir.
    /// </summary>
    public bool WorkingBackupMatchedInitialWorkingCopy =>
        HashesMatch(
            WorkingCopyHashBefore,
            WorkingBackupHash);

    /// <summary>
    /// Indica si el respaldo del pipeline representa la copia
    /// aislada antes de escribir.
    /// </summary>
    public bool PipelineBackupMatchedWorkingCopyBeforeWrite =>
        HashesMatch(
            WorkingCopyHashBefore,
            PipelineBackupHash);

    /// <summary>
    /// Indica si la copia aislada fue realmente modificada.
    /// </summary>
    public bool WorkingCopyWasModified =>
        !string.IsNullOrWhiteSpace(
            WorkingCopyHashBefore) &&
        !string.IsNullOrWhiteSpace(
            WorkingCopyHashAfter) &&
        !string.Equals(
            WorkingCopyHashBefore,
            WorkingCopyHashAfter,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si el género recuperado coincide con el solicitado.
    /// </summary>
    public bool GenreWasPersisted =>
        string.Equals(
            Normalize(RequestedGenre),
            Normalize(PersistedGenre),
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si la cantidad de imágenes permaneció intacta.
    /// </summary>
    public bool PicturesWerePreserved =>
        PictureCountBefore == PictureCountAfter;

    /// <summary>
    /// Indica si los dos mecanismos de respaldo utilizaron
    /// archivos diferentes.
    /// </summary>
    public bool BackupsAreIndependent =>
        !string.IsNullOrWhiteSpace(
            WorkingBackupPath) &&
        !string.IsNullOrWhiteSpace(
            PipelineBackupFilePath) &&
        !string.Equals(
            NormalizePath(WorkingBackupPath),
            NormalizePath(PipelineBackupFilePath),
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si el entorno temporal fue limpiado correctamente.
    /// </summary>
    public bool CleanupWasSuccessful =>
        CleanupWasAttempted &&
        TestDirectoryWasRemoved;

    /// <summary>
    /// Indica si la prueba superó todas las comprobaciones
    /// funcionales y de seguridad.
    /// </summary>
    public bool WasSuccessful =>
        EnvironmentWasPrepared &&
        AllExpectedStagesWereExecuted &&
        PipelineExecutionWasSuccessful &&
        PipelineBackupWasSuccessfulBeforeCleanup &&
        PipelineBackupMatchedWorkingCopyBeforeWrite &&
        WriteWasSuccessful &&
        VerificationWasSuccessful &&
        GenreVerificationWasSuccessful &&
        GenreWasPersisted &&
        PicturesWerePreserved &&
        OriginalFileRemainedUnchanged &&
        WorkingBackupMatchedInitialWorkingCopy &&
        WorkingCopyWasModified &&
        BackupsAreIndependent &&
        CleanupWasSuccessful &&
        string.IsNullOrWhiteSpace(
            ErrorMessage);

    /// <summary>
    /// Resumen legible del resultado integral.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    "La prueba integral del pipeline terminó " +
                    "correctamente. El género fue persistido, " +
                    "ambos respaldos fueron independientes, el " +
                    "archivo original permaneció intacto y el " +
                    "entorno temporal fue eliminado.";
            }

            return
                "La prueba integral del pipeline no superó todas " +
                "las comprobaciones. Etapas ejecutadas: " +
                $"{ExecutedStageCount}/{ExpectedStageCount}. " +
                $"Respaldo verificado: " +
                $"{ToSpanish(
                    PipelineBackupWasSuccessfulBeforeCleanup)}. " +
                $"Escritura correcta: " +
                $"{ToSpanish(WriteWasSuccessful)}. " +
                $"Verificación correcta: " +
                $"{ToSpanish(VerificationWasSuccessful)}. " +
                $"Original intacto: " +
                $"{ToSpanish(
                    OriginalFileRemainedUnchanged)}. " +
                $"Limpieza correcta: " +
                $"{ToSpanish(CleanupWasSuccessful)}.";
        }
    }

    private static bool HashesMatch(
        string firstHash,
        string secondHash)
    {
        return
            !string.IsNullOrWhiteSpace(firstHash) &&
            !string.IsNullOrWhiteSpace(secondHash) &&
            string.Equals(
                firstHash,
                secondHash,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static string NormalizePath(
        string path)
    {
        return Path.GetFullPath(
            path.Trim());
    }

    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}