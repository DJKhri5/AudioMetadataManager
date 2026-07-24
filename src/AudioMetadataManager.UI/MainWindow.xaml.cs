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
using AudioMetadataManager.UI.Services.Parsing;
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

    private SimulationPlanViewModel?
        _currentSimulationPlan;

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

        _audioAnalysisEngine =
            new AudioAnalysisEngine();

        _audioAnalysisTestRunner =
            new AudioAnalysisTestRunner(
                _audioAnalysisEngine);
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
    /// Ejecuta el diagnóstico completo y muestra el informe
    /// generado por AudioAnalysisReportBuilder.
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