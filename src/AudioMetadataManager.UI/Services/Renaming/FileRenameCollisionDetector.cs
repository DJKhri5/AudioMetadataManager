using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Renaming.Models;
using System.IO;

namespace AudioMetadataManager.UI.Services.Renaming;

/// <summary>
/// Motor de detección de colisiones y validación estructural para el renombrado seguro.
/// </summary>
public class FileRenameCollisionDetector
{
    private readonly SafeFileNameSanitizer _sanitizer;

    public FileRenameCollisionDetector(SafeFileNameSanitizer? sanitizer = null)
    {
        _sanitizer = sanitizer ?? new SafeFileNameSanitizer();
    }

    /// <summary>
    /// Valida si un archivo de audio puede renombrarse a su nombre propuesto de forma segura,
    /// comprobando sintaxis, existencia en disco, colisiones intra-lote y bloqueos.
    /// </summary>
    public RenameValidationResult Validate(
        AudioFile audioFile,
        IEnumerable<AudioFile>? batchContext = null)
    {
        string currentPath = audioFile.FullPath ?? string.Empty;
        string rawProposed = GetEffectiveProposedName(audioFile);

        return Validate(currentPath, rawProposed, audioFile.Extension, batchContext);
    }

    /// <summary>
    /// Obtiene el nombre propuesto efectivo a partir de la simulación o de los metadatos de la pista.
    /// </summary>
    public static string GetEffectiveProposedName(AudioFile audioFile)
    {
        if (audioFile == null) return string.Empty;

        string proposed = audioFile.Simulation?.ProposedFileName ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(proposed)) return proposed;

        if (!string.IsNullOrWhiteSpace(audioFile.Artist) && !string.IsNullOrWhiteSpace(audioFile.Title))
        {
            return $"{audioFile.Artist} - {audioFile.Title}{audioFile.Extension}";
        }

        var sim = new Simulation.FileSimulationService().Build(audioFile);
        return sim.ProposedFileName;
    }

    /// <summary>
    /// Valida si una ruta de archivo y un nombre propuesto cumplen todas las salvaguardas.
    /// </summary>
    public RenameValidationResult Validate(
        string currentFilePath,
        string proposedFileName,
        string originalExtension = "",
        IEnumerable<AudioFile>? batchContext = null)
    {
        RenameValidationResult result = new()
        {
            CurrentFilePath = currentFilePath,
            CurrentFileName = Path.GetFileName(currentFilePath),
            ProposedFileName = proposedFileName
        };

        if (string.IsNullOrWhiteSpace(currentFilePath))
        {
            result.Status = RenameValidationStatus.SourceFileNotFound;
            result.Message = "No se especificó la ruta del archivo de origen.";
            return result;
        }

        if (string.IsNullOrWhiteSpace(proposedFileName))
        {
            result.Status = RenameValidationStatus.NoProposalAvailable;
            result.Message = "No existe una propuesta de nombre para este archivo.";
            return result;
        }

        // Obtener directorio contenedor
        string? directory = Path.GetDirectoryName(currentFilePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = Directory.GetCurrentDirectory();
        }

        // 1. Saneamiento del nombre
        string ext = string.IsNullOrWhiteSpace(originalExtension)
            ? Path.GetExtension(currentFilePath)
            : originalExtension;

        string sanitized = _sanitizer.Sanitize(proposedFileName, ext);
        result.SanitizedFileName = sanitized;

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            result.Status = RenameValidationStatus.InvalidSyntaxOrCharacters;
            result.Message = "El nombre propuesto no es válido tras eliminar caracteres prohibidos.";
            return result;
        }

        if (!_sanitizer.IsValidFileName(sanitized, out string syntaxError))
        {
            result.Status = RenameValidationStatus.InvalidSyntaxOrCharacters;
            result.Message = syntaxError;
            return result;
        }

        string targetPath = Path.Combine(directory, sanitized);
        result.TargetFilePath = targetPath;

        // 2. Comprobar longitud total de ruta
        if (targetPath.Length >= SafeFileNameSanitizer.MaxPathLength)
        {
            result.Status = RenameValidationStatus.PathTooLong;
            result.Message = $"La ruta completa resultante ({targetPath.Length} caracteres) supera el límite de {SafeFileNameSanitizer.MaxPathLength}.";
            return result;
        }

        // 3. Comprobar si es un No-Op (nombre idéntico al actual en disco)
        if (string.Equals(result.CurrentFileName, sanitized, StringComparison.OrdinalIgnoreCase))
        {
            result.Status = RenameValidationStatus.IdenticalNameNoOp;
            result.Message = "El nombre propuesto coincide exactamente con el nombre actual del archivo.";
            return result;
        }

        // 4. Comprobar colisión en disco (si ya existe un archivo con el nombre destino)
        if (File.Exists(targetPath))
        {
            // Si en disco existe un archivo con esa ruta y NO es el mismo archivo físico
            result.Status = RenameValidationStatus.DestinationCollisionDisk;
            result.Message = $"Conflicto en disco: ya existe un archivo llamado '{sanitized}' en esta carpeta.";
            result.DiagnosticDetail = $"Ruta en conflicto: {targetPath}";
            return result;
        }

        // 5. Comprobar colisión intra-lote (si otro archivo del lote generará el mismo nombre destino)
        if (batchContext != null)
        {
            foreach (var other in batchContext)
            {
                if (string.Equals(other.FullPath, currentFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Mismo archivo
                }

                string otherProposed = GetEffectiveProposedName(other);
                if (!string.IsNullOrWhiteSpace(otherProposed))
                {
                    string otherSanitized = _sanitizer.Sanitize(otherProposed, other.Extension);
                    if (string.Equals(otherSanitized, sanitized, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Status = RenameValidationStatus.DestinationCollisionBatch;
                        result.Message = $"Conflicto en lote: otro archivo ({other.FileName}) generaría el mismo nombre '{sanitized}'.";
                        result.DiagnosticDetail = $"Archivo en conflicto: {other.FullPath}";
                        return result;
                    }
                }
            }
        }

        // 6. Comprobar si el archivo de origen existe físicamente
        if (!File.Exists(currentFilePath))
        {
            result.Status = RenameValidationStatus.SourceFileNotFound;
            result.Message = "El archivo de origen no fue encontrado en el disco.";
            return result;
        }

        // 7. Comprobar si el archivo de origen está bloqueado por otro proceso
        if (IsFileLocked(currentFilePath))
        {
            result.Status = RenameValidationStatus.SourceFileLocked;
            result.Message = "El archivo de origen está siendo utilizado por otra aplicación.";
            return result;
        }

        result.Status = RenameValidationStatus.ReadyToRename;
        result.Message = "Nombre propuesto verificado y listo para renombrar.";
        return result;
    }

    private static bool IsFileLocked(string filePath)
    {
        try
        {
            using FileStream stream = File.Open(
                filePath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
        catch
        {
            return false;
        }
    }
}
