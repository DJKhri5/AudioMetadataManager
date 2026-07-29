using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Base;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Engine;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Verification;

/// <summary>
/// Verifica los metadatos persistidos después de una escritura
/// completada correctamente.
/// </summary>
public sealed class MetadataVerificationStage :
    MetadataApplicationStageBase
{
    private readonly IMetadataWriterVerificationEngine
        _verificationEngine;

    /// <summary>
    /// Crea la etapa utilizando el motor de verificación
    /// predeterminado.
    /// </summary>
    public MetadataVerificationStage()
        : this(
            new MetadataWriterVerificationEngine())
    {
    }

    /// <summary>
    /// Crea la etapa con un motor de verificación proporcionado.
    /// </summary>
    public MetadataVerificationStage(
        IMetadataWriterVerificationEngine verificationEngine)
    {
        _verificationEngine =
            verificationEngine ??
            throw new ArgumentNullException(
                nameof(verificationEngine));
    }

    /// <inheritdoc />
    public override MetadataApplicationStage Stage =>
        MetadataApplicationStage.PostWriteVerification;

    /// <inheritdoc />
    public override string Name =>
        "Verificación posterior a la escritura";

    /// <inheritdoc />
    public override int ExecutionOrder =>
        400;

    /// <inheritdoc />
    protected override
        Task<MetadataApplicationStageExecution>
        ExecuteCoreAsync(
            MetadataApplicationContext context)
    {
        MetadataWriteResult? writeResult =
            context.WriteResult;

        if (writeResult is null)
        {
            return Task.FromResult(
                Failed(
                    "La verificación no puede comenzar sin un " +
                    "resultado de escritura.",
                    new[]
                    {
                        "El contexto no contiene un resultado " +
                        "de escritura."
                    }));
        }

        if (writeResult.Status ==
            MetadataWriteStatus.NoWritableChanges)
        {
            return Task.FromResult(
                Skipped(
                    "La verificación fue omitida porque no se " +
                    "escribieron metadatos.",
                    writeResult.Messages));
        }

        if (writeResult.Status ==
            MetadataWriteStatus.Cancelled)
        {
            return Task.FromResult(
                Cancelled(
                    "La verificación no comenzó porque la " +
                    "escritura fue cancelada.",
                    writeResult.Messages));
        }

        if (!writeResult.WasSuccessful)
        {
            return Task.FromResult(
                Failed(
                    "La verificación no puede comenzar porque " +
                    "la escritura no terminó correctamente.",
                    new[]
                    {
                        writeResult.Summary
                    }));
        }

        context.ThrowIfCancellationRequested();

        MetadataVerificationResult verificationResult =
            _verificationEngine.Verify(
                writeResult.FilePath,
                context.Request.ValidChanges,
                writeResult.PictureCountBefore);

        context.SetVerificationResult(
            verificationResult);

        context.ThrowIfCancellationRequested();

        if (verificationResult.WasSuccessful)
        {
            return Task.FromResult(
                Completed(
                    verificationResult.Summary,
                    verificationResult.Messages));
        }

        return Task.FromResult(
            Failed(
                verificationResult.Summary,
                verificationResult.Messages));
    }
}