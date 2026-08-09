using AudioMetadataManager.UI.Services.AudioAnalysis;
using AudioMetadataManager.UI.Services.AudioAnalysis.Diagnostics;
using System.IO;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Engine;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources.Pipeline
    .Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Mapping;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Integration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineComposition;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineExecution;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Verification;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Adapters;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Testing;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Diagnostics;
using AudioMetadataManager.UI.Views.Models.Simulation;
using ConsensusResult =
    AudioMetadataManager.UI.Services.MetadataSources
        .Consensus.Models.MetadataConsensusResult;

namespace AudioMetadataManager.UI.Services.Diagnostics;

/// <summary>
/// Orquesta la secuencia completa de diagnóstico técnico y pruebas
/// estructurales que antes vivía dentro del code-behind de
/// MainWindow.
///
/// Cada paso reporta su progreso a través del delegado log,
/// sin depender de ningún control de la interfaz.
/// </summary>
public sealed class AudioDiagnosticsOrchestrator
    : IAudioDiagnosticsOrchestrator
{
    private readonly AudioAnalysisTestRunner
        _audioAnalysisTestRunner;
    private readonly MetadataVerificationStageTestRunner
        _metadataVerificationStageTestRunner;
    private readonly MetadataApplicationPipelineFactoryTestRunner
        _metadataApplicationPipelineFactoryTestRunner;
    private readonly MetadataApplyResultBuilderTestRunner
        _metadataApplyResultBuilderTestRunner;
    private readonly MetadataApplicationCoordinatorTestRunner
        _metadataApplicationCoordinatorTestRunner;
    private readonly MetadataApplicationIsolatedExecutorTestRunner
        _metadataApplicationIsolatedExecutorTestRunner;
    private readonly MetadataApplicationPreservedExecutionTestRunner
        _metadataApplicationPreservedExecutionTestRunner;
    private readonly MetadataApplicationPromotionTestRunner
        _metadataApplicationPromotionTestRunner;
    private readonly MetadataApplicationRollbackTestRunner
        _metadataApplicationRollbackTestRunner;
    private readonly MetadataApplyBatchRequestTestRunner
        _metadataApplyBatchRequestTestRunner;
    private readonly MetadataApplyBatchResultTestRunner
        _metadataApplyBatchResultTestRunner;
    private readonly MetadataProductiveApplicationBatchCoordinatorTestRunner
        _metadataProductiveApplicationBatchCoordinatorTestRunner;
    private readonly ProductiveBatchSelectionTestRunner
        _productiveBatchSelectionTestRunner;
    private readonly MetadataApplyRequestIsolationFactoryTestRunner
        _metadataApplyRequestIsolationFactoryTestRunner;
    private readonly MetadataProductiveApplicationCoordinatorTestRunner
        _metadataProductiveApplicationCoordinatorTestRunner;
    private readonly MetadataProductiveApplicationApprovedTestRunner
        _metadataProductiveApplicationApprovedTestRunner;
    private readonly MetadataProductiveTwoPhaseBatchPreparationTestRunner
        _metadataProductiveTwoPhaseBatchPreparationTestRunner;
    private readonly MetadataProductiveTwoPhaseBatchCompletionTestRunner
        _metadataProductiveTwoPhaseBatchCompletionTestRunner;

    public AudioDiagnosticsOrchestrator(
        AudioAnalysisTestRunner audioAnalysisTestRunner,
        MetadataVerificationStageTestRunner metadataVerificationStageTestRunner,
        MetadataApplicationPipelineFactoryTestRunner metadataApplicationPipelineFactoryTestRunner,
        MetadataApplyResultBuilderTestRunner metadataApplyResultBuilderTestRunner,
        MetadataApplicationCoordinatorTestRunner metadataApplicationCoordinatorTestRunner,
        MetadataApplicationIsolatedExecutorTestRunner metadataApplicationIsolatedExecutorTestRunner,
        MetadataApplicationPreservedExecutionTestRunner metadataApplicationPreservedExecutionTestRunner,
        MetadataApplicationPromotionTestRunner metadataApplicationPromotionTestRunner,
        MetadataApplicationRollbackTestRunner metadataApplicationRollbackTestRunner,
        MetadataApplyBatchRequestTestRunner metadataApplyBatchRequestTestRunner,
        MetadataApplyBatchResultTestRunner metadataApplyBatchResultTestRunner,
        MetadataProductiveApplicationBatchCoordinatorTestRunner metadataProductiveApplicationBatchCoordinatorTestRunner,
        ProductiveBatchSelectionTestRunner productiveBatchSelectionTestRunner,
        MetadataApplyRequestIsolationFactoryTestRunner metadataApplyRequestIsolationFactoryTestRunner,
        MetadataProductiveApplicationCoordinatorTestRunner metadataProductiveApplicationCoordinatorTestRunner,
        MetadataProductiveApplicationApprovedTestRunner metadataProductiveApplicationApprovedTestRunner,
        MetadataProductiveTwoPhaseBatchPreparationTestRunner metadataProductiveTwoPhaseBatchPreparationTestRunner,
        MetadataProductiveTwoPhaseBatchCompletionTestRunner metadataProductiveTwoPhaseBatchCompletionTestRunner)
    {
        _audioAnalysisTestRunner = audioAnalysisTestRunner;
        _metadataVerificationStageTestRunner = metadataVerificationStageTestRunner;
        _metadataApplicationPipelineFactoryTestRunner = metadataApplicationPipelineFactoryTestRunner;
        _metadataApplyResultBuilderTestRunner = metadataApplyResultBuilderTestRunner;
        _metadataApplicationCoordinatorTestRunner = metadataApplicationCoordinatorTestRunner;
        _metadataApplicationIsolatedExecutorTestRunner = metadataApplicationIsolatedExecutorTestRunner;
        _metadataApplicationPreservedExecutionTestRunner = metadataApplicationPreservedExecutionTestRunner;
        _metadataApplicationPromotionTestRunner = metadataApplicationPromotionTestRunner;
        _metadataApplicationRollbackTestRunner = metadataApplicationRollbackTestRunner;
        _metadataApplyBatchRequestTestRunner = metadataApplyBatchRequestTestRunner;
        _metadataApplyBatchResultTestRunner = metadataApplyBatchResultTestRunner;
        _metadataProductiveApplicationBatchCoordinatorTestRunner = metadataProductiveApplicationBatchCoordinatorTestRunner;
        _productiveBatchSelectionTestRunner = productiveBatchSelectionTestRunner;
        _metadataApplyRequestIsolationFactoryTestRunner = metadataApplyRequestIsolationFactoryTestRunner;
        _metadataProductiveApplicationCoordinatorTestRunner = metadataProductiveApplicationCoordinatorTestRunner;
        _metadataProductiveApplicationApprovedTestRunner = metadataProductiveApplicationApprovedTestRunner;
        _metadataProductiveTwoPhaseBatchPreparationTestRunner = metadataProductiveTwoPhaseBatchPreparationTestRunner;
        _metadataProductiveTwoPhaseBatchCompletionTestRunner = metadataProductiveTwoPhaseBatchCompletionTestRunner;
    }

    /// <summary>
    /// Ejecuta la secuencia completa de diagnóstico técnico y
    /// pruebas estructurales sobre el archivo indicado.
    ///
    /// El resultado de cada paso se reporta a través de log,
    /// en el mismo orden y con el mismo contenido que antes se
    /// escribía directamente en LogTextBox desde MainWindow.
    /// </summary>
    public async Task RunFullDiagnosticAsync(
        string filePath,
        ProductiveBatchSelection currentSelection,
        Action<string> log,
        CancellationToken cancellationToken = default)
    {
        try
        {
            /*
             * El diagnóstico técnico general se ejecuta para todos
             * los formatos compatibles con AudioAnalysisEngine.
             */
            AudioAnalysisTestReport report =
                await Task.Run(
                    async () =>
                        await _audioAnalysisTestRunner.RunAsync(
                            filePath));

            log(report.ReportText);

            log(
                "Iniciando pruebas estructurales de la etapa " +
                "de verificación.");

            MetadataVerificationStageTestResult
                verificationStageTestResult =
                    await _metadataVerificationStageTestRunner
                        .RunAsync();

            log(
                "=== Etapa de verificación posterior a la " +
                "escritura ===");

            foreach (string message
                in verificationStageTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Resultado general: " +
                $"{ToSpanish(
                    verificationStageTestResult
                        .WasSuccessful)}");

            log(
                "=== Fin de las pruebas estructurales de " +
                "verificación ===");

            log(
                "Iniciando pruebas estructurales de la " +
                "composición del pipeline.");

            MetadataApplicationPipelineFactoryTestResult
                pipelineFactoryTestResult =
                    await _metadataApplicationPipelineFactoryTestRunner
                        .RunAsync();

            log(
                "=== Composición predeterminada del pipeline ===");

            foreach (string message
                in pipelineFactoryTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Resultado general: " +
                $"{ToSpanish(
                    pipelineFactoryTestResult
                        .WasSuccessful)}");

            log(
                pipelineFactoryTestResult.Summary);

            log(
                "=== Fin de las pruebas estructurales de " +
                "composición ===");

            log(
                "Iniciando prueba del constructor del " +
                "resultado final.");

            MetadataApplyResultBuilderTestResult
                resultBuilderTestResult =
                    await Task.Run(
                        () =>
                            _metadataApplyResultBuilderTestRunner
                                .Run());

            log(
                "=== Constructor del resultado final ===");

            log(
                $"Identificadores conservados: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .IdentifiersPreserved)}");

            log(
                $"Información del archivo conservada: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .FileInformationPreserved)}");

            log(
                $"Ruta de respaldo conservada: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .BackupPathPreserved)}");

            log(
                $"Cantidad de campos conservada: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .FieldCountPreserved)}");

            log(
                $"Valores de los campos conservados: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .FieldValuesPreserved)}");

            log(
                $"Estado de escritura conservado: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .WriteStatusPreserved)}");

            log(
                $"Estado de verificación conservado: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .VerificationStatusPreserved)}");

            log(
                $"Estado final correcto: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .FinalStatusCorrect)}");

            log(
                $"Mensajes consolidados: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .MessagesConsolidated)}");

            log(
                $"Mensajes duplicados eliminados: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .DuplicateMessagesRemoved)}");

            log(
                $"Tiempos coherentes: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .TimingIsValid)}");

            if (resultBuilderTestResult.ApplyResult is not null)
            {
                log(
                    $"Estado construido: " +
                    $"{resultBuilderTestResult
                        .ApplyResult.Status}");

                log(
                    $"Campos consolidados: " +
                    $"{resultBuilderTestResult
                        .ApplyResult.FieldResults.Count}");

                log(
                    $"Duración construida: " +
                    $"{resultBuilderTestResult
                        .ApplyResult.ElapsedTime}");
            }

            foreach (string message
                in resultBuilderTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            if (!string.IsNullOrWhiteSpace(
                    resultBuilderTestResult.ErrorMessage))
            {
                log(
                    $"Error: " +
                    $"{resultBuilderTestResult.ErrorMessage}");

                log(
                    $"Tipo de excepción: " +
                    $"{resultBuilderTestResult.ExceptionType}");
            }

            log(
                $"Prueba del constructor correcta: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: " +
                $"{resultBuilderTestResult.Summary}");

            log(
                "=== Fin de la prueba del constructor ===");

            log(
                "Iniciando pruebas controladas del coordinador " +
                "productivo.");

            MetadataApplicationCoordinatorTestResult
                coordinatorTestResult =
                    await _metadataApplicationCoordinatorTestRunner
                        .RunAsync();

            log(
                "=== Coordinador productivo de aplicación ===");

            foreach (string message
                in coordinatorTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Solicitud nula rechazada: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .NullRequestWasRejected)}");

            log(
                $"Fábrica nula rechazada: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .NullExecutorFactoryWasRejected)}");

            log(
                $"Cancelación previa controlada: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .PreCancelledExecutionWasHandled)}");

            log(
                $"Razón de cancelación correcta: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .CancellationStopReasonWasCorrect)}");

            log(
                $"Ejecutor nulo controlado: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .NullExecutorWasHandled)}");

            log(
                $"Razón del ejecutor nulo correcta: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .NullExecutorStopReasonWasCorrect)}");

            log(
                $"Excepción de fábrica controlada: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .FactoryExceptionWasHandled)}");

            log(
                $"Razón de excepción correcta: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .FactoryExceptionStopReasonWasCorrect)}");

            log(
                $"Resultados finalizados: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .ResultsWereFinalized)}");

            log(
                $"Prueba del coordinador correcta: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: {coordinatorTestResult.Summary}");

            log(
                "=== Fin de las pruebas del coordinador ===");

            log(
                "Iniciando pruebas estructurales de la solicitud productiva por lote.");

            MetadataApplyBatchRequestTestResult
                batchRequestTestResult =
                    _metadataApplyBatchRequestTestRunner.Run();

            log(
                "=== Solicitud productiva por lote ===");

            foreach (string message
                in batchRequestTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Lote vacío rechazado: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .EmptyBatchWasRejected)}");

            log(
                $"Lote válido aceptado: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .ValidBatchWasAccepted)}");

            log(
                $"Solicitudes válidas contabilizadas: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .ValidRequestsWereCounted)}");

            log(
                $"Cambios válidos contabilizados: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .ValidChangesWereCounted)}");

            log(
                $"Solicitudes inválidas ignoradas: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .InvalidRequestsWereIgnored)}");

            log(
                $"Rutas duplicadas detectadas: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .DuplicatePathsWereDetected)}");

            log(
                $"Lote duplicado rechazado: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .DuplicateBatchWasRejected)}");

            log(
                $"Identidad del lote creada: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .BatchIdentityWasCreated)}");

            log(
                $"Fecha de creación registrada: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .CreationTimeWasRecorded)}");

            log(
                $"Prueba de solicitud por lote correcta: " +
                $"{ToSpanish(
                    batchRequestTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: {batchRequestTestResult.Summary}");

            log(
                "=== Fin de las pruebas de solicitud productiva por lote ===");

            log(
                "Iniciando pruebas estructurales del resultado " +
                "productivo por lote.");

            MetadataApplyBatchResultTestResult
                batchResultTestResult =
                    _metadataApplyBatchResultTestRunner.Run();

            log(
                "=== Resultado productivo por lote ===");

            foreach (string message
                in batchResultTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Resultado vacío rechazado: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .EmptyResultWasRejected)}");

            log(
                $"Resultados correctos contabilizados: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .SuccessfulResultsWereCounted)}");

            log(
                $"Resultados fallidos contabilizados: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .FailedResultsWereCounted)}");

            log(
                $"Lote exitoso detectado: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .SuccessfulBatchWasDetected)}");

            log(
                $"Fallo parcial detectado: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .PartialFailureWasDetected)}");

            log(
                $"Identidad del lote preservada: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .BatchIdentityWasPreserved)}");

            log(
                $"Tiempos preservados: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .TimesWerePreserved)}");

            log(
                $"Duración calculada correctamente: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .DurationWasCalculated)}");

            log(
                $"Mensajes preservados: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .MessagesWerePreserved)}");

            log(
                $"Resumen generado: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .SummaryWasGenerated)}");

            log(
                $"Prueba de resultado por lote correcta: " +
                $"{ToSpanish(
                    batchResultTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: {batchResultTestResult.Summary}");

            log(
                "=== Fin de las pruebas del resultado productivo por lote ===");

            log(
                "Iniciando pruebas estructurales del coordinador " +
                "productivo por lote.");

            MetadataProductiveApplicationBatchCoordinatorTestResult
                batchCoordinatorTestResult =
                    await _metadataProductiveApplicationBatchCoordinatorTestRunner
                        .RunAsync();

            log(
                "=== Coordinador productivo por lote ===");

            foreach (string message
                in batchCoordinatorTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Dependencia individual nula rechazada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .NullCoordinatorWasRejected)}");

            log(
                $"Solicitud por lote nula rechazada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .NullBatchWasRejected)}");

            log(
                $"Cancelación previa respetada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .PreCancellationWasRespected)}");

            log(
                $"Resultado controlado creado: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ControlledResultWasCreated)}");

            log(
                $"Identidad del lote creada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .BatchIdentityWasCreated)}");

            log(
                $"Tiempos registrados: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .TimesWereRecorded)}");

            log(
                $"Resultado vacío sin éxito falso: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .EmptyResultWasNotSuccessful)}");

            log(
                $"Identidad del lote preservada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .BatchIdentityWasPreserved)}");

            log(
                $"Lote inválido rechazado: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .InvalidBatchWasRejected)}");

            log(
                $"Solicitudes válidas inspeccionadas: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ValidRequestsWereInspected)}");

            log(
                $"Resultados productivos creados: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ProductiveResultsWereCreated)}");

            log(
                $"PrepareAsync ejecutado una vez: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .IndividualPrepareWasCalledOnce)}");

            log(
                $"CompleteAsync ejecutado una vez: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .IndividualCompleteWasCalledOnce)}");

            log(
                $"Decisión Declined reenviada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .DeclinedDecisionWasForwarded)}");

            log(
                $"Resultados Approved creados: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ApprovedResultsWereCreated)}");

            log(
                $"CompleteAsync Approved ejecutado una vez: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ApprovedCompleteWasCalledOnce)}");

            log(
                $"Decisión Approved reenviada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ApprovedDecisionWasForwarded)}");

            log(
                $"Decisión no admitida rechazada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .UnsupportedDecisionWasRejected)}");

            log(
                $"Fail-fast tras fallo en segunda preparación: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .FailFastStoppedAfterSecondPrepare)}");

            log(
                $"Resultado de fallo parcial devuelto: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .PartialFailureResultWasReturned)}");

            log(
                $"Fallo parcial preservado en el lote: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .PartialFailureWasPreserved)}");

            log(
                $"Fail-fast tras fallo en segunda finalización: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .FailFastStoppedAfterSecondComplete)}");

            log(
                $"Excepción de finalización preservada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .CompleteExceptionWasPreserved)}");

            log(
                $"Fallo devuelto por PrepareAsync controlado: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ReturnedPrepareFailureStoppedBatch)}");

            log(
                $"Fallo devuelto por CompleteAsync controlado: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ReturnedCompleteFailureStoppedBatch)}");

            log(
                $"Solicitudes restantes registradas: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .RemainingRequestsWereReported)}");

            log(
                $"Cancelación durante el lote respetada: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .MidBatchCancellationWasRespected)}");

            log(
                $"Lote Approved múltiple ejecutado: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .MultiApprovedBatchWasExecuted)}");

            log(
                $"Approved reenviado a todo el lote: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .MultiApprovedDecisionWasForwarded)}");

            log(
                $"Fallo Approved detuvo el lote: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ApprovedFailureStoppedBatch)}");

            log(
                $"Resultado Approved previo preservado: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .ApprovedFailurePreservedPreviousResult)}");

            log(
                $"Mensajes registrados: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .MessagesWereRecorded)}");

            log(
                $"Prueba del coordinador por lote correcta: " +
                $"{ToSpanish(
                    batchCoordinatorTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: {batchCoordinatorTestResult.Summary}");

            log(
                "=== Fin de las pruebas del coordinador productivo por lote ===");

            log(
                "=== Estado productivo batch de la interfaz ===");

            log(
                $"Archivos actualmente seleccionados: " +
                $"{currentSelection.FileCount}");

            log(
                $"Cambios actualmente seleccionados: " +
                $"{currentSelection.ApprovedChangeCount}");

            log(
                $"Resumen actual: " +
                $"{currentSelection.Summary}");

            log(
                "=== Fin del estado productivo batch de la interfaz ===");

            log(
                "Iniciando pruebas estructurales de la selección " +
                "productiva por lote.");

            ProductiveBatchSelectionTestResult
                productiveBatchSelectionTestResult =
                    _productiveBatchSelectionTestRunner.Run();

            log(
                "=== Selección productiva por lote ===");

            foreach (string message
                in productiveBatchSelectionTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Selección vacía creada: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .EmptySelectionWasCreated)}");

            log(
                $"Plan aprobado agregado: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .ApprovedPlanWasAdded)}");

            log(
                $"Ruta existente reemplazada sin duplicar: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .DuplicatePathWasReplaced)}");

            log(
                $"Segundo plan agregado: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .SecondPlanWasAdded)}");

            log(
                $"Conteos actualizados: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .CountsWereUpdated)}");

            log(
                $"Elemento eliminado: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .ItemWasRemoved)}");

            log(
                $"Plan sin aprobación eliminó selección previa: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .PlanWithoutApprovalRemovedExistingItem)}");

            log(
                $"Solicitud batch creada: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .BatchRequestWasCreated)}");

            log(
                $"Solicitud batch estructuralmente válida: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .BatchRequestWasStructurallyValid)}");

            log(
                $"Conteos preservados en solicitud batch: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .BatchCountsWerePreserved)}");

            log(
                $"Selección limpiada correctamente: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .SelectionWasCleared)}");

            log(
                $"Prueba de selección productiva por lote correcta: " +
                $"{ToSpanish(
                    productiveBatchSelectionTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: " +
                $"{productiveBatchSelectionTestResult.Summary}");

            log(
                "=== Fin de las pruebas de selección productiva por lote ===");

            log(
                "Iniciando pruebas de preparación productiva batch " +
                "en dos fases.");

            MetadataProductiveTwoPhaseBatchPreparationTestResult
                twoPhasePreparationTestResult =
                    await _metadataProductiveTwoPhaseBatchPreparationTestRunner
                        .RunAsync();

            log(
                "=== Preparación productiva batch en dos fases ===");

            foreach (string message
                in twoPhasePreparationTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Dependencia nula rechazada: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .NullCoordinatorWasRejected)}");

            log(
                $"Lote nulo rechazado: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .NullBatchWasRejected)}");

            log(
                $"Lote inválido rechazado: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .InvalidBatchWasRejected)}");

            log(
                $"Todas las solicitudes preparadas: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .AllRequestsWerePrepared)}");

            log(
                $"Preparaciones pendientes de decisión: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .PreparationsWerePending)}");

            log(
                $"Lote listo para decisión global: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .BatchWasReadyForDecision)}");

            log(
                $"Fallo de preparación detuvo el lote: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .PreparationFailureStoppedBatch)}");

            log(
                $"Preparaciones pendientes limpiadas: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .PendingPreparationsWereCleanedUp)}");

            log(
                $"Lote fallido bloqueado para promoción: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .FailedBatchWasNotReadyForDecision)}");

            log(
                $"Prueba de preparación batch en dos fases correcta: " +
                $"{ToSpanish(
                    twoPhasePreparationTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: {twoPhasePreparationTestResult.Summary}");

            log(
                "=== Fin de la preparación productiva batch en dos fases ===");

            log(
                "Iniciando pruebas de finalización productiva batch " +
                "en dos fases.");

            MetadataProductiveTwoPhaseBatchCompletionTestResult
                twoPhaseCompletionTestResult =
                    await _metadataProductiveTwoPhaseBatchCompletionTestRunner
                        .RunAsync();

            log(
                "=== Finalización productiva batch en dos fases ===");

            foreach (string message
                in twoPhaseCompletionTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Preparación nula rechazada: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .NullPreparationWasRejected)}");

            log(
                $"Decisión global inválida rechazada: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .UnsupportedDecisionWasRejected)}");

            log(
                $"Preparación no lista rechazada: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .InvalidPreparationWasRejected)}");

            log(
                $"Declined finalizó todo el lote: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .DeclinedCompletedAllPreparations)}");

            log(
                $"Declined reenviado a todo el lote: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .DeclinedWasForwardedToAll)}");

            log(
                $"Lote Declined correcto: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .DeclinedBatchWasSuccessful)}");

            log(
                $"Approved finalizó todo el lote: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .ApprovedCompletedAllPreparations)}");

            log(
                $"Approved reenviado a todo el lote: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .ApprovedWasForwardedToAll)}");

            log(
                $"Lote Approved correcto: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .ApprovedBatchWasSuccessful)}");

            log(
                $"Fallo Approved detuvo nuevas promociones: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .ApprovedFailureStoppedFurtherPromotion)}");

            log(
                $"Preparaciones restantes descartadas: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .RemainingPreparationsWereDeclined)}");

            log(
                $"Lote Approved fallido sin éxito falso: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .FailedApprovedBatchWasNotSuccessful)}");

            log(
                $"Cancelación limpió preparaciones pendientes: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .CancellationCleanedPendingPreparations)}");

            log(
                $"Prueba de finalización batch en dos fases correcta: " +
                $"{ToSpanish(
                    twoPhaseCompletionTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: {twoPhaseCompletionTestResult.Summary}");

            log(
                "=== Fin de la finalización productiva batch en dos fases ===");

            log(
                "Iniciando prueba de la fábrica de solicitudes " +
                "aisladas.");

            MetadataApplyRequestIsolationFactoryTestResult
                isolationFactoryTestResult =
                    _metadataApplyRequestIsolationFactoryTestRunner
                        .Run();

            log(
                "=== Fábrica de solicitudes aisladas ===");

            foreach (string message
                in isolationFactoryTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Solicitud nula rechazada: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .NullRequestWasRejected)}");

            log(
                $"Ruta vacía rechazada: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .EmptyPathWasRejected)}");

            log(
                $"Identificadores conservados: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .IdentifiersWerePreserved)}");

            log(
                $"Fecha de creación conservada: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .CreationTimeWasPreserved)}");

            log(
                $"Cambios conservados: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .ChangesWerePreserved)}");

            log(
                $"Requisitos conservados: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .RequirementsWerePreserved)}");

            log(
                $"Ruta aislada aplicada: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .WorkingCopyPathWasApplied)}");

            log(
                $"Nombre aislado aplicado: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .WorkingCopyFileNameWasApplied)}");

            log(
                $"Prueba de la fábrica aislada correcta: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: {isolationFactoryTestResult.Summary}");

            log(
                "=== Fin de la prueba de solicitudes aisladas ===");

            log(
                "Iniciando prueba integral del ejecutor aislado. " +
                "El archivo original permanecerá protegido.");

            MetadataApplicationIsolatedExecutorTestResult
                isolatedExecutorTestResult =
                    await _metadataApplicationIsolatedExecutorTestRunner
                        .RunAsync(
                            filePath);

            log(
                "=== Ejecutor coordinado sobre copia aislada ===");

            foreach (string message
                in isolatedExecutorTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Entorno aislado preparado: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .IsolationWasPrepared)}");

            log(
                $"Pipeline coordinado correcto: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .PipelineWasSuccessful)}");

            log(
                $"Archivo original intacto: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .OriginalFileRemainedUnchanged)}");

            log(
                $"Copia temporal modificada: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .WorkingCopyWasModified)}");

            log(
                $"Respaldo inicial preservado: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .InitialBackupWasPreserved)}");

            log(
                $"Limpieza temporal correcta: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .CleanupWasSuccessful)}");

            log(
                $"Género solicitado: " +
                $"{isolatedExecutorTestResult.RequestedGenre}");

            log(
                $"Género persistido: " +
                $"{isolatedExecutorTestResult.PersistedGenre}");

            log(
                $"Género verificado: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .GenreWasPersisted)}");

            if (!string.IsNullOrWhiteSpace(
                    isolatedExecutorTestResult.ErrorMessage))
            {
                log(
                    $"Error del ejecutor aislado: " +
                    $"{isolatedExecutorTestResult.ErrorMessage}");
            }

            log(
                $"Prueba del ejecutor aislado correcta: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: " +
                $"{isolatedExecutorTestResult.Summary}");

            log(
                "=== Fin de la prueba del ejecutor aislado ===");

            log(
                "Iniciando prueba de conservación controlada de una " +
                "copia verificada.");

            MetadataApplicationPreservedExecutionTestResult
                preservedExecutionTestResult =
                    await _metadataApplicationPreservedExecutionTestRunner
                        .RunAsync(
                            filePath);

            log(
                "=== Conservación controlada de copia verificada ===");

            foreach (string message
                in preservedExecutionTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Ejecución correcta: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .ExecutionWasSuccessful)}");

            log(
                $"Entorno conservado: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .EnvironmentWasPreserved)}");

            log(
                $"Limpieza automática pospuesta: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .CleanupWasDeferred)}");

            log(
                $"Copia verificada disponible: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .WorkingCopyStillExisted)}");

            log(
                $"Respaldo inicial disponible: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .InitialBackupStillExisted)}");

            log(
                $"Archivo original intacto: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .OriginalFileRemainedUnchanged)}");

            log(
                $"Copia conservada modificada: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .WorkingCopyWasModified)}");

            log(
                $"Limpieza manual correcta: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .ManualCleanupWasSuccessful)}");

            log(
                $"Carpeta temporal eliminada: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .TemporaryDirectoryWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    preservedExecutionTestResult.ErrorMessage))
            {
                log(
                    $"Error de conservación controlada: " +
                    $"{preservedExecutionTestResult.ErrorMessage}");
            }

            log(
                $"Prueba de conservación correcta: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: " +
                $"{preservedExecutionTestResult.Summary}");

            log(
                "=== Fin de la prueba de conservación controlada ===");

            log(
                "Iniciando prueba de promoción controlada sobre archivos " +
                "temporales. El archivo seleccionado no será modificado.");

            MetadataApplicationPromotionTestResult
                promotionTestResult =
                    await _metadataApplicationPromotionTestRunner
                        .RunAsync(
                            filePath);

            log(
                "=== Promoción controlada sobre destino temporal ===");

            foreach (string message
                in promotionTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Entorno temporal preparado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .TestEnvironmentWasPrepared)}");

            log(
                $"Entradas validadas: " +
                $"{ToSpanish(
                    promotionTestResult
                        .InputsWereValidated)}");

            log(
                $"Respaldo productivo creado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .ProductiveBackupWasCreated)}");

            log(
                $"Respaldo productivo verificado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .ProductiveBackupWasVerified)}");

            log(
                $"Sustitución ejecutada: " +
                $"{ToSpanish(
                    promotionTestResult
                        .ReplacementWasExecuted)}");

            log(
                $"Destino promovido verificado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .PromotedFileWasVerified)}");

            log(
                $"Original de referencia intacto: " +
                $"{ToSpanish(
                    promotionTestResult
                        .ReferenceOriginalRemainedUnchanged)}");

            log(
                $"Copia verificada preservada: " +
                $"{ToSpanish(
                    promotionTestResult
                        .VerifiedCopyWasPreserved)}");

            log(
                $"Reversión no requerida: " +
                $"{ToSpanish(
                    promotionTestResult
                        .RollbackWasNotRequired)}");

            log(
                $"Entorno temporal eliminado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .TestEnvironmentWasRemoved)}");

            log(
                $"Respaldo temporal eliminado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .TemporaryBackupWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    promotionTestResult.ErrorMessage))
            {
                log(
                    $"Error de promoción controlada: " +
                    $"{promotionTestResult.ErrorMessage}");
            }

            log(
                $"Prueba de promoción correcta: " +
                $"{ToSpanish(
                    promotionTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: {promotionTestResult.Summary}");

            log(
                "=== Fin de la prueba de promoción controlada ===");

            log(
                "Iniciando prueba de reversión automática sobre archivos " +
                "temporales. El archivo seleccionado no será modificado.");

            MetadataApplicationRollbackTestResult
                rollbackTestResult =
                    await _metadataApplicationRollbackTestRunner
                        .RunAsync(
                            filePath);

            log(
                "=== Reversión automática sobre destino temporal ===");

            foreach (string message
                in rollbackTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Entorno temporal preparado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .TestEnvironmentWasPrepared)}");

            log(
                $"Entradas validadas: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .InputsWereValidated)}");

            log(
                $"Respaldo productivo creado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .ProductiveBackupWasCreated)}");

            log(
                $"Respaldo productivo verificado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .ProductiveBackupWasVerified)}");

            log(
                $"Sustitución temporal ejecutada: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .ReplacementWasExecuted)}");

            log(
                $"Fallo de verificación simulado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .VerificationFailureWasSimulated)}");

            log(
                $"Reversión iniciada: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .RollbackWasAttempted)}");

            log(
                $"Reversión correcta: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .RollbackWasSuccessful)}");

            log(
                $"Destino restaurado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .DestinationWasRestored)}");

            log(
                $"Original de referencia intacto: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .ReferenceOriginalRemainedUnchanged)}");

            log(
                $"Copia verificada preservada: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .VerifiedCopyWasPreserved)}");

            log(
                $"Destino en estado seguro: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .DestinationEndedInSafeState)}");

            log(
                $"Entorno temporal eliminado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .TestEnvironmentWasRemoved)}");

            log(
                $"Respaldo temporal eliminado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .TemporaryBackupWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    rollbackTestResult.ExpectedErrorMessage))
            {
                log(
                    $"Error esperado de la simulación: " +
                    $"{rollbackTestResult.ExpectedErrorMessage}");
            }

            log(
                $"Prueba de reversión correcta: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: {rollbackTestResult.Summary}");

            log(
                "=== Fin de la prueba de reversión automática ===");

            log(
                "Iniciando prueba controlada del coordinador productivo " +
                "individual. El archivo seleccionado no será modificado.");

            MetadataProductiveApplicationCoordinatorTestResult
                productiveCoordinatorTestResult =
                    await _metadataProductiveApplicationCoordinatorTestRunner
                        .RunAsync(
                            filePath);

            log(
                "=== Coordinador productivo individual temporal ===");

            foreach (string message
                in productiveCoordinatorTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Solicitud nula rechazada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .NullRequestWasRejected)}");

            log(
                $"Copia verificada preparada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .VerifiedCopyWasPrepared)}");

            log(
                $"Decisión de promoción pendiente: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .PromotionDecisionWasPending)}");

            log(
                $"Destino intacto durante preparación: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .OriginalRemainedUnchangedDuringPreparation)}");

            log(
                $"Decisión Declined procesada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedDecisionWasHandled)}");

            log(
                $"Promoción omitida tras rechazo: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedDecisionSkippedPromotion)}");

            log(
                $"Destino seguro después del rechazo: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedOriginalEndedInSafeState)}");

            log(
                $"Entorno aislado eliminado: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedEnvironmentWasCleaned)}");

            log(
                $"Rechazo finalizado correctamente: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedResultWasSuccessful)}");

            log(
                $"Decisión inválida rechazada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .InvalidDecisionWasRejected)}");

            log(
                $"Reutilización de preparación rechazada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .ReusedPreparationWasRejected)}");

            log(
                $"Entorno temporal general eliminado: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .TemporaryEnvironmentWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    productiveCoordinatorTestResult.ErrorMessage))
            {
                log(
                    $"Error del coordinador productivo: " +
                    $"{productiveCoordinatorTestResult.ErrorMessage}");
            }

            log(
                $"Prueba del coordinador productivo correcta: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: " +
                $"{productiveCoordinatorTestResult.Summary}");

            log(
                "=== Fin de la prueba del coordinador productivo ===");

            log(
                "Iniciando prueba controlada del camino Approved del " +
                "coordinador productivo. El archivo seleccionado no será " +
                "modificado.");

            MetadataProductiveApplicationApprovedTestResult
                productiveApprovedTestResult =
                    await _metadataProductiveApplicationApprovedTestRunner
                        .RunAsync(
                            filePath);

            log(
                "=== Camino Approved del coordinador productivo ===");

            foreach (string message
                in productiveApprovedTestResult.Messages)
            {
                log(
                    $"- {message}");
            }

            log(
                $"Entorno temporal preparado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .TestEnvironmentWasPrepared)}");

            log(
                $"Copia verificada preparada: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .VerifiedCopyWasPrepared)}");

            log(
                $"Decisión de promoción pendiente: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .PromotionDecisionWasPending)}");

            log(
                $"Destino intacto durante preparación: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .DestinationRemainedUnchangedDuringPreparation)}");

            log(
                $"Decisión Approved procesada: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ApprovedDecisionWasHandled)}");

            log(
                $"Promoción correcta: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .PromotionWasSuccessful)}");

            log(
                $"Respaldo productivo creado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ProductiveBackupWasCreated)}");

            log(
                $"Respaldo productivo verificado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ProductiveBackupWasVerified)}");

            log(
                $"Sustitución ejecutada: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ReplacementWasExecuted)}");

            log(
                $"Destino promovido verificado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .PromotedDestinationWasVerified)}");

            log(
                $"Género solicitado persistido: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .RequestedGenreWasPersisted)}");

            log(
                $"Género solicitado: " +
                $"{productiveApprovedTestResult.RequestedGenre}");

            log(
                $"Género persistido: " +
                $"{productiveApprovedTestResult.PersistedGenre}");

            log(
                $"Reversión no requerida: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .RollbackWasNotRequired)}");

            log(
                $"Original de referencia intacto: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ReferenceOriginalRemainedUnchanged)}");

            log(
                $"Destino en estado seguro: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .DestinationEndedInSafeState)}");

            log(
                $"Limpieza final intentada: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .FinalCleanupWasAttempted)}");

            log(
                $"Limpieza final correcta: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .FinalCleanupWasSuccessful)}");

            log(
                $"Resultado productivo correcto: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ProductiveResultWasSuccessful)}");

            log(
                $"Entorno temporal general eliminado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .TemporaryEnvironmentWasRemoved)}");

            log(
                $"Respaldo productivo temporal eliminado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .TemporaryProductiveBackupWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    productiveApprovedTestResult.ErrorMessage))
            {
                log(
                    $"Error del camino Approved: " +
                    $"{productiveApprovedTestResult.ErrorMessage}");
            }

            log(
                $"Prueba Approved correcta: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .WasSuccessful)}");

            log(
                $"Resumen: " +
                $"{productiveApprovedTestResult.Summary}");

            log(
                "=== Fin de la prueba Approved del coordinador productivo ===");

            await RunMetadataApplicationPipelineDiagnosticAsync(
                filePath,
                log);

            string extension =
                Path.GetExtension(filePath)
                    .ToLowerInvariant();

            switch (extension)
            {
                case ".mp3":
                    await RunMp3DiagnosticAsync(
                        filePath,
                        log);
                    break;

                case ".flac":
                    await RunFlacDiagnosticAsync(
                        filePath,
                        log);
                    break;

                default:
                    log(
                        "El análisis técnico general terminó. " +
                        $"Todavía no existe una prueba aislada de " +
                        $"escritura para el formato {extension}.");
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            log(
                "El diagnóstico o la prueba aislada fueron " +
                "cancelados.");
        }
        catch (Exception exception)
        {
            log(
                $"No fue posible ejecutar el diagnóstico. " +
                $"Detalle: {exception.Message}");
        }
    }

    /// <summary>
    /// Ejecuta las cuatro etapas reales del pipeline sobre una copia
    /// temporal aislada del archivo seleccionado.
    /// 
    /// El archivo original nunca se entrega a las etapas de
    /// escritura.
    /// </summary>
    private async Task
        RunMetadataApplicationPipelineDiagnosticAsync(
            string filePath,
            Action<string> log)
    {
        log(
            "Iniciando prueba integral aislada del pipeline.");

        MetadataApplicationPipelineIsolatedTestRunner testRunner =
            new();

        MetadataApplicationPipelineIsolatedTestResult testResult =
            await testRunner.RunAsync(
                filePath,
                requestedGenre:
                    DiagnosticMetadataTestValues.CreateGenre());

        log(
            "=== Pipeline integral aislado ===");

        log(
            $"Entorno preparado: " +
            $"{ToSpanish(testResult.EnvironmentWasPrepared)}");

        log(
            $"Etapas registradas: " +
            $"{testResult.RegisteredStageCount}");

        log(
            $"Etapas ejecutadas: " +
            $"{testResult.ExecutedStageCount}");

        log(
            $"Ejecución del pipeline correcta: " +
            $"{ToSpanish(
                testResult.PipelineExecutionWasSuccessful)}");

        log(
            $"Respaldo del pipeline correcto: " +
            $"{ToSpanish(
                testResult
                    .PipelineBackupWasSuccessfulBeforeCleanup)}");

        log(
            $"Escritura correcta: " +
            $"{ToSpanish(testResult.WriteWasSuccessful)}");

        log(
            $"Verificación posterior correcta: " +
            $"{ToSpanish(
                testResult.VerificationWasSuccessful)}");

        log(
            $"Género verificado: " +
            $"{ToSpanish(
                testResult.GenreVerificationWasSuccessful)}");

        log(
            $"Género solicitado: " +
            $"{DisplayDiagnosticValue(
                testResult.RequestedGenre)}");

        log(
            $"Género persistido: " +
            $"{DisplayDiagnosticValue(
                testResult.PersistedGenre)}");

        log(
            $"Imágenes antes: " +
            $"{testResult.PictureCountBefore}");

        log(
            $"Imágenes después: " +
            $"{testResult.PictureCountAfter}");

        log(
            $"Limpieza ejecutada: " +
            $"{ToSpanish(testResult.CleanupWasAttempted)}");

        log(
            $"Carpeta temporal eliminada: " +
            $"{ToSpanish(testResult.TestDirectoryWasRemoved)}");

        foreach (string message in testResult.Messages)
        {
            log(
                $"- {message}");
        }

        if (!string.IsNullOrWhiteSpace(
                testResult.ErrorMessage))
        {
            log(
                $"Error: {testResult.ErrorMessage}");
        }

        log(
            $"Prueba integral correcta: " +
            $"{ToSpanish(testResult.WasSuccessful)}");

        log(
            $"Resumen: {testResult.Summary}");

        log(
            "=== Fin del pipeline integral aislado ===");
    }


    /// <summary>
    /// Ejecuta la inspección de sólo lectura y la prueba aislada
    /// de escritura para un archivo MP3.
    ///
    /// El archivo original nunca se modifica.
    /// </summary>
    private async Task RunMp3DiagnosticAsync(
        string filePath,
        Action<string> log)
    {
        log(
            "Iniciando inspección MP3 de sólo lectura.");

        TagLibMp3MetadataAdapter tagLibAdapter =
            new();

        TagLibMp3InspectionResult inspectionResult =
            await Task.Run(
                () =>
                    tagLibAdapter.Inspect(
                        filePath));

        string inspectionReport =
            TagLibMp3InspectionDiagnostics.BuildReport(
                inspectionResult);

        log(inspectionReport);

        if (!inspectionResult.WasSuccessful)
        {
            log(
                "La inspección MP3 no terminó correctamente. " +
                "La prueba aislada no se ejecutará.");

            return;
        }

        log(
            "Iniciando prueba aislada de escritura MP3. " +
            "El archivo original permanecerá intacto.");

        TagLibMp3IsolatedWriteTestRunner isolatedTestRunner =
            new();

        TagLibMp3IsolatedWriteTestResult isolatedTestResult =
            await isolatedTestRunner.RunAsync(
                filePath,
                requestedGenre:
                    DiagnosticMetadataTestValues.CreateGenre());

        string isolatedTestReport =
            TagLibMp3IsolatedWriteTestDiagnostics.BuildReport(
                isolatedTestResult);

        log(isolatedTestReport);

        if (isolatedTestResult.WasSuccessful)
        {
            log(
                "La prueba aislada MP3 terminó correctamente. " +
                "El género fue guardado en la copia temporal, " +
                "la carátula fue preservada y el archivo original " +
                "permaneció intacto.");

            log(
                $"Carpeta de la prueba MP3: " +
                $"{isolatedTestResult.TestDirectoryPath}");

            return;
        }

        log(
            "La prueba aislada MP3 no superó todas las " +
            "comprobaciones. El escritor real continuará " +
            "desactivado en el pipeline principal.");
    }


    /// <summary>
    /// Ejecuta la primera prueba real de escritura FLAC sobre una
    /// copia temporal aislada.
    ///
    /// En esta fase no modifica el archivo original ni activa el
    /// escritor FLAC dentro del pipeline principal.
    /// </summary>
    private async Task RunFlacDiagnosticAsync(
        string filePath,
        Action<string> log)
    {
        log(
            "Iniciando prueba aislada de escritura FLAC. " +
            "El archivo original permanecerá intacto.");

        TagLibFlacIsolatedWriteTestRunner isolatedTestRunner =
            new();

        TagLibIsolatedWriteTestResult isolatedTestResult =
            await isolatedTestRunner.RunAsync(
                filePath,
                requestedGenre:
                    DiagnosticMetadataTestValues.CreateGenre());

        log(
            "=== Prueba aislada de escritura FLAC ===");

        log(
            $"Archivo original: " +
            $"{isolatedTestResult.OriginalFilePath}");

        log(
            $"Copia de trabajo: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.WorkingCopyPath)}");

        log(
            $"Respaldo de la copia: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.WorkingBackupPath)}");

        log(
            $"Género original: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.OriginalGenre)}");

        log(
            $"Género solicitado: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.RequestedGenre)}");

        log(
            $"Género persistido: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.PersistedGenre)}");

        log(
            $"Original intacto: " +
            $"{ToSpanish(
                isolatedTestResult
                    .OriginalFileRemainedUnchanged)}");

        log(
            $"Respaldo coincide con la copia inicial: " +
            $"{ToSpanish(
                isolatedTestResult
                    .BackupMatchesInitialWorkingCopy)}");

        log(
            $"Copia modificada realmente: " +
            $"{ToSpanish(
                isolatedTestResult
                    .WorkingCopyWasModified)}");

        log(
            $"Género verificado: " +
            $"{ToSpanish(
                isolatedTestResult.GenreWasPersisted)}");

        log(
            $"Imágenes antes: " +
            $"{isolatedTestResult.PictureCountBefore}");

        log(
            $"Imágenes después: " +
            $"{isolatedTestResult.PictureCountAfter}");

        log(
            $"Carátulas preservadas: " +
            $"{ToSpanish(
                isolatedTestResult.PicturesWerePreserved)}");

        if (isolatedTestResult.WriteResult is not null)
        {
            log(
                $"Escritor utilizado: " +
                $"{isolatedTestResult.WriteResult.WriterName}");

            log(
                $"Estado de escritura: " +
                $"{isolatedTestResult.WriteResult.Status}");

            log(
                $"Campos escritos: " +
                $"{isolatedTestResult.WriteResult.WrittenFieldCount}");

            log(
                $"Campos fallidos: " +
                $"{isolatedTestResult.WriteResult.FailedFieldCount}");

            foreach (string message
                in isolatedTestResult.WriteResult.Messages)
            {
                log(
                    $"- {message}");
            }
        }

        foreach (string message
            in isolatedTestResult.Messages)
        {
            log(
                $"- {message}");
        }

        log(
            $"Prueba FLAC correcta: " +
            $"{ToSpanish(
                isolatedTestResult.WasSuccessful)}");

        log(
            $"Resumen: {isolatedTestResult.Summary}");

        log(
            "=== Fin de la prueba aislada FLAC ===");

        if (isolatedTestResult.WasSuccessful)
        {
            log(
                "La prueba aislada FLAC terminó correctamente. " +
                "El género fue guardado en la copia temporal, " +
                "las imágenes fueron preservadas y el archivo " +
                "original permaneció intacto.");

            log(
                $"Carpeta de la prueba FLAC: " +
                $"{isolatedTestResult.TestDirectoryPath}");

            return;
        }

        log(
            "La prueba aislada FLAC no superó todas las " +
            "comprobaciones. El escritor FLAC continuará " +
            "desactivado en el pipeline principal.");
    }


    /// <summary>
    /// Prepara un valor para mostrarlo en los informes de
    /// diagnóstico.
    /// </summary>
    private static string DisplayDiagnosticValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(sin información)"
            : value.Trim();
    }


    /// <summary>
    /// Convierte un valor lógico a texto en español
    /// para el registro de actividad.
    /// </summary>
    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }

}
