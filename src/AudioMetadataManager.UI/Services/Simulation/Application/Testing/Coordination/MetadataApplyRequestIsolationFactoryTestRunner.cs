using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta comprobaciones controladas sobre la fábrica de
/// solicitudes aisladas sin escribir archivos.
/// </summary>
public sealed class MetadataApplyRequestIsolationFactoryTestRunner
{
    public MetadataApplyRequestIsolationFactoryTestResult Run()
    {
        List<string> messages =
            new();

        MetadataApplyRequestIsolationFactory factory =
            new();

        bool nullRequestWasRejected =
            false;

        bool emptyPathWasRejected =
            false;

        try
        {
            _ = factory.Create(
                null!,
                "working-test.flac");
        }
        catch (ArgumentNullException exception)
            when (exception.ParamName == "originalRequest")
        {
            nullRequestWasRejected =
                true;
        }

        messages.Add(
            nullRequestWasRejected
                ? "La solicitud nula fue rechazada correctamente."
                : "La solicitud nula no fue rechazada.");

        MetadataApplyRequest originalRequest =
            new()
            {
                RequestId =
                    Guid.NewGuid(),

                PlanId =
                    Guid.NewGuid(),

                CreatedAtUtc =
                    DateTimeOffset.UtcNow.AddMinutes(-1),

                FilePath =
                    "original-test.flac",

                FileName =
                    "original-test.flac",

                Changes =
                    new[]
                    {
                        new MetadataFieldChange
                        {
                            Field =
                                MetadataField.Genre,

                            OriginalValue =
                                "House",

                            NewValue =
                                "Electronic",

                            WasManuallyApproved =
                                true,

                            Confidence =
                                0.85
                        }
                    },

                RequireBackup =
                    true,

                RequirePostWriteVerification =
                    true
            };

        try
        {
            _ = factory.Create(
                originalRequest,
                " ");
        }
        catch (ArgumentException exception)
            when (exception.ParamName == "workingCopyPath")
        {
            emptyPathWasRejected =
                true;
        }

        messages.Add(
            emptyPathWasRejected
                ? "La ruta vacía fue rechazada correctamente."
                : "La ruta vacía no fue rechazada.");

        string workingCopyPath =
            Path.Combine(
                Path.GetTempPath(),
                "working_metadata_isolation_test.flac");

        MetadataApplyRequest isolatedRequest =
            factory.Create(
                originalRequest,
                workingCopyPath);

        bool identifiersWerePreserved =
            isolatedRequest.RequestId ==
                originalRequest.RequestId &&
            isolatedRequest.PlanId ==
                originalRequest.PlanId;

        bool creationTimeWasPreserved =
            isolatedRequest.CreatedAtUtc ==
                originalRequest.CreatedAtUtc;

        bool changesWerePreserved =
            ReferenceEquals(
                isolatedRequest.Changes,
                originalRequest.Changes);

        bool requirementsWerePreserved =
            isolatedRequest.RequireBackup ==
                originalRequest.RequireBackup &&
            isolatedRequest
                .RequirePostWriteVerification ==
                originalRequest
                    .RequirePostWriteVerification;

        string expectedPath =
            Path.GetFullPath(
                workingCopyPath);

        bool workingCopyPathWasApplied =
            string.Equals(
                isolatedRequest.FilePath,
                expectedPath,
                StringComparison.OrdinalIgnoreCase);

        bool workingCopyFileNameWasApplied =
            string.Equals(
                isolatedRequest.FileName,
                Path.GetFileName(
                    expectedPath),
                StringComparison.Ordinal);

        messages.Add(
            identifiersWerePreserved
                ? "Los identificadores fueron conservados."
                : "Los identificadores no fueron conservados.");

        messages.Add(
            creationTimeWasPreserved
                ? "La fecha de creación fue conservada."
                : "La fecha de creación no fue conservada.");

        messages.Add(
            changesWerePreserved
                ? "La colección de cambios fue conservada."
                : "La colección de cambios no fue conservada.");

        messages.Add(
            requirementsWerePreserved
                ? "Los requisitos fueron conservados."
                : "Los requisitos no fueron conservados.");

        messages.Add(
            workingCopyPathWasApplied
                ? "La ruta de la copia fue aplicada correctamente."
                : "La ruta de la copia no fue aplicada.");

        messages.Add(
            workingCopyFileNameWasApplied
                ? "El nombre de la copia fue aplicado correctamente."
                : "El nombre de la copia no fue aplicado.");

        return new MetadataApplyRequestIsolationFactoryTestResult
        {
            NullRequestWasRejected =
                nullRequestWasRejected,

            EmptyPathWasRejected =
                emptyPathWasRejected,

            IdentifiersWerePreserved =
                identifiersWerePreserved,

            CreationTimeWasPreserved =
                creationTimeWasPreserved,

            ChangesWerePreserved =
                changesWerePreserved,

            RequirementsWerePreserved =
                requirementsWerePreserved,

            WorkingCopyPathWasApplied =
                workingCopyPathWasApplied,

            WorkingCopyFileNameWasApplied =
                workingCopyFileNameWasApplied,

            Messages =
                messages.ToArray()
        };
    }
}