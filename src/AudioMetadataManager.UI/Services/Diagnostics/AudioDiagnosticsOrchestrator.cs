using AudioMetadataManager.UI.Services.AudioAnalysis.Diagnostics;
using System.IO;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Integration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineExecution;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Adapters;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Testing;
using AudioMetadataManager.UI.Views.Models.Simulation;

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

    public AudioDiagnosticsOrchestrator(
        AudioAnalysisTestRunner audioAnalysisTestRunner)
    {
        _audioAnalysisTestRunner =
            audioAnalysisTestRunner;
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
