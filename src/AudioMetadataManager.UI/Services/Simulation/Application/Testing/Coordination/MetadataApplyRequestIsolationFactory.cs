using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Construye una solicitud equivalente destinada exclusivamente
/// a una copia temporal aislada.
/// </summary>
public sealed class MetadataApplyRequestIsolationFactory
{
    /// <summary>
    /// Conserva los identificadores, cambios y requisitos de la
    /// solicitud original, sustituyendo únicamente el archivo
    /// de destino por la copia de trabajo.
    /// </summary>
    public MetadataApplyRequest Create(
        MetadataApplyRequest originalRequest,
        string workingCopyPath)
    {
        ArgumentNullException.ThrowIfNull(
            originalRequest);

        if (string.IsNullOrWhiteSpace(
                workingCopyPath))
        {
            throw new ArgumentException(
                "No se recibió una ruta válida para la copia " +
                "de trabajo.",
                nameof(workingCopyPath));
        }

        string normalizedWorkingCopyPath =
            Path.GetFullPath(
                workingCopyPath.Trim());

        return new MetadataApplyRequest
        {
            RequestId =
                originalRequest.RequestId,

            PlanId =
                originalRequest.PlanId,

            CreatedAtUtc =
                originalRequest.CreatedAtUtc,

            FilePath =
                normalizedWorkingCopyPath,

            FileName =
                Path.GetFileName(
                    normalizedWorkingCopyPath),

            Changes =
                originalRequest.Changes,

            RequireBackup =
                originalRequest.RequireBackup,

            RequirePostWriteVerification =
                originalRequest
                    .RequirePostWriteVerification
        };
    }
}