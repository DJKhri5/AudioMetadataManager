using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services;
using AudioMetadataManager.UI.Services.AudioAnalysis;
using AudioMetadataManager.UI.Services.AudioAnalysis.Diagnostics;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;
using AudioMetadataManager.UI.Services.MetadataSources;
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
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Pipeline;
using AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Mapping;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation.Application.Pipeline.Integration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
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
    .Application.Writing.TagLibIntegration.Preparation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Testing;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Decision;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;
using AudioMetadataManager.UI.Views;
using AudioMetadataManager.UI.Views.Models.Simulation;
using AudioMetadataManager.UI.Views.Models.Simulation.Mapping;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ConsensusResult =
    AudioMetadataManager.UI.Services.MetadataSources
        .Consensus.Models.MetadataConsensusResult;

namespace AudioMetadataManager.UI;

public partial class MainWindow : Window
{
    private readonly FileScannerService _fileScannerService =
        new();

    private readonly FileNameParserService
        _fileNameParserService =
            new();

    private readonly AudioAnalysisEngine 
        _audioAnalysisEngine;

    private readonly AudioAnalysisTestRunner
        _audioAnalysisTestRunner;

    private readonly MetadataVerificationStageTestRunner
        _metadataVerificationStageTestRunner;

    private readonly
        MetadataApplicationPipelineFactoryTestRunner
            _metadataApplicationPipelineFactoryTestRunner;

    private SimulationPlanViewModel?
        _currentSimulationPlan;

    /// <summary>
    /// Construye y valida una solicitud usando solamente los
    /// cambios aprobados por el usuario.
    ///
    /// Para archivos MP3, primero prepara los cambios mediante
    /// TagLibSharp exclusivamente en memoria y verifica que el
    /// archivo físico permanezca intacto.
    ///
    /// Después ejecuta el pipeline seguro, que crea el respaldo
    /// obligatorio y utiliza todavía un escritor de diagnóstico.
    /// </summary>
    private async void
        AudioFileDetailsViewControl_ValidateApprovedChangesRequested(
            object? sender,
            EventArgs e)
    {
        if (_currentSimulationPlan is null)
        {
            AppendLog(
                "No existe un plan de simulación activo.");

            return;
        }

        if (!_currentSimulationPlan.HasApprovedChanges)
        {
            AppendLog(
                "No existen cambios aprobados para validar.");

            return;
        }

        AppendLog(
            "Iniciando validación de cambios aprobados.");

        try
        {
            MetadataApplyRequestFactory requestFactory =
                new();

            var request =
                requestFactory.Create(
                    _currentSimulationPlan);

            /*
             * Preparación MP3 exclusivamente en memoria.
             *
             * Esta prueba asigna los cambios al objeto TagLib.Tag,
             * pero no ejecuta TagLib.File.Save().
             */
            if (string.Equals(
                    Path.GetExtension(
                        request.FilePath),
                    ".mp3",
                    StringComparison.OrdinalIgnoreCase))
            {
                AppendLog(
                    "Iniciando preparación MP3 en memoria.");

                TagLibMp3ChangePreparer preparer =
                    new();

                TagLibMp3PreparationResult preparationResult =
                    await Task.Run(
                        () =>
                            preparer.Prepare(
                                request.FilePath,
                                request.ValidChanges));

                string preparationReport =
                    TagLibMp3PreparationDiagnostics.BuildReport(
                        preparationResult);

                LogTextBox.AppendText(
                    Environment.NewLine +
                    preparationReport +
                    Environment.NewLine);

                LogTextBox.ScrollToEnd();

                if (!preparationResult.WasSuccessful)
                {
                    AppendLog(
                        "La preparación MP3 en memoria no superó " +
                        "todas las comprobaciones. El pipeline no " +
                        "continuará.");

                    return;
                }

                AppendLog(
                    "La preparación MP3 en memoria terminó " +
                    "correctamente. El archivo físico permaneció " +
                    "intacto.");
            }
            else
            {
                AppendLog(
                    "La preparación TagLibSharp en memoria se " +
                    "omitió porque el archivo seleccionado no es MP3.");
            }

            /*
             * Después de superar la preparación en memoria,
             * ejecutamos el pipeline seguro existente.
             */
            MetadataApplicationPipeline pipeline =
                new();

            Progress<MetadataApplicationProgress> progress =
                new(
                    progressUpdate =>
                    {
                        AppendLog(
                            $"Aplicación segura: " +
                            $"{progressUpdate.Summary}");
                    });

            MetadataApplicationPipelineResult result =
                await pipeline.ExecuteAsync(
                    request,
                    progress);

            string report =
                MetadataApplicationPipelineDiagnostics
                    .BuildReport(
                        result);

            LogTextBox.AppendText(
                Environment.NewLine +
                report +
                Environment.NewLine);

            LogTextBox.ScrollToEnd();

            if (result.ValidationResult?.IsValid != true)
            {
                AppendLog(
                    "La solicitud fue rechazada por la " +
                    "validación previa.");

                return;
            }

            if (result.BackupResult?.WasSuccessful == true)
            {
                AppendLog(
                    "La solicitud fue validada y el respaldo se " +
                    "creó correctamente. Ningún metadato fue " +
                    "modificado.");

                AppendLog(
                    $"Ruta del respaldo: " +
                    $"{result.BackupResult.BackupFilePath}");

                return;
            }

            AppendLog(
                "La solicitud fue validada, pero el respaldo no " +
                "pudo completarse. Ningún metadato fue modificado.");
        }
        catch (Exception exception)
        {
            AppendLog(
                "No fue posible validar los cambios aprobados. " +
                $"Detalle: {exception.Message}");
        }
    }

    /// <summary>
    /// Abre la ventana de configuración de las fuentes externas
    /// de metadatos.
    /// </summary>
    private void OpenMetadataSourcesSettings_Click(
        object sender,
        RoutedEventArgs e)
    {
        MetadataSourcesSettingsWindow settingsWindow =
            new()
            {
                Owner = this
            };

        settingsWindow.ShowDialog();
    }

    public MainWindow()
    {
        InitializeComponent();

        AudioFileDetailsViewControl
            .ValidateApprovedChangesRequested +=
                AudioFileDetailsViewControl_ValidateApprovedChangesRequested;

        _audioAnalysisEngine =
            new AudioAnalysisEngine();

        _audioAnalysisTestRunner =
            new AudioAnalysisTestRunner(
                _audioAnalysisEngine);

        _metadataVerificationStageTestRunner =
            new MetadataVerificationStageTestRunner();

        _metadataApplicationPipelineFactoryTestRunner =
            new MetadataApplicationPipelineFactoryTestRunner();
    }

    /// <summary>
    /// Escanea la carpeta seleccionada y muestra los archivos
    /// compatibles en la tabla.
    ///
    /// Este proceso no ejecuta todavía el análisis PCM.
    /// </summary>
    private void ScanButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string folderPath =
            LibraryPathTextBox.Text;

        LogTextBox.AppendText(
            $"{Environment.NewLine}" +
            "Iniciando análisis de la biblioteca...");

        List<AudioFile> audioFiles =
            _fileScannerService.ScanFolder(
                folderPath);

        AudioFilesDataGrid.ItemsSource =
            audioFiles;

        AudioFilesDataGrid.SelectedItem =
            null;

        UpdateSelectedFileButtons();

        LogTextBox.AppendText(
            $"{Environment.NewLine}" +
            $"Análisis finalizado. Se encontraron " +
            $"{audioFiles.Count} archivos compatibles.");

        LogTextBox.ScrollToEnd();

        SaveProjectButton.IsEnabled =
            audioFiles.Count > 0;

        ExportButton.IsEnabled =
            audioFiles.Count > 0;
    }

    /// <summary>
    /// Abre el selector de carpetas.
    /// </summary>
    private void BrowseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        OpenFolderDialog dialog = new()
        {
            Title =
                "Seleccione la biblioteca musical",

            Multiselect =
                false
        };

        bool? result =
            dialog.ShowDialog(this);

        if (result != true)
        {
            return;
        }

        LibraryPathTextBox.Text =
            dialog.FolderName;

        ScanButton.IsEnabled =
            true;

        AudioFilesDataGrid.ItemsSource =
            null;

        AudioFilesDataGrid.SelectedItem =
            null;

        UpdateSelectedFileButtons();

        SaveProjectButton.IsEnabled =
            false;

        ExportButton.IsEnabled =
            false;

        ApplyChangesButton.IsEnabled =
            false;

        LogTextBox.AppendText(
            $"{Environment.NewLine}" +
            $"Biblioteca seleccionada: " +
            $"{dialog.FolderName}");

        LogTextBox.ScrollToEnd();
    }

    /// <summary>
    /// Habilita las acciones cuando existe una fila
    /// seleccionada.
    /// </summary>
    private void AudioFilesDataGrid_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        UpdateSelectedFileButtons();
    }

    /// <summary>
    /// Ejecuta el pipeline sobre el archivo seleccionado
    /// y muestra un resumen compacto.
    /// </summary>
    private async void AnalyzeSelectedFileButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        AudioFile? audioFile =
            GetSelectedAudioFile();

        if (audioFile is null)
        {
            AppendLog(
                "No existe un archivo seleccionado.");

            return;
        }

        if (!TryGetValidFilePath(
                audioFile,
                out string filePath))
        {
            return;
        }

        SetAudioAnalysisControlsEnabled(
            false);

        AppendLog(
            $"Iniciando análisis técnico: " +
            $"{audioFile.FileName}");

        try
        {
            /*
             * El pipeline se ejecuta en un hilo de trabajo para
             * evitar que la ventana quede bloqueada durante la
             * decodificación PCM.
             */
            AudioAnalysisResult result =
                await Task.Run(
                    async () =>
                        await _audioAnalysisEngine.AnalyzeAsync(
                            filePath));

            AppendLog(
                $"Análisis técnico finalizado: " +
                $"{audioFile.FileName}");

            AppendLog(
                $"Estado: {result.StatusDisplay}");

            AppendLog(
                $"Resumen: {result.SummaryDisplay}");

            if (result.Warnings.Count > 0)
            {
                AppendLog(
                    "Advertencias:");

                foreach (string warning in result.Warnings)
                {
                    AppendLog(
                        $"- {warning}");
                }
            }
            ParsedFileName parsedFileName =
                _fileNameParserService.Parse(
                    audioFile);

            MetadataSearchRequest searchRequest =
                new()
                {
                    FileName =
                        audioFile.FileName,

                    ParsedArtist =
                        parsedFileName.Artist,

                    ParsedTitle =
                        parsedFileName.Title,

                    ParsedVersion =
                        parsedFileName.Version,

                    TaggedArtist =
                        audioFile.Artist,
        
                    TaggedTitle =
                        audioFile.Title,

                    TaggedAlbum =
                        audioFile.Album,

                    TaggedYear =
                        audioFile.Year,

                    Duration =
                        audioFile.Duration
                };

            MetadataSearchContext searchContext =
                new(searchRequest);

            MetadataSourceManager sourceManager =
                MetadataSourceFactory.CreateDefault();

            MetadataSearchPipeline pipeline =
                new(sourceManager);

            MetadataSearchPipelineResult pipelineResult =
                await pipeline.ExecuteAsync(
                    searchContext);

            // Informe del pipeline.
            string pipelineReport =
                MetadataSearchPipelineDiagnostics.BuildReport(
                    pipelineResult);

            LogTextBox.AppendText(
                Environment.NewLine +
                pipelineReport +
                Environment.NewLine);

            // Identidad local y ranking.
            LocalMetadataComparisonInputFactory
                localMetadataFactory =
                    new();

            MetadataComparisonInput localMetadata =
                localMetadataFactory.Create(
                    audioFile,
                    parsedFileName);

            MetadataCandidateEvaluationEngine
                candidateEvaluationEngine =
                    new();

            MetadataCandidateEvaluationBatchResult
                candidateEvaluationBatch =
                    candidateEvaluationEngine.EvaluateBatch(
                        localMetadata,
                        pipelineResult.Candidates);

            string candidateRankingReport =
                MetadataCandidateEvaluationDiagnostics.BuildReport(
                    candidateEvaluationBatch);

            LogTextBox.AppendText(
                Environment.NewLine +
                candidateRankingReport +
                Environment.NewLine);

            // Nuevo consenso.
            MetadataConsensusOrchestrator
                consensusOrchestrator =
                    new();

            ConsensusResult consensusResult =
                consensusOrchestrator.Evaluate(
                    candidateEvaluationBatch);

            string consensusReport =
                MetadataConsensusDiagnostics.BuildReport(
                    consensusResult);

            LogTextBox.AppendText(
                Environment.NewLine +
                consensusReport +
                Environment.NewLine);

            MetadataChangeDecisionEngine
                changeDecisionEngine =
                    new();

            MetadataChangePlan changePlan =
                changeDecisionEngine.BuildPlan(
                    audioFile,
                    consensusResult);

            SimulationPlanViewModelFactory
                simulationPlanFactory =
                    new();

            _currentSimulationPlan =
                simulationPlanFactory.Create(
                    changePlan);

            AudioFileDetailsViewControl.SimulationPlan =
                _currentSimulationPlan;

            string changePlanReport =
                MetadataChangePlanDiagnostics.BuildReport(
                    changePlan);

            LogTextBox.AppendText(
                Environment.NewLine +
                changePlanReport +
                Environment.NewLine);

            LogTextBox.ScrollToEnd();

            LogTextBox.ScrollToEnd();

            AppendLog(
                $"Nombre interpretado: " +
                $"{parsedFileName.CleanName}");

            AppendLog(
                $"Parser completado correctamente: " +
                $"{ToSpanish(parsedFileName.WasParsedSuccessfully)}");

            if (!string.IsNullOrWhiteSpace(
                    parsedFileName.Notes))
            {
                AppendLog(
                    $"Observación del parser: " +
                    $"{parsedFileName.Notes}");
            }

            MetadataComparisonDiagnostics metadataDiagnostics =
                new();

            string metadataReport =
                metadataDiagnostics.Run(
                    audioFile,
                    parsedFileName);

            LogTextBox.AppendText(
                Environment.NewLine +
                metadataReport +
                Environment.NewLine);

            LogTextBox.ScrollToEnd();
        }
        catch (Exception exception)
        {
            AppendLog(
                $"No fue posible analizar el archivo. " +
                $"Detalle: {exception.Message}");
        }
        finally
        {
            SetAudioAnalysisControlsEnabled(
                true);
        }
    }

    /// <summary>
    /// Ejecuta el diagnóstico técnico general y selecciona
    /// automáticamente la prueba TagLibSharp correspondiente al
    /// formato del archivo.
    ///
    /// Las escrituras reales se realizan únicamente sobre copias
    /// temporales aisladas. El archivo original nunca se entrega
    /// a los escritores.
    /// </summary>
    private async void RunAudioDiagnosticButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        AudioFile? audioFile =
            GetSelectedAudioFile();

        if (audioFile is null)
        {
            AppendLog(
                "No existe un archivo seleccionado.");

            return;
        }

        if (!TryGetValidFilePath(
                audioFile,
                out string filePath))
        {
            return;
        }

        SetAudioAnalysisControlsEnabled(
            false);

        AppendLog(
            $"Iniciando diagnóstico técnico: " +
            $"{audioFile.FileName}");

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

            LogTextBox.AppendText(
                Environment.NewLine +
                Environment.NewLine +
                report.ReportText +
                Environment.NewLine);

            LogTextBox.ScrollToEnd();

            AppendLog(
                "Iniciando pruebas estructurales de la etapa " +
                "de verificación.");

            MetadataVerificationStageTestResult
                verificationStageTestResult =
                    await _metadataVerificationStageTestRunner
                        .RunAsync();

            AppendLog(
                "=== Etapa de verificación posterior a la " +
                "escritura ===");

            foreach (string message
                in verificationStageTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Resultado general: " +
                $"{ToSpanish(
                    verificationStageTestResult
                        .WasSuccessful)}");

            AppendLog(
                "=== Fin de las pruebas estructurales de " +
                "verificación ===");

            AppendLog(
                "Iniciando pruebas estructurales de la " +
                "composición del pipeline.");

            MetadataApplicationPipelineFactoryTestResult
                pipelineFactoryTestResult =
                    await _metadataApplicationPipelineFactoryTestRunner
                        .RunAsync();

            AppendLog(
                "=== Composición predeterminada del pipeline ===");

            foreach (string message
                in pipelineFactoryTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Resultado general: " +
                $"{ToSpanish(
                    pipelineFactoryTestResult
                        .WasSuccessful)}");

            AppendLog(
                pipelineFactoryTestResult.Summary);

            AppendLog(
                "=== Fin de las pruebas estructurales de " +
                "composición ===");

            await RunMetadataApplicationPipelineDiagnosticAsync(
                filePath);

            string extension =
                Path.GetExtension(filePath)
                    .ToLowerInvariant();

            switch (extension)
            {
                case ".mp3":
                    await RunMp3DiagnosticAsync(
                        filePath);
                    break;

                case ".flac":
                    await RunFlacDiagnosticAsync(
                        filePath);
                    break;

                default:
                    AppendLog(
                        "El análisis técnico general terminó. " +
                        $"Todavía no existe una prueba aislada de " +
                        $"escritura para el formato {extension}.");
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog(
                "El diagnóstico o la prueba aislada fueron " +
                "cancelados.");
        }
        catch (Exception exception)
        {
            AppendLog(
                $"No fue posible ejecutar el diagnóstico. " +
                $"Detalle: {exception.Message}");
        }
        finally
        {
            SetAudioAnalysisControlsEnabled(
                true);
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
            string filePath)
    {
        AppendLog(
            "Iniciando prueba integral aislada del pipeline.");

        MetadataApplicationPipelineIsolatedTestRunner testRunner =
            new();

        MetadataApplicationPipelineIsolatedTestResult testResult =
            await testRunner.RunAsync(
                filePath,
                requestedGenre:
                    "Electronic");

        AppendLog(
            "=== Pipeline integral aislado ===");

        AppendLog(
            $"Entorno preparado: " +
            $"{ToSpanish(testResult.EnvironmentWasPrepared)}");

        AppendLog(
            $"Etapas registradas: " +
            $"{testResult.RegisteredStageCount}");

        AppendLog(
            $"Etapas ejecutadas: " +
            $"{testResult.ExecutedStageCount}");

        AppendLog(
            $"Ejecución del pipeline correcta: " +
            $"{ToSpanish(
                testResult.PipelineExecutionWasSuccessful)}");

        AppendLog(
            $"Respaldo del pipeline correcto: " +
            $"{ToSpanish(
                testResult
                    .PipelineBackupWasSuccessfulBeforeCleanup)}");

        AppendLog(
            $"Escritura correcta: " +
            $"{ToSpanish(testResult.WriteWasSuccessful)}");

        AppendLog(
            $"Verificación posterior correcta: " +
            $"{ToSpanish(
                testResult.VerificationWasSuccessful)}");

        AppendLog(
            $"Género verificado: " +
            $"{ToSpanish(
                testResult.GenreVerificationWasSuccessful)}");

        AppendLog(
            $"Género solicitado: " +
            $"{DisplayDiagnosticValue(
                testResult.RequestedGenre)}");

        AppendLog(
            $"Género persistido: " +
            $"{DisplayDiagnosticValue(
                testResult.PersistedGenre)}");

        AppendLog(
            $"Imágenes antes: " +
            $"{testResult.PictureCountBefore}");

        AppendLog(
            $"Imágenes después: " +
            $"{testResult.PictureCountAfter}");

        AppendLog(
            $"Limpieza ejecutada: " +
            $"{ToSpanish(testResult.CleanupWasAttempted)}");

        AppendLog(
            $"Carpeta temporal eliminada: " +
            $"{ToSpanish(testResult.TestDirectoryWasRemoved)}");

        foreach (string message in testResult.Messages)
        {
            AppendLog(
                $"- {message}");
        }

        if (!string.IsNullOrWhiteSpace(
                testResult.ErrorMessage))
        {
            AppendLog(
                $"Error: {testResult.ErrorMessage}");
        }

        AppendLog(
            $"Prueba integral correcta: " +
            $"{ToSpanish(testResult.WasSuccessful)}");

        AppendLog(
            $"Resumen: {testResult.Summary}");

        AppendLog(
            "=== Fin del pipeline integral aislado ===");
    }

    /// <summary>
    /// Ejecuta la inspección de sólo lectura y la prueba aislada
    /// de escritura para un archivo MP3.
    ///
    /// El archivo original nunca se modifica.
    /// </summary>
    private async Task RunMp3DiagnosticAsync(
        string filePath)
    {
        AppendLog(
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

        LogTextBox.AppendText(
            Environment.NewLine +
            inspectionReport +
            Environment.NewLine);

        LogTextBox.ScrollToEnd();

        if (!inspectionResult.WasSuccessful)
        {
            AppendLog(
                "La inspección MP3 no terminó correctamente. " +
                "La prueba aislada no se ejecutará.");

            return;
        }

        AppendLog(
            "Iniciando prueba aislada de escritura MP3. " +
            "El archivo original permanecerá intacto.");

        TagLibMp3IsolatedWriteTestRunner isolatedTestRunner =
            new();

        TagLibMp3IsolatedWriteTestResult isolatedTestResult =
            await isolatedTestRunner.RunAsync(
                filePath,
                requestedGenre:
                    "Electronic");

        string isolatedTestReport =
            TagLibMp3IsolatedWriteTestDiagnostics.BuildReport(
                isolatedTestResult);

        LogTextBox.AppendText(
            Environment.NewLine +
            isolatedTestReport +
            Environment.NewLine);

        LogTextBox.ScrollToEnd();

        if (isolatedTestResult.WasSuccessful)
        {
            AppendLog(
                "La prueba aislada MP3 terminó correctamente. " +
                "El género fue guardado en la copia temporal, " +
                "la carátula fue preservada y el archivo original " +
                "permaneció intacto.");

            AppendLog(
                $"Carpeta de la prueba MP3: " +
                $"{isolatedTestResult.TestDirectoryPath}");

            return;
        }

        AppendLog(
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
        string filePath)
    {
        AppendLog(
            "Iniciando prueba aislada de escritura FLAC. " +
            "El archivo original permanecerá intacto.");

        TagLibFlacIsolatedWriteTestRunner isolatedTestRunner =
            new();

        TagLibIsolatedWriteTestResult isolatedTestResult =
            await isolatedTestRunner.RunAsync(
                filePath,
                requestedGenre:
                    "Electronic");

        AppendLog(
            "=== Prueba aislada de escritura FLAC ===");

        AppendLog(
            $"Archivo original: " +
            $"{isolatedTestResult.OriginalFilePath}");

        AppendLog(
            $"Copia de trabajo: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.WorkingCopyPath)}");

        AppendLog(
            $"Respaldo de la copia: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.WorkingBackupPath)}");

        AppendLog(
            $"Género original: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.OriginalGenre)}");

        AppendLog(
            $"Género solicitado: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.RequestedGenre)}");

        AppendLog(
            $"Género persistido: " +
            $"{DisplayDiagnosticValue(
                isolatedTestResult.PersistedGenre)}");

        AppendLog(
            $"Original intacto: " +
            $"{ToSpanish(
                isolatedTestResult
                    .OriginalFileRemainedUnchanged)}");

        AppendLog(
            $"Respaldo coincide con la copia inicial: " +
            $"{ToSpanish(
                isolatedTestResult
                    .BackupMatchesInitialWorkingCopy)}");

        AppendLog(
            $"Copia modificada realmente: " +
            $"{ToSpanish(
                isolatedTestResult
                    .WorkingCopyWasModified)}");

        AppendLog(
            $"Género verificado: " +
            $"{ToSpanish(
                isolatedTestResult.GenreWasPersisted)}");

        AppendLog(
            $"Imágenes antes: " +
            $"{isolatedTestResult.PictureCountBefore}");

        AppendLog(
            $"Imágenes después: " +
            $"{isolatedTestResult.PictureCountAfter}");

        AppendLog(
            $"Carátulas preservadas: " +
            $"{ToSpanish(
                isolatedTestResult.PicturesWerePreserved)}");

        if (isolatedTestResult.WriteResult is not null)
        {
            AppendLog(
                $"Escritor utilizado: " +
                $"{isolatedTestResult.WriteResult.WriterName}");

            AppendLog(
                $"Estado de escritura: " +
                $"{isolatedTestResult.WriteResult.Status}");

            AppendLog(
                $"Campos escritos: " +
                $"{isolatedTestResult.WriteResult.WrittenFieldCount}");

            AppendLog(
                $"Campos fallidos: " +
                $"{isolatedTestResult.WriteResult.FailedFieldCount}");

            foreach (string message
                in isolatedTestResult.WriteResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }
        }

        foreach (string message
            in isolatedTestResult.Messages)
        {
            AppendLog(
                $"- {message}");
        }

        AppendLog(
            $"Prueba FLAC correcta: " +
            $"{ToSpanish(
                isolatedTestResult.WasSuccessful)}");

        AppendLog(
            $"Resumen: {isolatedTestResult.Summary}");

        AppendLog(
            "=== Fin de la prueba aislada FLAC ===");

        if (isolatedTestResult.WasSuccessful)
        {
            AppendLog(
                "La prueba aislada FLAC terminó correctamente. " +
                "El género fue guardado en la copia temporal, " +
                "las imágenes fueron preservadas y el archivo " +
                "original permaneció intacto.");

            AppendLog(
                $"Carpeta de la prueba FLAC: " +
                $"{isolatedTestResult.TestDirectoryPath}");

            return;
        }

        AppendLog(
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
    /// Obtiene el archivo seleccionado en la tabla.
    /// </summary>
    private AudioFile? GetSelectedAudioFile()
    {
        return AudioFilesDataGrid.SelectedItem
            as AudioFile;
    }

    /// <summary>
    /// Obtiene y comprueba la ruta del archivo seleccionado.
    /// </summary>
    private bool TryGetValidFilePath(
        AudioFile audioFile,
        out string filePath)
    {
        filePath =
            audioFile.FullPath?.Trim() ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            AppendLog(
                "El archivo seleccionado no contiene " +
                "una ruta válida.");

            return false;
        }

        if (!Path.IsPathFullyQualified(filePath))
        {
            AppendLog(
                "La ruta del archivo seleccionado " +
                "no es una ruta completa.");

            return false;
        }

        if (!File.Exists(filePath))
        {
            AppendLog(
                $"No se encontró el archivo: {filePath}");

            return false;
        }

        return true;
    }

    /// <summary>
    /// Activa o desactiva los controles mientras se ejecuta
    /// el análisis.
    /// </summary>
    private void SetAudioAnalysisControlsEnabled(
        bool isEnabled)
    {
        BrowseButton.IsEnabled =
            isEnabled;

        ScanButton.IsEnabled =
            isEnabled &&
            !string.IsNullOrWhiteSpace(
                LibraryPathTextBox.Text);

        AudioFilesDataGrid.IsEnabled =
            isEnabled;

        AnalyzeSelectedFileButton.IsEnabled =
            isEnabled &&
            AudioFilesDataGrid.SelectedItem is AudioFile;

        RunAudioDiagnosticButton.IsEnabled =
            isEnabled &&
            AudioFilesDataGrid.SelectedItem is AudioFile;
    }

    /// <summary>
    /// Actualiza los botones asociados a la fila seleccionada.
    /// </summary>
    private void UpdateSelectedFileButtons()
    {
        bool hasSelectedFile =
            AudioFilesDataGrid.SelectedItem
            is AudioFile;

        AnalyzeSelectedFileButton.IsEnabled =
            hasSelectedFile;

        RunAudioDiagnosticButton.IsEnabled =
            hasSelectedFile;
    }

    /// <summary>
    /// Agrega una línea al registro de actividad.
    /// </summary>
    private void AppendLog(
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        LogTextBox.AppendText(
            Environment.NewLine +
            message.Trim());

        LogTextBox.ScrollToEnd();
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