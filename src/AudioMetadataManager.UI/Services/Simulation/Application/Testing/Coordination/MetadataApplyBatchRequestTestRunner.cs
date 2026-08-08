using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta comprobaciones estructurales controladas sobre una
/// solicitud productiva por lote.
///
/// No modifica archivos ni ejecuta el pipeline productivo.
/// </summary>
public sealed class MetadataApplyBatchRequestTestRunner
{
    /// <summary>
    /// Ejecuta las comprobaciones del modelo por lote.
    /// </summary>
    public MetadataApplyBatchRequestTestResult Run()
    {
        List<string> messages = new();

        MetadataApplyBatchRequest emptyBatch =
            new();

        bool emptyBatchWasRejected =
            !emptyBatch.IsStructurallyValid &&
            emptyBatch.RequestCount == 0 &&
            emptyBatch.ValidRequestCount == 0 &&
            emptyBatch.ValidChangeCount == 0;

        messages.Add(
            emptyBatchWasRejected
                ? "El lote vacío fue rechazado correctamente."
                : "El lote vacío no fue rechazado correctamente.");

        MetadataApplyRequest firstValidRequest =
            CreateValidRequest(
                @"C:\BatchTests\FirstTrack.flac",
                MetadataField.Genre,
                "Dance",
                "Electronic");

        MetadataApplyRequest secondValidRequest =
            CreateValidRequest(
                @"C:\BatchTests\SecondTrack.mp3",
                MetadataField.Label,
                string.Empty,
                "Test Records");

        MetadataApplyRequest invalidRequest =
            new()
            {
                PlanId = Guid.Empty,
                FilePath =
                    @"C:\BatchTests\InvalidTrack.flac",
                FileName =
                    "InvalidTrack.flac",
                Changes =
                    Array.Empty<MetadataFieldChange>()
            };

        MetadataApplyBatchRequest validBatch =
            new()
            {
                Requests =
                    new[]
                    {
                        firstValidRequest,
                        invalidRequest,
                        secondValidRequest
                    }
            };

        bool validBatchWasAccepted =
            validBatch.IsStructurallyValid;

        messages.Add(
            validBatchWasAccepted
                ? "El lote con solicitudes válidas fue aceptado."
                : "El lote con solicitudes válidas fue rechazado.");

        bool validRequestsWereCounted =
            validBatch.RequestCount == 3 &&
            validBatch.ValidRequestCount == 2;

        messages.Add(
            validRequestsWereCounted
                ? "Las solicitudes válidas fueron contadas correctamente."
                : "El conteo de solicitudes válidas fue incorrecto.");

        bool validChangesWereCounted =
            validBatch.ValidChangeCount == 2;

        messages.Add(
            validChangesWereCounted
                ? "Los cambios válidos fueron contados correctamente."
                : "El conteo de cambios válidos fue incorrecto.");

        bool invalidRequestsWereIgnored =
            !validBatch.ValidRequests.Contains(
                invalidRequest) &&
            validBatch.ValidRequests.Contains(
                firstValidRequest) &&
            validBatch.ValidRequests.Contains(
                secondValidRequest);

        messages.Add(
            invalidRequestsWereIgnored
                ? "Las solicitudes inválidas fueron ignoradas."
                : "Las solicitudes inválidas no fueron ignoradas.");

        MetadataApplyRequest duplicateRequest =
            CreateValidRequest(
                firstValidRequest.FilePath,
                MetadataField.Album,
                "Previous Album",
                "Updated Album");

        MetadataApplyBatchRequest duplicateBatch =
            new()
            {
                Requests =
                    new[]
                    {
                        firstValidRequest,
                        duplicateRequest
                    }
            };

        bool duplicatePathsWereDetected =
            duplicateBatch.HasDuplicateFilePaths &&
            duplicateBatch.DuplicateFilePaths.Count == 1;

        messages.Add(
            duplicatePathsWereDetected
                ? "Las rutas duplicadas fueron detectadas."
                : "Las rutas duplicadas no fueron detectadas.");

        bool duplicateBatchWasRejected =
            !duplicateBatch.IsStructurallyValid;

        messages.Add(
            duplicateBatchWasRejected
                ? "El lote con rutas duplicadas fue rechazado."
                : "El lote con rutas duplicadas fue aceptado.");

        bool batchIdentityWasCreated =
            validBatch.BatchId != Guid.Empty;

        messages.Add(
            batchIdentityWasCreated
                ? "La identidad del lote fue creada."
                : "La identidad del lote no fue creada.");

        bool creationTimeWasRecorded =
            validBatch.CreatedAtUtc != default &&
            validBatch.CreatedAtUtc <=
                DateTimeOffset.UtcNow;

        messages.Add(
            creationTimeWasRecorded
                ? "La fecha de creación del lote fue registrada."
                : "La fecha de creación del lote no fue registrada.");

        return new MetadataApplyBatchRequestTestResult
        {
            EmptyBatchWasRejected =
                emptyBatchWasRejected,

            ValidBatchWasAccepted =
                validBatchWasAccepted,

            ValidRequestsWereCounted =
                validRequestsWereCounted,

            ValidChangesWereCounted =
                validChangesWereCounted,

            InvalidRequestsWereIgnored =
                invalidRequestsWereIgnored,

            DuplicatePathsWereDetected =
                duplicatePathsWereDetected,

            DuplicateBatchWasRejected =
                duplicateBatchWasRejected,

            BatchIdentityWasCreated =
                batchIdentityWasCreated,

            CreationTimeWasRecorded =
                creationTimeWasRecorded,

            Messages =
                messages.ToArray()
        };
    }

    /// <summary>
    /// Crea una solicitud individual estructuralmente válida
    /// para las comprobaciones controladas.
    /// </summary>
    private static MetadataApplyRequest CreateValidRequest(
        string filePath,
        MetadataField field,
        string originalValue,
        string newValue)
    {
        return new MetadataApplyRequest
        {
            PlanId =
                Guid.NewGuid(),

            FilePath =
                filePath,

            FileName =
                Path.GetFileName(
                    filePath),

            Changes =
                new[]
                {
                    new MetadataFieldChange
                    {
                        Field =
                            field,

                        OriginalValue =
                            originalValue,

                        NewValue =
                            newValue,

                        WasManuallyApproved =
                            true,

                        Confidence =
                            1.0,

                        SupportingSources =
                            new[]
                            {
                                "Prueba estructural por lote"
                            }
                    }
                },

            RequireBackup =
                true,

            RequirePostWriteVerification =
                true
        };
    }
}