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
    public async Task<MetadataApplicationIsolatedExecutionResult>
        ExecuteAsync(
            MetadataApplyRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

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
                cleanupWasSuccessful =
                    _isolationHarness.TryCleanup(
                        isolationContext);
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

            ErrorMessage =
                errorMessage
        };
    }
}