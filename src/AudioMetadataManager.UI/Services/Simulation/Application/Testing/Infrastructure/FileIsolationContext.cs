namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

/// <summary>
/// Contiene las rutas y huellas digitales iniciales de un
/// entorno aislado de pruebas.
///
/// El archivo original nunca debe entregarse al componente que
/// se está probando. Toda escritura debe realizarse sobre
/// WorkingCopyPath.
/// </summary>
public sealed class FileIsolationContext
{
    /// <summary>
    /// Ruta del archivo original protegido.
    /// </summary>
    public string OriginalFilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre del archivo original.
    /// </summary>
    public string OriginalFileName { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta de la copia sobre la que puede ejecutarse la prueba.
    /// </summary>
    public string WorkingCopyPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta del respaldo independiente de la copia de trabajo.
    /// </summary>
    public string WorkingBackupPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Carpeta temporal que contiene todos los archivos de la
    /// prueba.
    /// </summary>
    public string TestDirectoryPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash SHA-256 del archivo original antes de comenzar.
    /// </summary>
    public string OriginalHashBefore { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash SHA-256 inicial de la copia de trabajo.
    /// </summary>
    public string WorkingCopyHashBefore { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash SHA-256 del respaldo inicial.
    /// </summary>
    public string WorkingBackupHash { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si las rutas mínimas del entorno fueron creadas.
    /// </summary>
    public bool IsCreated =>
        !string.IsNullOrWhiteSpace(OriginalFilePath) &&
        !string.IsNullOrWhiteSpace(WorkingCopyPath) &&
        !string.IsNullOrWhiteSpace(WorkingBackupPath) &&
        !string.IsNullOrWhiteSpace(TestDirectoryPath);

    /// <summary>
    /// Indica si el respaldo coincide con el estado inicial de
    /// la copia de trabajo.
    /// </summary>
    public bool BackupMatchesInitialWorkingCopy =>
        !string.IsNullOrWhiteSpace(WorkingCopyHashBefore) &&
        string.Equals(
            WorkingCopyHashBefore,
            WorkingBackupHash,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Resumen compacto del entorno aislado.
    /// </summary>
    public string Summary
    {
        get
        {
            if (!IsCreated)
            {
                return
                    "El entorno aislado no fue creado " +
                    "correctamente.";
            }

            return BackupMatchesInitialWorkingCopy
                ? "El entorno aislado y su respaldo fueron " +
                  "creados correctamente."
                : "El entorno fue creado, pero el respaldo no " +
                  "coincide con la copia inicial.";
        }
    }
}