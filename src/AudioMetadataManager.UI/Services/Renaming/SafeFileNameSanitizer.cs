using System.IO;
using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.Renaming;

/// <summary>
/// Proporciona reglas de validación y saneamiento de nombres de archivo
/// conforme a los estándares de sistemas de archivos Windows (NTFS/FAT32).
/// </summary>
public class SafeFileNameSanitizer
{
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

    private static readonly HashSet<string> ReservedDeviceNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public const int MaxPathLength = 260;
    public const int MaxFileNameLength = 240;

    /// <summary>
    /// Sanea un nombre de archivo propuesto eliminando caracteres prohibidos,
    /// recortando espacios y puntos al final, y evitando colisiones con dispositivos reservados.
    /// </summary>
    public string Sanitize(string proposedName, string originalExtension = "")
    {
        if (string.IsNullOrWhiteSpace(proposedName))
        {
            return string.Empty;
        }

        string rawName = proposedName.Trim();
        string extension = string.Empty;

        int lastDotIndex = rawName.LastIndexOf('.');
        if (lastDotIndex > 0 && lastDotIndex < rawName.Length - 1)
        {
            string candidateExt = rawName.Substring(lastDotIndex);
            // Si parece una extensión de audio estándar sin espacios
            if (candidateExt.Length <= 6 && !candidateExt.Contains(' ') && !candidateExt.Contains('/') && !candidateExt.Contains('\\'))
            {
                extension = NormalizeExtension(candidateExt);
                rawName = rawName.Substring(0, lastDotIndex);
            }
        }

        if (string.IsNullOrWhiteSpace(extension) && !string.IsNullOrWhiteSpace(originalExtension))
        {
            extension = NormalizeExtension(originalExtension);
        }

        // 1. Reemplazar caracteres no permitidos en Windows (< > : " / \ | ? * y caracteres de control)
        string cleaned = rawName;
        foreach (char invalidChar in InvalidFileNameChars)
        {
            cleaned = cleaned.Replace(invalidChar, '_');
        }

        // 2. Limpieza de espacios múltiples y caracteres de control restantes
        cleaned = Regex.Replace(cleaned, @"[\x00-\x1F\x7F]", string.Empty);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        // 3. En Windows los nombres no pueden terminar en punto ni en espacio
        cleaned = cleaned.TrimEnd('.', ' ');

        // 4. Si tras la limpieza quedó vacío, devolvemos vacío
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return string.Empty;
        }

        // 5. Verificar si coincide con nombres reservados de dispositivos en Windows (ej. CON, PRN, AUX)
        if (ReservedDeviceNames.Contains(cleaned))
        {
            cleaned = $"_{cleaned}_";
        }

        // 6. Restricción de longitud máxima de nombre de archivo
        int maxBaseLength = MaxFileNameLength - extension.Length;
        if (maxBaseLength > 0 && cleaned.Length > maxBaseLength)
        {
            cleaned = cleaned.Substring(0, maxBaseLength).TrimEnd('.', ' ');
        }

        return $"{cleaned}{extension}";
    }

    /// <summary>
    /// Verifica si un nombre de archivo propuesto es sintácticamente válido para Windows.
    /// </summary>
    public bool IsValidFileName(string fileName, out string validationError)
    {
        validationError = string.Empty;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            validationError = "El nombre de archivo no puede estar vacío.";
            return false;
        }

        string trimmed = fileName.Trim();
        if (trimmed.IndexOfAny(InvalidFileNameChars) >= 0)
        {
            validationError = "El nombre contiene caracteres no permitidos en Windows (\\ / : * ? \" < > |).";
            return false;
        }

        string baseName = Path.GetFileNameWithoutExtension(trimmed);
        if (baseName.EndsWith('.') || baseName.EndsWith(' '))
        {
            validationError = "El nombre base no puede terminar en punto ni en espacio.";
            return false;
        }

        if (ReservedDeviceNames.Contains(baseName))
        {
            validationError = $"'{baseName}' es un nombre de dispositivo reservado en el sistema operativo.";
            return false;
        }

        if (trimmed.Length > MaxFileNameLength)
        {
            validationError = $"El nombre excede el límite seguro de {MaxFileNameLength} caracteres.";
            return false;
        }

        return true;
    }

    public static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        string normalized = extension.Trim().ToLowerInvariant();
        return normalized.StartsWith('.') ? normalized : $".{normalized}";
    }
}
