using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta una solicitud de aplicación exclusivamente sobre una
/// copia temporal aislada y verifica que el archivo original
/// permanezca intacto.
/// </summary>
public sealed class MetadataApplicationIsolatedExecutor
{
    private const string IsolationFolderName =
        "MetadataApplicationIsolatedExecution";

    private readonly FileIsolationTestHarness
        _isolationHarness;

    private readonly MetadataApplyRequestIsolationFactory
        _requestIsolationFactory;

    private readonly IMetadataApplicationCoordinator
        _applicationCoordinator;

    /// <summary>
    /// Crea el ejecutor con las dependencias predeterminadas.
    /// </summary>
    public MetadataApplicationIsolatedExecutor()
        : this(
            new FileIsolationTestHarness(),
            new MetadataApplyRequestIsolationFactory(),
            new MetadataApplicationCoordinator())
    {
    }

    /// <summary>
    /// Crea el ejecutor con dependencias proporcionadas.
    /// </summary>
    public MetadataApplicationIsolatedExecutor(
        FileIsolationTestHarness isolationHarness,
        MetadataApplyRequestIsolationFactory
            requestIsolationFactory,
        IMetadataApplicationCoordinator
            applicationCoordinator)
    {
        _isolationHarness =
            isolationHarness ??
            throw new ArgumentNullException(
                nameof(isolationHarness));

        _requestIsolationFactory =
            requestIsolationFactory ??
            throw new ArgumentNullException(
                nameof(requestIsolationFactory));

        _applicationCoordinator =
            applicationCoordinator ??
            throw new ArgumentNullException(
                nameof(applicationCoordinator));
    }

    /// <summary>
    /// Ejecuta el pipeline sobre una copia temporal y consolida
    /// las comprobaciones funcionales y de seguridad.
    /// </summary>
    /// <summary>
    /// Ejecuta utilizando el comportamiento seguro predeterminado,
    /// que elimina siempre el entorno temporal al finalizar.
    /// </summary>
    public Task<MetadataApplicationIsolatedExecutionResult>
        ExecuteAsync(
            MetadataApplyRequest request,
            CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            request,
            MetadataApplicationIsolatedExecutionOptions
                .SafeCleanupDefault,
            cancellationToken);
    }

    /// <summary>
    /// Ejecuta el pipeline sobre una copia temporal utilizando la
    /// política indicada para conservar o eliminar el entorno.
    /// </summary>
    public async Task<MetadataApplicationIsolatedExecutionResult>
        ExecuteAsync(
            MetadataApplyRequest request,
            MetadataApplicationIsolatedExecutionOptions options,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        ArgumentNullException.ThrowIfNull(
            options);

        FileIsolationContext? isolationContext =
            null;

        MetadataApplicationPipelineResult?
            pipelineResult =
                null;

        FileIsolationVerificationResult?
            isolationVerification =
                null;

        bool cleanupWasSuccessful =
            false;

        bool environmentWasPreserved =
            false;

        string errorMessage =
            string.Empty;

        try
        {
            isolationContext =
                await _isolationHarness.CreateAsync(
                    request.FilePath,
                    IsolationFolderName,
                    cancellationToken);

            MetadataApplyRequest isolatedRequest =
                _requestIsolationFactory.Create(
                    request,
                    isolationContext.WorkingCopyPath);

            pipelineResult =
                await _applicationCoordinator.ExecuteAsync(
                    isolatedRequest,
                    cancellationToken);

            isolationVerification =
                await _isolationHarness.VerifyAsync(
                    isolationContext,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            errorMessage =
                "La ejecución aislada fue cancelada.";
        }
        catch (Exception exception)
        {
            errorMessage =
                exception.Message;
        }
        finally
        {
            if (isolationContext is not null)
            {
                bool executionWasSuccessful =
                    string.IsNullOrWhiteSpace(
                        errorMessage) &&
                    isolationContext.IsCreated &&
                    pipelineResult?.WasSuccessful == true &&
                    isolationVerification?
                        .OriginalFileRemainedUnchanged == true &&
                    isolationVerification?
                        .WorkingCopyWasModified == true &&
                    isolationVerification?
                        .BackupMatchesInitialWorkingCopy == true;

                if (!executionWasSuccessful &&
                    string.IsNullOrWhiteSpace(
                        errorMessage))
                {
                    errorMessage =
                        BuildFailureDiagnostic(
                            isolationContext,
                            pipelineResult,
                            isolationVerification);
                }

                bool preserveSuccessfulEnvironment =
                    executionWasSuccessful &&
                    options.PreserveVerifiedWorkingCopy;

                if (preserveSuccessfulEnvironment)
                {
                    environmentWasPreserved =
                        true;
                }
                else
                {
                    bool shouldCleanup =
                        executionWasSuccessful
                            ? options.CleanupAfterExecution
                            : options.CleanupAfterFailure;

                    if (shouldCleanup)
                    {
                        cleanupWasSuccessful =
                            _isolationHarness.TryCleanup(
                                isolationContext);
                    }
                }
            }
        }

        return new MetadataApplicationIsolatedExecutionResult
        {
            IsolationContext =
                isolationContext,

            PipelineResult =
                pipelineResult,

            IsolationVerification =
                isolationVerification,

            CleanupWasSuccessful =
                cleanupWasSuccessful,

            EnvironmentWasPreserved =
                environmentWasPreserved,

            ErrorMessage =
                errorMessage
        };
    }

    private static string BuildFailureDiagnostic(
        FileIsolationContext isolationContext,
        MetadataApplicationPipelineResult? pipelineResult,
        FileIsolationVerificationResult? isolationVerification)
    {
        List<string> reasons =
            new();

        if (!isolationContext.IsCreated)
        {
            reasons.Add(
                "el entorno aislado no quedó creado correctamente");
        }

        if (pipelineResult is null)
        {
            reasons.Add(
                "el pipeline no produjo un resultado");
        }
        else if (!pipelineResult.WasSuccessful)
        {
            string pipelineFailure =
                $"el pipeline no terminó correctamente " +
                $"(motivo: {pipelineResult.StopReason})";

            if (!string.IsNullOrWhiteSpace(
                    pipelineResult.ErrorMessage))
            {
                pipelineFailure +=
                    $": {pipelineResult.ErrorMessage}";
            }

            reasons.Add(
                pipelineFailure);

            MetadataApplicationStageResult? failedStage =
                pipelineResult.StageResults
                    .LastOrDefault(
                        result =>
                            result.IsBlockingFailure);

            if (failedStage is not null)
            {
                string stageFailure =
                    $"la etapa {failedStage.StageDisplay} falló: " +
                    failedStage.Message;

                if (failedStage.Details.Count > 0)
                {
                    stageFailure +=
                        " Detalle: " +
                        string.Join(
                            " | ",
                            failedStage.Details);
                }

                reasons.Add(
                    stageFailure);
            }
        }

        if (isolationVerification is null)
        {
            reasons.Add(
                "no se obtuvo una verificación del entorno aislado");
        }
        else
        {
            if (!isolationVerification.OriginalFileRemainedUnchanged)
            {
                reasons.Add(
                    "el archivo original cambió durante la ejecución aislada");
            }

            if (!isolationVerification.WorkingCopyWasModified)
            {
                reasons.Add(
                    "la copia de trabajo no registró modificaciones");
            }

            if (!isolationVerification.BackupMatchesInitialWorkingCopy)
            {
                reasons.Add(
                    "el respaldo inicial no coincide con la copia de trabajo original");
            }
        }

        if (reasons.Count == 0)
        {
            return
                "La ejecución aislada no cumplió todos los criterios " +
                "de éxito requeridos.";
        }

        return
            "La ejecución aislada no pudo conservar una copia " +
            "verificada promovible porque " +
            string.Join(
                "; ",
                reasons) +
            ".";
    }
}