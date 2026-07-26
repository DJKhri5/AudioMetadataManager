using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;

/// <summary>
/// Ejecuta comprobaciones de seguridad antes de permitir que
/// una solicitud avance hacia el respaldo o la escritura.
/// </summary>
public sealed class MetadataApplyRequestValidator
{
    private static readonly IReadOnlySet<string>
        SupportedExtensions =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ".mp3",
                ".wav",
                ".flac",
                ".aif",
                ".aiff"
            };

    private static readonly IReadOnlySet<MetadataField>
        SupportedFields =
            new HashSet<MetadataField>
            {
                MetadataField.Artist,
                MetadataField.Title,
                MetadataField.Version,
                MetadataField.Album,
                MetadataField.Genre,
                MetadataField.Label
            };

    /// <summary>
    /// Valida completamente una solicitud.
    /// </summary>
    public MetadataApplyValidationResult Validate(
        MetadataApplyRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        List<MetadataApplyValidationIssue> issues =
            new();

        ValidateStructure(
            request,
            issues);

        ValidateFile(
            request,
            issues);

        ValidateSafetyOptions(
            request,
            issues);

        ValidateChanges(
            request,
            issues);

        return new MetadataApplyValidationResult
        {
            Issues =
                issues.ToArray()
        };
    }

    private static void ValidateStructure(
        MetadataApplyRequest request,
        List<MetadataApplyValidationIssue> issues)
    {
        if (request.PlanId == Guid.Empty)
        {
            AddError(
                issues,
                "PLAN_ID_MISSING",
                "La solicitud no contiene un identificador " +
                "de plan válido.");
        }

        if (string.IsNullOrWhiteSpace(
                request.FilePath))
        {
            AddError(
                issues,
                "FILE_PATH_MISSING",
                "La solicitud no contiene una ruta de archivo.");
        }

        if (request.ValidChanges.Count == 0)
        {
            AddError(
                issues,
                "NO_VALID_CHANGES",
                "La solicitud no contiene cambios válidos " +
                "aprobados.");
        }
    }

    private static void ValidateFile(
        MetadataApplyRequest request,
        List<MetadataApplyValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(
                request.FilePath))
        {
            return;
        }

        if (!File.Exists(
                request.FilePath))
        {
            AddError(
                issues,
                "FILE_NOT_FOUND",
                "El archivo original no existe o ya no está " +
                "disponible.");

            return;
        }

        string extension =
            Path.GetExtension(
                request.FilePath);

        if (!SupportedExtensions.Contains(
                extension))
        {
            AddError(
                issues,
                "UNSUPPORTED_EXTENSION",
                $"La extensión '{extension}' no está " +
                "soportada por el motor de aplicación.");
        }

        try
        {
            FileAttributes attributes =
                File.GetAttributes(
                    request.FilePath);

            if (attributes.HasFlag(
                    FileAttributes.ReadOnly))
            {
                AddError(
                    issues,
                    "FILE_READ_ONLY",
                    "El archivo está marcado como solo lectura.");
            }
        }
        catch (Exception exception)
        {
            AddError(
                issues,
                "FILE_ATTRIBUTES_UNAVAILABLE",
                "No fue posible comprobar los atributos del " +
                $"archivo: {exception.Message}");
        }

        try
        {
            using FileStream stream =
                new(
                    request.FilePath,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.Read);

            AddInformation(
                issues,
                "FILE_WRITE_ACCESS_CONFIRMED",
                "El archivo puede abrirse con permisos de " +
                "lectura y escritura.");
        }
        catch (UnauthorizedAccessException)
        {
            AddError(
                issues,
                "FILE_ACCESS_DENIED",
                "Windows rechazó el acceso de escritura al " +
                "archivo.");
        }
        catch (IOException exception)
        {
            AddError(
                issues,
                "FILE_LOCKED_OR_UNAVAILABLE",
                "El archivo está bloqueado o no puede abrirse " +
                $"para escritura: {exception.Message}");
        }
        catch (Exception exception)
        {
            AddError(
                issues,
                "FILE_ACCESS_CHECK_FAILED",
                "No fue posible comprobar el acceso de " +
                $"escritura: {exception.Message}");
        }
    }

    private static void ValidateSafetyOptions(
        MetadataApplyRequest request,
        List<MetadataApplyValidationIssue> issues)
    {
        if (!request.RequireBackup)
        {
            AddError(
                issues,
                "BACKUP_REQUIRED",
                "La aplicación no permite escribir metadatos " +
                "sin crear previamente una copia de seguridad.");
        }

        if (!request.RequirePostWriteVerification)
        {
            AddError(
                issues,
                "VERIFICATION_REQUIRED",
                "La aplicación exige volver a leer y verificar " +
                "los metadatos después de la escritura.");
        }
    }

    private static void ValidateChanges(
        MetadataApplyRequest request,
        List<MetadataApplyValidationIssue> issues)
    {
        MetadataField[] duplicateFields =
            request.ValidChanges
                .GroupBy(
                    change =>
                        change.Field)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .ToArray();

        foreach (MetadataField duplicateField
            in duplicateFields)
        {
            AddError(
                issues,
                "DUPLICATE_FIELD_CHANGE",
                "La solicitud contiene más de un cambio para " +
                "el mismo campo.",
                duplicateField);
        }

        foreach (MetadataFieldChange change
            in request.Changes)
        {
            ValidateChange(
                change,
                issues);
        }
    }

    private static void ValidateChange(
        MetadataFieldChange change,
        List<MetadataApplyValidationIssue> issues)
    {
        if (change.Field ==
            MetadataField.Unknown)
        {
            AddError(
                issues,
                "UNKNOWN_FIELD",
                "La solicitud contiene un campo desconocido.");

            return;
        }

        if (!SupportedFields.Contains(
                change.Field))
        {
            AddError(
                issues,
                "UNSUPPORTED_FIELD",
                "El campo no está soportado por el motor de " +
                "aplicación.",
                change.Field);
        }

        if (string.IsNullOrWhiteSpace(
                change.NewValue))
        {
            AddError(
                issues,
                "EMPTY_NEW_VALUE",
                "El nuevo valor está vacío.",
                change.Field);
        }

        if (!change.IsValidChange)
        {
            AddError(
                issues,
                "INVALID_FIELD_CHANGE",
                "El cambio no representa una modificación " +
                "válida y diferente del valor original.",
                change.Field);
        }

        if (change.Confidence < 0 ||
            change.Confidence > 1)
        {
            AddError(
                issues,
                "INVALID_CONFIDENCE",
                "La confianza asociada al cambio está fuera " +
                "del intervalo permitido.",
                change.Field);
        }

        if (change.SupportingSources.Count == 0)
        {
            AddWarning(
                issues,
                "NO_SUPPORTING_SOURCES",
                "El cambio no conserva información sobre sus " +
                "fuentes de respaldo.",
                change.Field);
        }

        if (change.WasManuallyApproved)
        {
            AddInformation(
                issues,
                "MANUALLY_APPROVED",
                "El cambio fue aprobado manualmente por el " +
                "usuario.",
                change.Field);
        }
    }

    private static void AddError(
        List<MetadataApplyValidationIssue> issues,
        string code,
        string message,
        MetadataField? field = null)
    {
        issues.Add(
            new MetadataApplyValidationIssue
            {
                Severity =
                    MetadataApplyValidationIssueSeverity.Error,

                Code =
                    code,

                Message =
                    message,

                Field =
                    field
            });
    }

    private static void AddWarning(
        List<MetadataApplyValidationIssue> issues,
        string code,
        string message,
        MetadataField? field = null)
    {
        issues.Add(
            new MetadataApplyValidationIssue
            {
                Severity =
                    MetadataApplyValidationIssueSeverity.Warning,

                Code =
                    code,

                Message =
                    message,

                Field =
                    field
            });
    }

    private static void AddInformation(
        List<MetadataApplyValidationIssue> issues,
        string code,
        string message,
        MetadataField? field = null)
    {
        issues.Add(
            new MetadataApplyValidationIssue
            {
                Severity =
                    MetadataApplyValidationIssueSeverity
                        .Information,

                Code =
                    code,

                Message =
                    message,

                Field =
                    field
            });
    }
}