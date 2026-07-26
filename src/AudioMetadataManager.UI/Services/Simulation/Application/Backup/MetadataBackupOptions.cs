using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup;

/// <summary>
/// Configura la política utilizada para crear respaldos antes
/// de modificar archivos musicales.
/// </summary>
public sealed class MetadataBackupOptions
{
    /// <summary>
    /// Nombre de la carpeta principal que contendrá todos los
    /// respaldos administrados por la aplicación.
    ///
    /// Cuando no se proporciona una ruta raíz personalizada,
    /// esta carpeta se crea junto al archivo original.
    /// </summary>
    public string BackupFolderName { get; init; } =
        "AudioMetadataManager_Backup";

    /// <summary>
    /// Ruta raíz personalizada para almacenar respaldos.
    ///
    /// Si queda vacía, el motor utilizará una carpeta cercana
    /// al archivo original.
    /// </summary>
    public string RootBackupDirectory { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si cada operación debe organizarse dentro de una
    /// carpeta identificada por fecha.
    /// </summary>
    public bool OrganizeByDate { get; init; } =
        true;

    /// <summary>
    /// Formato utilizado para la carpeta de fecha.
    ///
    /// Se recomienda conservar un formato ordenable.
    /// </summary>
    public string DateFolderFormat { get; init; } =
        "yyyy-MM-dd";

    /// <summary>
    /// Indica si debe crearse una carpeta adicional para el
    /// identificador del plan de simulación.
    /// </summary>
    public bool OrganizeByPlanId { get; init; } =
        true;

    /// <summary>
    /// Indica si se conserva la estructura relativa de
    /// carpetas cuando posteriormente se procesen bibliotecas
    /// completas.
    ///
    /// Esta opción queda preparada para el procesamiento por
    /// lotes.
    /// </summary>
    public bool PreserveRelativeDirectoryStructure
    { get; init; } =
            true;

    /// <summary>
    /// Indica si una copia existente puede reemplazarse.
    ///
    /// Por seguridad, el valor predeterminado es falso.
    /// </summary>
    public bool AllowOverwrite { get; init; } =
        false;

    /// <summary>
    /// Indica si el motor debe generar un nombre alternativo
    /// cuando ya existe un respaldo con el mismo nombre.
    /// </summary>
    public bool GenerateUniqueNameOnCollision { get; init; } =
        true;

    /// <summary>
    /// Indica si el tamaño de la copia debe coincidir con el
    /// archivo original.
    /// </summary>
    public bool VerifyFileSize { get; init; } =
        true;

    /// <summary>
    /// Indica si debe calcularse una huella criptográfica del
    /// original y de la copia.
    /// </summary>
    public bool VerifyHash { get; init; } =
        true;

    /// <summary>
    /// Algoritmo de hash solicitado.
    ///
    /// Inicialmente utilizaremos SHA-256.
    /// </summary>
    public string HashAlgorithmName { get; init; } =
        "SHA256";

    /// <summary>
    /// Tamaño del búfer utilizado durante la copia.
    /// </summary>
    public int CopyBufferSize { get; init; } =
        1024 * 1024;

    /// <summary>
    /// Indica si la escritura del respaldo debe solicitar
    /// vaciado inmediato al almacenamiento físico.
    /// </summary>
    public bool FlushToDisk { get; init; } =
        true;

    /// <summary>
    /// Indica si la configuración contiene valores utilizables.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(
            BackupFolderName) &&
        !string.IsNullOrWhiteSpace(
            DateFolderFormat) &&
        !string.IsNullOrWhiteSpace(
            HashAlgorithmName) &&
        CopyBufferSize >= 4096;

    /// <summary>
    /// Nombre de carpeta normalizado.
    /// </summary>
    public string NormalizedBackupFolderName =>
        NormalizeFolderName(
            BackupFolderName);

    /// <summary>
    /// Ruta raíz personalizada normalizada.
    /// </summary>
    public string NormalizedRootBackupDirectory =>
        string.IsNullOrWhiteSpace(
            RootBackupDirectory)
                ? string.Empty
                : Path.GetFullPath(
                    RootBackupDirectory.Trim());

    private static string NormalizeFolderName(
        string value)
    {
        string normalized =
            string.IsNullOrWhiteSpace(value)
                ? "AudioMetadataManager_Backup"
                : value.Trim();

        foreach (char invalidCharacter
            in Path.GetInvalidFileNameChars())
        {
            normalized =
                normalized.Replace(
                    invalidCharacter,
                    '_');
        }

        return normalized;
    }
}