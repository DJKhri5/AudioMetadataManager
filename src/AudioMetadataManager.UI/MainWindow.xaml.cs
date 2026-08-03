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
using AudioMetadataManager.UI.Services.Simulation.Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Integration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;
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
using System.ComponentModel;
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

    private readonly MetadataApplyResultBuilderTestRunner
        _metadataApplyResultBuilderTestRunner;

    private readonly MetadataApplicationCoordinatorTestRunner
        _metadataApplicationCoordinatorTestRunner;

    private readonly
        MetadataApplyRequestIsolationFactoryTestRunner
            _metadataApplyRequestIsolationFactoryTestRunner;

    private readonly
        MetadataApplicationIsolatedExecutorTestRunner
            _metadataApplicationIsolatedExecutorTestRunner;

    private readonly
        MetadataApplicationPreservedExecutionTestRunner
            _metadataApplicationPreservedExecutionTestRunner;

    private readonly
        MetadataApplicationPromotionTestRunner
            _metadataApplicationPromotionTestRunner;

    private readonly
        MetadataApplicationRollbackTestRunner
            _metadataApplicationRollbackTestRunner;

    private readonly
        MetadataProductiveApplicationCoordinatorTestRunner
            _metadataProductiveApplicationCoordinatorTestRunner;

    private readonly
        MetadataProductiveApplicationApprovedTestRunner
            _metadataProductiveApplicationApprovedTestRunner;

    private readonly MetadataApplicationIsolatedExecutor
            _metadataApplicationIsolatedExecutor;

    private SimulationPlanViewModel?
        _currentSimulationPlan;

    /// <summary>
    /// Sustituye el plan activo y mantiene sincronizado el estado
    /// de las acciones productivas de la interfaz.
    /// </summary>
    private void SetCurrentSimulationPlan(
        SimulationPlanViewModel? simulationPlan)
    {
        if (_currentSimulationPlan is not null)
        {
            _currentSimulationPlan.PropertyChanged -=
                CurrentSimulationPlan_PropertyChanged;
        }

        _currentSimulationPlan =
            simulationPlan;

        if (_currentSimulationPlan is not null)
        {
            _currentSimulationPlan.PropertyChanged +=
                CurrentSimulationPlan_PropertyChanged;
        }

        AudioFileDetailsViewControl.SimulationPlan =
            _currentSimulationPlan;

        UpdateApplyChangesButtonState();
    }

    /// <summary>
    /// Reacciona a cambios en las propuestas aprobadas del plan.
    /// </summary>
    private void CurrentSimulationPlan_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(
                e.PropertyName) ||
            string.Equals(
                e.PropertyName,
                nameof(
                    SimulationPlanViewModel
                        .HasApprovedChanges),
                StringComparison.Ordinal) ||
            string.Equals(
                e.PropertyName,
                nameof(
                    SimulationPlanViewModel
                        .ApprovedChangeCount),
                StringComparison.Ordinal))
        {
            UpdateApplyChangesButtonState();
        }
    }

    /// <summary>
    /// Habilita la aplicación productiva únicamente cuando existe
    /// un plan activo con al menos un cambio aprobado.
    /// </summary>
    private void UpdateApplyChangesButtonState()
    {
        ApplyChangesButton.IsEnabled =
            _currentSimulationPlan?.HasApprovedChanges ==
            true;
    }

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

        _metadataApplyResultBuilderTestRunner =
            MetadataApplyResultBuilderTestRunner
                .CreateDefault();

        _metadataApplicationCoordinatorTestRunner =
            new MetadataApplicationCoordinatorTestRunner();

        _metadataApplyRequestIsolationFactoryTestRunner =
            new MetadataApplyRequestIsolationFactoryTestRunner();

        _metadataApplicationIsolatedExecutorTestRunner =
            new MetadataApplicationIsolatedExecutorTestRunner();

        _metadataApplicationPreservedExecutionTestRunner =
            new MetadataApplicationPreservedExecutionTestRunner();

        _metadataApplicationPromotionTestRunner =
            new MetadataApplicationPromotionTestRunner();

        _metadataApplicationRollbackTestRunner =
            new MetadataApplicationRollbackTestRunner();

        _metadataProductiveApplicationCoordinatorTestRunner =
            new MetadataProductiveApplicationCoordinatorTestRunner();

        _metadataProductiveApplicationApprovedTestRunner =
            new MetadataProductiveApplicationApprovedTestRunner();

        _metadataApplicationIsolatedExecutor =
            new MetadataApplicationIsolatedExecutor();
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

        SetCurrentSimulationPlan(
            null);

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

        SetCurrentSimulationPlan(
            null);

        UpdateSelectedFileButtons();

        SaveProjectButton.IsEnabled =
            false;

        ExportButton.IsEnabled =
            false;

        LogTextBox.AppendText(
            $"{Environment.NewLine}" +
            $"Biblioteca seleccionada: " +
            $"{dialog.FolderName}");

        LogTextBox.ScrollToEnd();
    }

    /// <summary>
    /// Solicita confirmación explícita antes de iniciar una
    /// aplicación productiva de metadatos.
    ///
    /// Este incremento todavía no ejecuta el coordinador ni modifica
    /// archivos.
    /// </summary>
    /// <summary>
    /// Confirma los cambios aprobados y ejecuta el pipeline completo
    /// exclusivamente sobre una copia temporal aislada.
    ///
    /// El archivo original nunca se entrega al escritor.
    /// </summary>
    private async void ApplyChangesButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_currentSimulationPlan is null)
        {
            AppendLog(
                "No existe un plan de simulación activo.");

            UpdateApplyChangesButtonState();
            return;
        }

        if (!_currentSimulationPlan.HasApprovedChanges)
        {
            AppendLog(
                "No existen cambios aprobados para aplicar.");

            UpdateApplyChangesButtonState();
            return;
        }

        int approvedChangeCount =
            _currentSimulationPlan.ApprovedChangeCount;

        MessageBoxResult confirmation =
            MessageBox.Show(
                this,
                $"Se ejecutarán {approvedChangeCount} cambio(s) " +
                $"aprobado(s) sobre una copia temporal del archivo:" +
                $"\n\n{_currentSimulationPlan.FileName}\n\n" +
                "El archivo original permanecerá protegido y no " +
                "será entregado al escritor.\n\n" +
                "La copia temporal y su respaldo serán eliminados " +
                "después de finalizar las verificaciones.\n\n" +
                "¿Desea continuar?",
                "Confirmar ejecución aislada",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

        if (confirmation != MessageBoxResult.Yes)
        {
            AppendLog(
                "La ejecución aislada fue cancelada por el usuario.");

            return;
        }

        ApplyChangesButton.IsEnabled =
            false;

        AppendLog(
            $"Ejecución aislada confirmada para " +
            $"{approvedChangeCount} cambio(s) aprobado(s).");

        AppendLog(
            "Preparando una copia temporal protegida.");

        try
        {
            MetadataApplyRequestFactory requestFactory =
                new();

            MetadataApplyRequest request =
                requestFactory.Create(
                    _currentSimulationPlan);

            MetadataApplicationIsolatedExecutionResult result =
                await _metadataApplicationIsolatedExecutor
                    .ExecuteAsync(
                        request);

            AppendLog(
                "=== Aplicación aprobada sobre copia aislada ===");

            AppendLog(
                $"Entorno aislado preparado: " +
                $"{ToSpanish(
                    result.IsolationWasPrepared)}");

            AppendLog(
                $"Pipeline completado: " +
                $"{ToSpanish(
                    result.PipelineWasSuccessful)}");

            AppendLog(
                $"Archivo original intacto: " +
                $"{ToSpanish(
                    result.OriginalFileRemainedUnchanged)}");

            AppendLog(
                $"Copia temporal modificada: " +
                $"{ToSpanish(
                    result.WorkingCopyWasModified)}");

            AppendLog(
                $"Respaldo inicial preservado: " +
                $"{ToSpanish(
                    result.InitialBackupWasPreserved)}");

            AppendLog(
                $"Limpieza temporal correcta: " +
                $"{ToSpanish(
                    result.CleanupWasSuccessful)}");

            if (result.PipelineResult is not null)
            {
                AppendLog(
                    $"Estado del pipeline: " +
                    $"{result.PipelineResult.StopReason}");

                AppendLog(
                    $"Etapas correctas: " +
                    $"{result.PipelineResult
                        .SuccessfulStageCount}");

                AppendLog(
                    $"Campos aplicados en la copia: " +
                    $"{result.PipelineResult
                        .ApplyResult?
                        .SuccessfulFieldCount ?? 0}");
            }

            if (!string.IsNullOrWhiteSpace(
                    result.ErrorMessage))
            {
                AppendLog(
                    $"Error: {result.ErrorMessage}");
            }

            AppendLog(
                $"Ejecución aislada correcta: " +
                $"{ToSpanish(
                    result.WasSuccessful)}");

            AppendLog(
                $"Resumen: {result.Summary}");

            AppendLog(
                "=== Fin de la aplicación aislada ===");

            if (result.WasSuccessful)
            {
                MessageBox.Show(
                    this,
                    "La ejecución sobre la copia temporal terminó " +
                    "correctamente.\n\n" +
                    "El archivo original permaneció intacto.\n\n" +
                    "Todavía no se han aplicado cambios al archivo " +
                    "original.",
                    "Ejecución aislada completada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    this,
                    "La ejecución aislada no superó todas las " +
                    "comprobaciones.\n\n" +
                    "Revise el Registro de actividad para obtener " +
                    "más información.\n\n" +
                    "El archivo original no debe considerarse " +
                    "modificado por esta operación.",
                    "Ejecución aislada incompleta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception exception)
        {
            AppendLog(
                "No fue posible completar la ejecución aislada. " +
                $"Detalle: {exception.Message}");

            MessageBox.Show(
                this,
                "No fue posible completar la ejecución aislada.\n\n" +
                "Revise el Registro de actividad para obtener más " +
                "información.",
                "Error de ejecución aislada",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            UpdateApplyChangesButtonState();
        }
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

            SetCurrentSimulationPlan(
                simulationPlanFactory.Create(
                    changePlan));

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

            AppendLog(
                "Iniciando prueba del constructor del " +
                "resultado final.");

            MetadataApplyResultBuilderTestResult
                resultBuilderTestResult =
                    await Task.Run(
                        () =>
                            _metadataApplyResultBuilderTestRunner
                                .Run());

            AppendLog(
                "=== Constructor del resultado final ===");

            AppendLog(
                $"Identificadores conservados: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .IdentifiersPreserved)}");

            AppendLog(
                $"Información del archivo conservada: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .FileInformationPreserved)}");

            AppendLog(
                $"Ruta de respaldo conservada: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .BackupPathPreserved)}");

            AppendLog(
                $"Cantidad de campos conservada: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .FieldCountPreserved)}");

            AppendLog(
                $"Valores de los campos conservados: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .FieldValuesPreserved)}");

            AppendLog(
                $"Estado de escritura conservado: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .WriteStatusPreserved)}");

            AppendLog(
                $"Estado de verificación conservado: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .VerificationStatusPreserved)}");

            AppendLog(
                $"Estado final correcto: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .FinalStatusCorrect)}");

            AppendLog(
                $"Mensajes consolidados: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .MessagesConsolidated)}");

            AppendLog(
                $"Mensajes duplicados eliminados: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .DuplicateMessagesRemoved)}");

            AppendLog(
                $"Tiempos coherentes: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .TimingIsValid)}");

            if (resultBuilderTestResult.ApplyResult is not null)
            {
                AppendLog(
                    $"Estado construido: " +
                    $"{resultBuilderTestResult
                        .ApplyResult.Status}");

                AppendLog(
                    $"Campos consolidados: " +
                    $"{resultBuilderTestResult
                        .ApplyResult.FieldResults.Count}");

                AppendLog(
                    $"Duración construida: " +
                    $"{resultBuilderTestResult
                        .ApplyResult.ElapsedTime}");
            }

            foreach (string message
                in resultBuilderTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            if (!string.IsNullOrWhiteSpace(
                    resultBuilderTestResult.ErrorMessage))
            {
                AppendLog(
                    $"Error: " +
                    $"{resultBuilderTestResult.ErrorMessage}");

                AppendLog(
                    $"Tipo de excepción: " +
                    $"{resultBuilderTestResult.ExceptionType}");
            }

            AppendLog(
                $"Prueba del constructor correcta: " +
                $"{ToSpanish(
                    resultBuilderTestResult
                        .WasSuccessful)}");

            AppendLog(
                $"Resumen: " +
                $"{resultBuilderTestResult.Summary}");

            AppendLog(
                "=== Fin de la prueba del constructor ===");

            AppendLog(
                "Iniciando pruebas controladas del coordinador " +
                "productivo.");

            MetadataApplicationCoordinatorTestResult
                coordinatorTestResult =
                    await _metadataApplicationCoordinatorTestRunner
                        .RunAsync();

            AppendLog(
                "=== Coordinador productivo de aplicación ===");

            foreach (string message
                in coordinatorTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Solicitud nula rechazada: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .NullRequestWasRejected)}");

            AppendLog(
                $"Fábrica nula rechazada: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .NullExecutorFactoryWasRejected)}");

            AppendLog(
                $"Cancelación previa controlada: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .PreCancelledExecutionWasHandled)}");

            AppendLog(
                $"Razón de cancelación correcta: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .CancellationStopReasonWasCorrect)}");

            AppendLog(
                $"Ejecutor nulo controlado: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .NullExecutorWasHandled)}");

            AppendLog(
                $"Razón del ejecutor nulo correcta: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .NullExecutorStopReasonWasCorrect)}");

            AppendLog(
                $"Excepción de fábrica controlada: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .FactoryExceptionWasHandled)}");

            AppendLog(
                $"Razón de excepción correcta: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .FactoryExceptionStopReasonWasCorrect)}");

            AppendLog(
                $"Resultados finalizados: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .ResultsWereFinalized)}");

            AppendLog(
                $"Prueba del coordinador correcta: " +
                $"{ToSpanish(
                    coordinatorTestResult
                        .WasSuccessful)}");

            AppendLog(
                $"Resumen: {coordinatorTestResult.Summary}");

            AppendLog(
                "=== Fin de las pruebas del coordinador ===");

            AppendLog(
                "Iniciando prueba de la fábrica de solicitudes " +
                "aisladas.");

            MetadataApplyRequestIsolationFactoryTestResult
                isolationFactoryTestResult =
                    _metadataApplyRequestIsolationFactoryTestRunner
                        .Run();

            AppendLog(
                "=== Fábrica de solicitudes aisladas ===");

            foreach (string message
                in isolationFactoryTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Solicitud nula rechazada: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .NullRequestWasRejected)}");

            AppendLog(
                $"Ruta vacía rechazada: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .EmptyPathWasRejected)}");

            AppendLog(
                $"Identificadores conservados: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .IdentifiersWerePreserved)}");

            AppendLog(
                $"Fecha de creación conservada: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .CreationTimeWasPreserved)}");

            AppendLog(
                $"Cambios conservados: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .ChangesWerePreserved)}");

            AppendLog(
                $"Requisitos conservados: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .RequirementsWerePreserved)}");

            AppendLog(
                $"Ruta aislada aplicada: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .WorkingCopyPathWasApplied)}");

            AppendLog(
                $"Nombre aislado aplicado: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .WorkingCopyFileNameWasApplied)}");

            AppendLog(
                $"Prueba de la fábrica aislada correcta: " +
                $"{ToSpanish(
                    isolationFactoryTestResult
                        .WasSuccessful)}");

            AppendLog(
                $"Resumen: {isolationFactoryTestResult.Summary}");

            AppendLog(
                "=== Fin de la prueba de solicitudes aisladas ===");

            AppendLog(
                "Iniciando prueba integral del ejecutor aislado. " +
                "El archivo original permanecerá protegido.");

            MetadataApplicationIsolatedExecutorTestResult
                isolatedExecutorTestResult =
                    await _metadataApplicationIsolatedExecutorTestRunner
                        .RunAsync(
                            filePath);

            AppendLog(
                "=== Ejecutor coordinado sobre copia aislada ===");

            foreach (string message
                in isolatedExecutorTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Entorno aislado preparado: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .IsolationWasPrepared)}");

            AppendLog(
                $"Pipeline coordinado correcto: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .PipelineWasSuccessful)}");

            AppendLog(
                $"Archivo original intacto: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .OriginalFileRemainedUnchanged)}");

            AppendLog(
                $"Copia temporal modificada: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .WorkingCopyWasModified)}");

            AppendLog(
                $"Respaldo inicial preservado: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .InitialBackupWasPreserved)}");

            AppendLog(
                $"Limpieza temporal correcta: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .CleanupWasSuccessful)}");

            AppendLog(
                $"Género solicitado: " +
                $"{isolatedExecutorTestResult.RequestedGenre}");

            AppendLog(
                $"Género persistido: " +
                $"{isolatedExecutorTestResult.PersistedGenre}");

            AppendLog(
                $"Género verificado: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .GenreWasPersisted)}");

            if (!string.IsNullOrWhiteSpace(
                    isolatedExecutorTestResult.ErrorMessage))
            {
                AppendLog(
                    $"Error del ejecutor aislado: " +
                    $"{isolatedExecutorTestResult.ErrorMessage}");
            }

            AppendLog(
                $"Prueba del ejecutor aislado correcta: " +
                $"{ToSpanish(
                    isolatedExecutorTestResult
                        .WasSuccessful)}");

            AppendLog(
                $"Resumen: " +
                $"{isolatedExecutorTestResult.Summary}");

            AppendLog(
                "=== Fin de la prueba del ejecutor aislado ===");

            AppendLog(
                "Iniciando prueba de conservación controlada de una " +
                "copia verificada.");

            MetadataApplicationPreservedExecutionTestResult
                preservedExecutionTestResult =
                    await _metadataApplicationPreservedExecutionTestRunner
                        .RunAsync(
                            filePath);

            AppendLog(
                "=== Conservación controlada de copia verificada ===");

            foreach (string message
                in preservedExecutionTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Ejecución correcta: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .ExecutionWasSuccessful)}");

            AppendLog(
                $"Entorno conservado: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .EnvironmentWasPreserved)}");

            AppendLog(
                $"Limpieza automática pospuesta: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .CleanupWasDeferred)}");

            AppendLog(
                $"Copia verificada disponible: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .WorkingCopyStillExisted)}");

            AppendLog(
                $"Respaldo inicial disponible: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .InitialBackupStillExisted)}");

            AppendLog(
                $"Archivo original intacto: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .OriginalFileRemainedUnchanged)}");

            AppendLog(
                $"Copia conservada modificada: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .WorkingCopyWasModified)}");

            AppendLog(
                $"Limpieza manual correcta: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .ManualCleanupWasSuccessful)}");

            AppendLog(
                $"Carpeta temporal eliminada: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .TemporaryDirectoryWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    preservedExecutionTestResult.ErrorMessage))
            {
                AppendLog(
                    $"Error de conservación controlada: " +
                    $"{preservedExecutionTestResult.ErrorMessage}");
            }

            AppendLog(
                $"Prueba de conservación correcta: " +
                $"{ToSpanish(
                    preservedExecutionTestResult
                        .WasSuccessful)}");

            AppendLog(
                $"Resumen: " +
                $"{preservedExecutionTestResult.Summary}");

            AppendLog(
                "=== Fin de la prueba de conservación controlada ===");

            AppendLog(
                "Iniciando prueba de promoción controlada sobre archivos " +
                "temporales. El archivo seleccionado no será modificado.");

            MetadataApplicationPromotionTestResult
                promotionTestResult =
                    await _metadataApplicationPromotionTestRunner
                        .RunAsync(
                            filePath);

            AppendLog(
                "=== Promoción controlada sobre destino temporal ===");

            foreach (string message
                in promotionTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Entorno temporal preparado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .TestEnvironmentWasPrepared)}");

            AppendLog(
                $"Entradas validadas: " +
                $"{ToSpanish(
                    promotionTestResult
                        .InputsWereValidated)}");

            AppendLog(
                $"Respaldo productivo creado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .ProductiveBackupWasCreated)}");

            AppendLog(
                $"Respaldo productivo verificado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .ProductiveBackupWasVerified)}");

            AppendLog(
                $"Sustitución ejecutada: " +
                $"{ToSpanish(
                    promotionTestResult
                        .ReplacementWasExecuted)}");

            AppendLog(
                $"Destino promovido verificado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .PromotedFileWasVerified)}");

            AppendLog(
                $"Original de referencia intacto: " +
                $"{ToSpanish(
                    promotionTestResult
                        .ReferenceOriginalRemainedUnchanged)}");

            AppendLog(
                $"Copia verificada preservada: " +
                $"{ToSpanish(
                    promotionTestResult
                        .VerifiedCopyWasPreserved)}");

            AppendLog(
                $"Reversión no requerida: " +
                $"{ToSpanish(
                    promotionTestResult
                        .RollbackWasNotRequired)}");

            AppendLog(
                $"Entorno temporal eliminado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .TestEnvironmentWasRemoved)}");

            AppendLog(
                $"Respaldo temporal eliminado: " +
                $"{ToSpanish(
                    promotionTestResult
                        .TemporaryBackupWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    promotionTestResult.ErrorMessage))
            {
                AppendLog(
                    $"Error de promoción controlada: " +
                    $"{promotionTestResult.ErrorMessage}");
            }

            AppendLog(
                $"Prueba de promoción correcta: " +
                $"{ToSpanish(
                    promotionTestResult
                        .WasSuccessful)}");

            AppendLog(
                $"Resumen: {promotionTestResult.Summary}");

            AppendLog(
                "=== Fin de la prueba de promoción controlada ===");

            AppendLog(
                "Iniciando prueba de reversión automática sobre archivos " +
                "temporales. El archivo seleccionado no será modificado.");

            MetadataApplicationRollbackTestResult
                rollbackTestResult =
                    await _metadataApplicationRollbackTestRunner
                        .RunAsync(
                            filePath);

            AppendLog(
                "=== Reversión automática sobre destino temporal ===");

            foreach (string message
                in rollbackTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Entorno temporal preparado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .TestEnvironmentWasPrepared)}");

            AppendLog(
                $"Entradas validadas: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .InputsWereValidated)}");

            AppendLog(
                $"Respaldo productivo creado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .ProductiveBackupWasCreated)}");

            AppendLog(
                $"Respaldo productivo verificado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .ProductiveBackupWasVerified)}");

            AppendLog(
                $"Sustitución temporal ejecutada: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .ReplacementWasExecuted)}");

            AppendLog(
                $"Fallo de verificación simulado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .VerificationFailureWasSimulated)}");

            AppendLog(
                $"Reversión iniciada: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .RollbackWasAttempted)}");

            AppendLog(
                $"Reversión correcta: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .RollbackWasSuccessful)}");

            AppendLog(
                $"Destino restaurado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .DestinationWasRestored)}");

            AppendLog(
                $"Original de referencia intacto: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .ReferenceOriginalRemainedUnchanged)}");

            AppendLog(
                $"Copia verificada preservada: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .VerifiedCopyWasPreserved)}");

            AppendLog(
                $"Destino en estado seguro: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .DestinationEndedInSafeState)}");

            AppendLog(
                $"Entorno temporal eliminado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .TestEnvironmentWasRemoved)}");

            AppendLog(
                $"Respaldo temporal eliminado: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .TemporaryBackupWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    rollbackTestResult.ExpectedErrorMessage))
            {
                AppendLog(
                    $"Error esperado de la simulación: " +
                    $"{rollbackTestResult.ExpectedErrorMessage}");
            }

            AppendLog(
                $"Prueba de reversión correcta: " +
                $"{ToSpanish(
                    rollbackTestResult
                        .WasSuccessful)}");

            AppendLog(
                $"Resumen: {rollbackTestResult.Summary}");

            AppendLog(
                "=== Fin de la prueba de reversión automática ===");

            AppendLog(
                "Iniciando prueba controlada del coordinador productivo " +
                "individual. El archivo seleccionado no será modificado.");

            MetadataProductiveApplicationCoordinatorTestResult
                productiveCoordinatorTestResult =
                    await _metadataProductiveApplicationCoordinatorTestRunner
                        .RunAsync(
                            filePath);

            AppendLog(
                "=== Coordinador productivo individual temporal ===");

            foreach (string message
                in productiveCoordinatorTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Solicitud nula rechazada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .NullRequestWasRejected)}");

            AppendLog(
                $"Copia verificada preparada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .VerifiedCopyWasPrepared)}");

            AppendLog(
                $"Decisión de promoción pendiente: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .PromotionDecisionWasPending)}");

            AppendLog(
                $"Destino intacto durante preparación: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .OriginalRemainedUnchangedDuringPreparation)}");

            AppendLog(
                $"Decisión Declined procesada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedDecisionWasHandled)}");

            AppendLog(
                $"Promoción omitida tras rechazo: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedDecisionSkippedPromotion)}");

            AppendLog(
                $"Destino seguro después del rechazo: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedOriginalEndedInSafeState)}");

            AppendLog(
                $"Entorno aislado eliminado: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedEnvironmentWasCleaned)}");

            AppendLog(
                $"Rechazo finalizado correctamente: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .DeclinedResultWasSuccessful)}");

            AppendLog(
                $"Decisión inválida rechazada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .InvalidDecisionWasRejected)}");

            AppendLog(
                $"Reutilización de preparación rechazada: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .ReusedPreparationWasRejected)}");

            AppendLog(
                $"Entorno temporal general eliminado: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .TemporaryEnvironmentWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    productiveCoordinatorTestResult.ErrorMessage))
            {
                AppendLog(
                    $"Error del coordinador productivo: " +
                    $"{productiveCoordinatorTestResult.ErrorMessage}");
            }

            AppendLog(
                $"Prueba del coordinador productivo correcta: " +
                $"{ToSpanish(
                    productiveCoordinatorTestResult
                        .WasSuccessful)}");

            AppendLog(
                $"Resumen: " +
                $"{productiveCoordinatorTestResult.Summary}");

            AppendLog(
                "=== Fin de la prueba del coordinador productivo ===");

            AppendLog(
                "Iniciando prueba controlada del camino Approved del " +
                "coordinador productivo. El archivo seleccionado no será " +
                "modificado.");

            MetadataProductiveApplicationApprovedTestResult
                productiveApprovedTestResult =
                    await _metadataProductiveApplicationApprovedTestRunner
                        .RunAsync(
                            filePath);

            AppendLog(
                "=== Camino Approved del coordinador productivo ===");

            foreach (string message
                in productiveApprovedTestResult.Messages)
            {
                AppendLog(
                    $"- {message}");
            }

            AppendLog(
                $"Entorno temporal preparado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .TestEnvironmentWasPrepared)}");

            AppendLog(
                $"Copia verificada preparada: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .VerifiedCopyWasPrepared)}");

            AppendLog(
                $"Decisión de promoción pendiente: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .PromotionDecisionWasPending)}");

            AppendLog(
                $"Destino intacto durante preparación: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .DestinationRemainedUnchangedDuringPreparation)}");

            AppendLog(
                $"Decisión Approved procesada: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ApprovedDecisionWasHandled)}");

            AppendLog(
                $"Promoción correcta: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .PromotionWasSuccessful)}");

            AppendLog(
                $"Respaldo productivo creado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ProductiveBackupWasCreated)}");

            AppendLog(
                $"Respaldo productivo verificado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ProductiveBackupWasVerified)}");

            AppendLog(
                $"Sustitución ejecutada: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ReplacementWasExecuted)}");

            AppendLog(
                $"Destino promovido verificado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .PromotedDestinationWasVerified)}");

            AppendLog(
                $"Género solicitado persistido: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .RequestedGenreWasPersisted)}");

            AppendLog(
                $"Género solicitado: " +
                $"{productiveApprovedTestResult.RequestedGenre}");

            AppendLog(
                $"Género persistido: " +
                $"{productiveApprovedTestResult.PersistedGenre}");

            AppendLog(
                $"Reversión no requerida: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .RollbackWasNotRequired)}");

            AppendLog(
                $"Original de referencia intacto: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ReferenceOriginalRemainedUnchanged)}");

            AppendLog(
                $"Destino en estado seguro: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .DestinationEndedInSafeState)}");

            AppendLog(
                $"Limpieza final intentada: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .FinalCleanupWasAttempted)}");

            AppendLog(
                $"Limpieza final correcta: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .FinalCleanupWasSuccessful)}");

            AppendLog(
                $"Resultado productivo correcto: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .ProductiveResultWasSuccessful)}");

            AppendLog(
                $"Entorno temporal general eliminado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .TemporaryEnvironmentWasRemoved)}");

            AppendLog(
                $"Respaldo productivo temporal eliminado: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .TemporaryProductiveBackupWasRemoved)}");

            if (!string.IsNullOrWhiteSpace(
                    productiveApprovedTestResult.ErrorMessage))
            {
                AppendLog(
                    $"Error del camino Approved: " +
                    $"{productiveApprovedTestResult.ErrorMessage}");
            }

            AppendLog(
                $"Prueba Approved correcta: " +
                $"{ToSpanish(
                    productiveApprovedTestResult
                        .WasSuccessful)}");

            AppendLog(
                $"Resumen: " +
                $"{productiveApprovedTestResult.Summary}");

            AppendLog(
                "=== Fin de la prueba Approved del coordinador productivo ===");

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