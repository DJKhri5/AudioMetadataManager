using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Renaming.Models;
using System.IO;
using System.Security.Cryptography;

namespace AudioMetadataManager.UI.Services.Renaming;

/// <summary>
/// Servicio principal para la ejecución física y reversible de operaciones de renombrado.
/// </summary>
public class FileRenameService
{
    private readonly FileRenameCollisionDetector _collisionDetector;
    private readonly List<RenameJournalEntry> _journal = new();

    public FileRenameService(FileRenameCollisionDetector? collisionDetector = null)
    {
        _collisionDetector = collisionDetector ?? new FileRenameCollisionDetector();
    }

    /// <summary>
    /// Historial de operaciones de renombrado registradas en la sesión.
    /// </summary>
    public IReadOnlyList<RenameJournalEntry> Journal => _journal.AsReadOnly();

    /// <summary>
    /// Renombra un archivo de audio de forma segura tras validar colisiones y registrar la bitácora.
    /// </summary>
    public FileRenameResult Rename(
        AudioFile audioFile,
        IEnumerable<AudioFile>? batchContext = null)
    {
        var validation = _collisionDetector.Validate(audioFile, batchContext);
        if (!validation.CanProceed)
        {
            return new FileRenameResult
            {
                WasSuccessful = false,
                OriginalFilePath = audioFile.FullPath ?? string.Empty,
                OriginalFileName = audioFile.FileName ?? string.Empty,
                NewFileName = validation.SanitizedFileName,
                NewFilePath = validation.TargetFilePath,
                ErrorMessage = validation.Message
            };
        }

        return ExecutePhysicalRename(audioFile, validation.TargetFilePath, validation.SanitizedFileName);
    }

    /// <summary>
    /// Ejecuta el renombrado físico en disco y actualiza el modelo en memoria.
    /// </summary>
    private FileRenameResult ExecutePhysicalRename(
        AudioFile audioFile,
        string targetFilePath,
        string sanitizedFileName)
    {
        string originalPath = audioFile.FullPath!;
        string originalFileName = audioFile.FileName!;

        string sha256 = string.Empty;
        try
        {
            sha256 = ComputeFileHash(originalPath);
        }
        catch (Exception ex)
        {
            return new FileRenameResult
            {
                WasSuccessful = false,
                OriginalFilePath = originalPath,
                OriginalFileName = originalFileName,
                NewFileName = sanitizedFileName,
                NewFilePath = targetFilePath,
                ErrorMessage = $"No se pudo calcular el hash de seguridad antes de renombrar: {ex.Message}"
            };
        }

        try
        {
            File.Move(originalPath, targetFilePath);

            // Crear entrada en la bitácora
            RenameJournalEntry journalEntry = new()
            {
                OriginalFilePath = originalPath,
                RenamedFilePath = targetFilePath,
                OriginalFileName = originalFileName,
                NewFileName = sanitizedFileName,
                FileSha256 = sha256
            };
            _journal.Add(journalEntry);

            // Actualizar el modelo AudioFile
            audioFile.FullPath = targetFilePath;
            audioFile.FileName = sanitizedFileName;

            return new FileRenameResult
            {
                WasSuccessful = true,
                OriginalFilePath = originalPath,
                OriginalFileName = originalFileName,
                NewFilePath = targetFilePath,
                NewFileName = sanitizedFileName,
                JournalEntry = journalEntry
            };
        }
        catch (Exception ex)
        {
            return new FileRenameResult
            {
                WasSuccessful = false,
                OriginalFilePath = originalPath,
                OriginalFileName = originalFileName,
                NewFileName = sanitizedFileName,
                NewFilePath = targetFilePath,
                ErrorMessage = $"Error al mover el archivo en disco: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Revierte una operación de renombrado previa utilizando la bitácora transaccional.
    /// </summary>
    public bool Rollback(RenameJournalEntry journalEntry, out string errorMessage, AudioFile? audioFile = null)
    {
        errorMessage = string.Empty;

        if (journalEntry.WasRolledBack)
        {
            errorMessage = "Esta operación ya fue revertida anteriormente.";
            return false;
        }

        if (!File.Exists(journalEntry.RenamedFilePath))
        {
            errorMessage = $"El archivo renombrado no fue encontrado en '{journalEntry.RenamedFilePath}'.";
            return false;
        }

        if (File.Exists(journalEntry.OriginalFilePath))
        {
            errorMessage = $"No se puede revertir: ya existe un archivo en la ruta original '{journalEntry.OriginalFilePath}'.";
            return false;
        }

        try
        {
            File.Move(journalEntry.RenamedFilePath, journalEntry.OriginalFilePath);
            journalEntry.WasRolledBack = true;
            journalEntry.RollbackTimestampUtc = DateTime.UtcNow;

            if (audioFile != null && string.Equals(audioFile.FullPath, journalEntry.RenamedFilePath, StringComparison.OrdinalIgnoreCase))
            {
                audioFile.FullPath = journalEntry.OriginalFilePath;
                audioFile.FileName = journalEntry.OriginalFileName;
            }

            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Fallo al revertir el archivo en disco: {ex.Message}";
            return false;
        }
    }

    private static string ComputeFileHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }
}
