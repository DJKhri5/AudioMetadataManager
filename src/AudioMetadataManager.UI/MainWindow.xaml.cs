using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services;
using AudioMetadataManager.UI.Services.AudioAnalysis;
using AudioMetadataManager.UI.Services.AudioAnalysis.Diagnostics;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;
using AudioMetadataManager.UI.Services.Diagnostics;
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
using AudioMetadataManager.UI.Services.Simulation.Application.BatchWorkflow;
using AudioMetadataManager.UI.Services.Simulation.Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Mapping;
using AudioMetadataManager.UI.Services.Simulation.Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Diagnostics;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Preparation;
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

    private readonly AudioDiagnosticsOrchestrator
        _audioDiagnosticsOrchestrator;

    private readonly MetadataApplicationIsolatedExecutor
            _metadataApplicationIsolatedExecutor;

    private readonly MetadataProductiveApplicationCoordinator
            _metadataProductiveApplicationCoordinator;

    private readonly ProductiveBatchWorkflowService
            _productiveBatchWorkflowService;

    private readonly ProductiveBatchSelection
            _productiveBatchSelection =
                new();

    private bool
    _isProductiveBatchOperationInProgress;

    private SimulationPlanViewModel?
        _currentSimulationPlan;

    /// <summary>
    /// Define el origen de una solicitud de análisis individual.
    /// </summary>
    private enum FileAnalysisInvocation
    {
        /// <summary>
        /// El análisis fue solicitado explícitamente por el usuario.
        /// </summary>
        UserRequested,

        /// <summary>
        /// El análisis se ejecuta automáticamente después de aplicar cambios.
        /// </summary>
        PostApplicationRefresh
    }

    /// <summary>
    /// Sustituye el plan activo y mantiene sincronizado el estado
    /// de las acciones productivas de la interfaz.
    /// </summary>
    private void SetCurrentSimulationPlan(
        SimulationPlanViewModel? simulationPlan)
    {
        /*
         * Antes de abandonar el plan actual conservamos su estado
         * aprobado dentro de la selección productiva por lote.
         */
        SynchronizePlanWithProductiveBatchSelection(
            _currentSimulationPlan);

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

        /*
         * También sincronizamos el nuevo plan. Esto es importante
         * cuando un reanálisis elimina cambios que antes estaban
         * aprobados: la entrada batch anterior debe desaparecer.
         */
        SynchronizePlanWithProductiveBatchSelection(
            _currentSimulationPlan);

        AudioFileDetailsViewControl.SimulationPlan =
            _currentSimulationPlan;

        UpdateApplyChangesButtonState();
    }

    /// <summary>
    /// Sincroniza el plan visual indicado con la selección
    /// productiva persistente por lote.
    ///
    /// Un plan con cambios aprobados se agrega o reemplaza.
    /// Un plan sin cambios aprobados elimina cualquier selección
    /// anterior correspondiente al mismo archivo.
    /// </summary>
    private void SynchronizePlanWithProductiveBatchSelection(
        SimulationPlanViewModel? simulationPlan)
    {
        if (simulationPlan is null)
        {
            UpdateProductiveBatchUiState();
            return;
        }

        if (string.IsNullOrWhiteSpace(
                simulationPlan.FilePath))
        {
            UpdateProductiveBatchUiState();
            return;
        }

        _productiveBatchSelection.AddOrReplace(
            simulationPlan);

        UpdateProductiveBatchUiState();
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

            SynchronizePlanWithProductiveBatchSelection(
                _currentSimulationPlan);

            AppendLog(
                $"Selección productiva por lote: " +
                $"{_productiveBatchSelection.Summary}");
        }
    }

    /// <summary>
    /// Habilita la aplicación productiva individual únicamente
    /// cuando existe un plan activo con cambios aprobados y no
    /// existe una operación productiva batch en curso.
    /// </summary>
    private void UpdateApplyChangesButtonState()
    {
        ApplyChangesButton.IsEnabled =
            !_isProductiveBatchOperationInProgress &&
            _currentSimulationPlan?.HasApprovedChanges ==
                true;
    }

    /// <summary>
    /// Mantiene sincronizado el resumen y la disponibilidad
    /// de las acciones productivas por lote.
    /// </summary>
    private void UpdateProductiveBatchUiState()
    {
        bool hasBatchItems =
            _productiveBatchSelection.HasItems;

        ReviewProductiveBatchButton.IsEnabled =
            hasBatchItems &&
            !_isProductiveBatchOperationInProgress;

        ProductiveBatchSummaryTextBlock.Text =
            _isProductiveBatchOperationInProgress
                ? $"Lote productivo: operación en curso · " +
                  $"{_productiveBatchSelection.FileCount} archivo(s)"
                : hasBatchItems
                    ? $"Lote productivo: " +
                      $"{_productiveBatchSelection.FileCount} archivo(s) · " +
                      $"{_productiveBatchSelection.ApprovedChangeCount} cambio(s)"
                    : "Lote productivo: ningún archivo seleccionado";
    }

    /// <summary>
    /// Ejecuta el workflow productivo por lote.
    ///
    /// La ventana mantiene únicamente las responsabilidades de
    /// interacción con el usuario y actualización visual.
    /// La preparación, finalización y coordinación productiva se
    /// delegan al ProductiveBatchWorkflowService.
    /// </summary>
    private async void ReviewProductiveBatchButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_isProductiveBatchOperationInProgress)
        {
            AppendLog(
                "Ya existe una operación productiva por lote en curso.");

            return;
        }

        if (!_productiveBatchSelection.HasItems)
        {
            AppendLog(
                "No existen archivos seleccionados para aplicación " +
                "productiva por lote.");

            UpdateProductiveBatchUiState();

            return;
        }

        ProductiveBatchRequestMapper mapper =
            new();

        MetadataApplyBatchRequest batchRequest;

        try
        {
            batchRequest =
                mapper.Map(
                    _productiveBatchSelection);
        }
        catch (Exception exception)
        {
            AppendLog(
                "No fue posible construir la solicitud productiva " +
                $"por lote: {exception.Message}");

            return;
        }

        MessageBoxResult initialConfirmation =
            MessageBox.Show(
                $"Se prepararán " +
                $"{batchRequest.ValidRequestCount} archivo(s) " +
                $"con {batchRequest.ValidChangeCount} cambio(s) " +
                "aprobado(s).\n\n" +
                "Durante esta primera fase todavía no se modificarán " +
                "los archivos originales.\n\n" +
                "¿Deseas continuar con la preparación del lote?",
                "Preparar lote productivo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

        if (initialConfirmation !=
            MessageBoxResult.Yes)
        {
            AppendLog(
                "La preparación productiva por lote fue cancelada " +
                "por el usuario.");

            return;
        }

        _isProductiveBatchOperationInProgress =
            true;

        UpdateApplyChangesButtonState();
        UpdateProductiveBatchUiState();

        try
        {
            AppendLog(
                "Iniciando preparación productiva por lote.");

            AppendLog(
                $"Archivos del lote: " +
                $"{batchRequest.ValidRequestCount}.");

            AppendLog(
                $"Cambios aprobados del lote: " +
                $"{batchRequest.ValidChangeCount}.");

            ProductiveBatchPreparation preparation =
                await _productiveBatchWorkflowService
                    .PrepareAsync(
                        batchRequest);

            AppendLog(
                preparation.Summary);

            foreach (string message in
                     preparation.PreparationResult.Messages)
            {
                AppendLog(
                    message);
            }

            if (!preparation.IsReadyForDecision)
            {
                AppendLog(
                    "El lote no quedó preparado para una decisión " +
                    "productiva. No se promocionará ningún archivo.");

                return;
            }

            MessageBoxResult promotionConfirmation =
                MessageBox.Show(
                    $"La preparación del lote ha finalizado.\n\n" +
                    $"Archivos preparados: " +
                    $"{preparation.PreparationResult.RequestedCount}" +
                    "\n\n" +
                    "Todos los archivos han superado la fase de " +
                    "preparación y todavía permanecen aislados de " +
                    "los originales.\n\n" +
                    "¿Deseas aplicar definitivamente los cambios?",
                    "Aplicar lote productivo",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            MetadataPromotionDecision decision =
                promotionConfirmation ==
                    MessageBoxResult.Yes
                    ? MetadataPromotionDecision.Approved
                    : MetadataPromotionDecision.Declined;

            AppendLog(
                decision ==
                    MetadataPromotionDecision.Approved
                    ? "El usuario aprobó la promoción productiva " +
                      "del lote."
                    : "El usuario rechazó la promoción productiva " +
                      "del lote.");

            MetadataProductiveBatchCompletionResult
                completionResult =
                    await _productiveBatchWorkflowService
                        .CompleteAsync(
                            preparation,
                            decision);

            AppendLog(
                completionResult.Summary);

            foreach (string message in
                     completionResult.Messages)
            {
                AppendLog(
                    message);
            }

            if (decision ==
                    MetadataPromotionDecision.Approved &&
                completionResult.WasSuccessful)
            {
                AppendLog(
                    "La aplicación productiva por lote finalizó " +
                    "correctamente.");

                /*
                 * La preparación productiva ya fue consumida.
                 * No debe permanecer disponible para una segunda
                 * ejecución desde la interfaz.
                 */
                _productiveBatchSelection.Clear();

                /*
                 * Los archivos originales sí cambiaron.
                 * Invalidamos el plan activo para impedir que la UI
                 * siga mostrando metadatos anteriores a la promoción.
                 */
                SetCurrentSimulationPlan(
                    null);
            }
            else if (decision ==
                     MetadataPromotionDecision.Declined)
            {
                AppendLog(
                    "El lote fue descartado de forma segura. " +
                    "Los archivos originales no fueron modificados.");

                /*
                 * Aunque no hubo promoción, la preparación ya fue
                 * finalizada y limpiada por el coordinador two-phase.
                 * Por tanto tampoco puede reutilizarse.
                 */
                _productiveBatchSelection.Clear();
            }
            else
            {
                AppendLog(
                    "La aplicación productiva por lote no terminó " +
                    "completamente. Revisa el diagnóstico anterior.");

                /*
                 * Un resultado parcial también consume la preparación.
                 * El coordinador ya se responsabiliza de limpiar las
                 * preparaciones pendientes después del fallo.
                 *
                 * Mantener la selección aquí permitiría intentar volver
                 * a ejecutar solicitudes asociadas a una preparación
                 * que ya no existe.
                 */
                _productiveBatchSelection.Clear();

                /*
                 * No eliminamos automáticamente el plan visual.
                 * Puede existir un resultado parcial y necesitamos
                 * conservar el contexto visible para el usuario.
                 */
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog(
                "La operación productiva por lote fue cancelada.");
        }
        catch (Exception exception)
        {
            AppendLog(
                "Se produjo un error durante la aplicación " +
                $"productiva por lote: {exception.Message}");
        }
        finally
        {
            _isProductiveBatchOperationInProgress =
                false;

            UpdateApplyChangesButtonState();
            UpdateProductiveBatchUiState();
        }
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

        _audioDiagnosticsOrchestrator =
            new AudioDiagnosticsOrchestrator(
                new AudioAnalysisTestRunner(
                    _audioAnalysisEngine));

        _metadataApplicationIsolatedExecutor =
            new MetadataApplicationIsolatedExecutor();

        _metadataProductiveApplicationCoordinator =
            new MetadataProductiveApplicationCoordinator();

        _productiveBatchWorkflowService =
            new ProductiveBatchWorkflowService();

        UpdateProductiveBatchUiState();
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

        _productiveBatchSelection.Clear();

        UpdateProductiveBatchUiState();

        AppendLog(
            "La selección productiva por lote fue limpiada " +
            "al volver a escanear la biblioteca.");

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

        _productiveBatchSelection.Clear();

        UpdateProductiveBatchUiState();

        AppendLog(
            "La selección productiva por lote fue limpiada " +
            "al seleccionar una biblioteca diferente.");

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
    /// Vuelve a leer un archivo aplicado productivamente y actualiza
    /// solamente su fila, su selección y su panel de detalles.
    /// </summary>
    private bool RefreshAppliedAudioFile(
        string filePath)
    {
        AudioFile? refreshedAudioFile =
            _fileScannerService.ScanFile(
                filePath);

        if (refreshedAudioFile is null)
        {
            AppendLog(
                "No fue posible volver a leer el archivo aplicado.");

            return false;
        }

        if (AudioFilesDataGrid.ItemsSource is not
            List<AudioFile> audioFiles)
        {
            AppendLog(
                "La lista visible de archivos no está disponible.");

            return false;
        }

        int existingIndex =
            audioFiles.FindIndex(
                audioFile =>
                    string.Equals(
                        audioFile.FullPath,
                        refreshedAudioFile.FullPath,
                        StringComparison.OrdinalIgnoreCase));

        if (existingIndex < 0)
        {
            AppendLog(
                "El archivo aplicado no fue encontrado en la tabla.");

            return false;
        }

        audioFiles[existingIndex] =
            refreshedAudioFile;

        AudioFilesDataGrid.ItemsSource =
            null;

        AudioFilesDataGrid.ItemsSource =
            audioFiles;

        AudioFilesDataGrid.SelectedItem =
            null;

        AudioFilesDataGrid.SelectedItem =
            refreshedAudioFile;

        AudioFilesDataGrid.ScrollIntoView(
            refreshedAudioFile);

        AppendLog(
            "La fila aplicada fue actualizada desde el archivo real.");

        return true;
    }

    /// <summary>
    /// Vuelve a leer desde disco todos los archivos promovidos
    /// correctamente por una operación productiva batch.
    ///
    /// Las filas se actualizan en una sola operación para evitar
    /// cambiar repetidamente la selección del DataGrid.
    ///
    /// Si el archivo que estaba seleccionado fue promovido,
    /// conserva esa selección utilizando la instancia recién leída.
    /// </summary>
    private IReadOnlyList<string>
        RefreshSuccessfullyPromotedBatchFiles(
            MetadataApplyBatchRequest batchRequest,
            MetadataProductiveBatchCompletionResult completionResult)
    {
        ArgumentNullException.ThrowIfNull(
            batchRequest);

        ArgumentNullException.ThrowIfNull(
            completionResult);

        List<string> refreshedFilePaths =
            new();

        if (AudioFilesDataGrid.ItemsSource is not
            List<AudioFile> audioFiles)
        {
            AppendLog(
                "La lista visible de archivos no está disponible " +
                "para el refresco productivo por lote.");

            return refreshedFilePaths;
        }

        AudioFile? selectedAudioFile =
            GetSelectedAudioFile();

        string selectedFilePath =
            selectedAudioFile?.FullPath ??
            string.Empty;

        AudioFile? refreshedSelectedAudioFile =
            null;

        int comparableCount =
            Math.Min(
                batchRequest.ValidRequests.Count,
                completionResult.DecisionResults.Count);

        for (int index = 0;
            index < comparableCount;
            index++)
        {
            MetadataProductiveApplicationResult
                productiveResult =
                    completionResult.DecisionResults[index];

            if (!productiveResult.WasSuccessfullyPromoted)
            {
                continue;
            }

            MetadataApplyRequest request =
                batchRequest.ValidRequests[index];

            AudioFile? refreshedAudioFile =
                _fileScannerService.ScanFile(
                    request.FilePath);

            if (refreshedAudioFile is null)
            {
                AppendLog(
                    $"No fue posible volver a leer el archivo " +
                    $"promovido: {request.FileName}.");

                continue;
            }

            int existingIndex =
                audioFiles.FindIndex(
                    audioFile =>
                        string.Equals(
                            audioFile.FullPath,
                            refreshedAudioFile.FullPath,
                            StringComparison.OrdinalIgnoreCase));

            if (existingIndex < 0)
            {
                AppendLog(
                    $"El archivo promovido no fue encontrado en " +
                    $"la tabla: {request.FileName}.");

                continue;
            }

            audioFiles[existingIndex] =
                refreshedAudioFile;

            refreshedFilePaths.Add(
                refreshedAudioFile.FullPath);

            AppendLog(
                $"Fila actualizada desde el archivo promovido: " +
                $"{refreshedAudioFile.FileName}.");

            if (string.Equals(
                    selectedFilePath,
                    refreshedAudioFile.FullPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                refreshedSelectedAudioFile =
                    refreshedAudioFile;
            }
        }

        /*
         * Actualizamos ItemsSource una sola vez después de sustituir
         * todas las instancias afectadas.
         */
        AudioFilesDataGrid.ItemsSource =
            null;

        AudioFilesDataGrid.ItemsSource =
            audioFiles;

        if (refreshedSelectedAudioFile is not null)
        {
            AudioFilesDataGrid.SelectedItem =
                refreshedSelectedAudioFile;

            AudioFilesDataGrid.ScrollIntoView(
                refreshedSelectedAudioFile);

            AppendLog(
                "La selección actual fue restaurada usando los " +
                "metadatos recién leídos del archivo promovido.");
        }
        else if (!string.IsNullOrWhiteSpace(
                     selectedFilePath))
        {
            AudioFile? preservedSelection =
                audioFiles.FirstOrDefault(
                    audioFile =>
                        string.Equals(
                            audioFile.FullPath,
                            selectedFilePath,
                            StringComparison.OrdinalIgnoreCase));

            if (preservedSelection is not null)
            {
                AudioFilesDataGrid.SelectedItem =
                    preservedSelection;
            }
        }

        AppendLog(
            $"Refresco batch completado: " +
            $"{refreshedFilePaths.Count} archivo(s) releído(s).");

        return refreshedFilePaths;
    }

    /// <summary>
    /// Reconstruye el plan visual después de una promoción batch
    /// solamente cuando el archivo actualmente seleccionado fue uno
    /// de los archivos promovidos y releídos correctamente.
    ///
    /// Los demás archivos ya quedan actualizados en la tabla y serán
    /// analizados normalmente cuando el usuario los seleccione.
    /// </summary>
    private async Task
        RefreshCurrentSimulationPlanAfterBatchAsync(
            IReadOnlyList<string> refreshedFilePaths)
    {
        ArgumentNullException.ThrowIfNull(
            refreshedFilePaths);

        AudioFile? selectedAudioFile =
            GetSelectedAudioFile();

        if (selectedAudioFile is null)
        {
            SetCurrentSimulationPlan(
                null);

            AppendLog(
                "No existe una fila seleccionada después del " +
                "refresco productivo por lote.");

            return;
        }

        bool selectedFileWasRefreshed =
            refreshedFilePaths.Any(
                filePath =>
                    string.Equals(
                        filePath,
                        selectedAudioFile.FullPath,
                        StringComparison.OrdinalIgnoreCase));

        if (!selectedFileWasRefreshed)
        {
            /*
             * El archivo actualmente visible no fue promovido.
             * Conservamos su plan sin volver a ejecutar búsquedas ni
             * alterar posibles aprobaciones pendientes.
             */
            AppendLog(
                "El archivo actualmente seleccionado no fue " +
                "promovido; su plan visual fue conservado.");

            return;
        }

        AppendLog(
            "Reconstruyendo el plan del archivo seleccionado " +
            "desde los metadatos promovidos.");

        await AnalyzeSelectedFileAsync(
            FileAnalysisInvocation.PostApplicationRefresh);

        AppendLog(
            "El plan seleccionado fue reconstruido después de " +
            "la aplicación productiva por lote.");
    }

    /// <summary>
    /// Elimina de la selección productiva exclusivamente los
    /// archivos que fueron promovidos correctamente.
    ///
    /// Los fallidos y los no ejecutados permanecen seleccionados
    /// para revisión o reintento.
    /// </summary>
    private void RemoveSuccessfullyPromotedBatchItems(
        MetadataApplyBatchRequest batchRequest,
        MetadataProductiveBatchCompletionResult completionResult)
    {
        int comparableCount =
            Math.Min(
                batchRequest.ValidRequests.Count,
                completionResult.DecisionResults.Count);

        for (int index = 0;
            index < comparableCount;
            index++)
        {
            MetadataProductiveApplicationResult
                result =
                    completionResult.DecisionResults[index];

            if (!result.WasSuccessfullyPromoted)
            {
                continue;
            }

            MetadataApplyRequest
                request =
                    batchRequest.ValidRequests[index];

            bool removed =
                _productiveBatchSelection.Remove(
                    request.FilePath);

            AppendLog(
                removed
                    ? $"Archivo promovido retirado del lote: " +
                      $"{request.FileName}."
                    : $"El archivo promovido ya no estaba presente " +
                      $"en la selección: {request.FileName}.");
        }

        UpdateProductiveBatchUiState();
    }

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

            MetadataProductiveApplicationResult productiveResult =
                await _metadataProductiveApplicationCoordinator
                    .PrepareAsync(
                        request);

            MetadataApplicationIsolatedExecutionResult? result =
                productiveResult.IsolatedExecutionResult;

            if (result is null)
            {
                AppendLog(
                    "La preparación productiva no produjo un resultado " +
                    "aislado disponible.");

                if (!string.IsNullOrWhiteSpace(
                        productiveResult.ErrorMessage))
                {
                    AppendLog(
                        $"Error: {productiveResult.ErrorMessage}");
                }

                AppendLog(
                    $"Resumen: {productiveResult.Summary}");

                MessageBox.Show(
                    this,
                    "No fue posible preparar una copia verificada para la " +
                    "segunda confirmación.\n\n" +
                    "El archivo original permaneció intacto.\n\n" +
                    "Revise el Registro de actividad para obtener más " +
                    "información.",
                    "Preparación productiva incompleta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

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
                $"Entorno verificado conservado: " +
                $"{ToSpanish(
                    result.EnvironmentWasPreserved)}");

            AppendLog(
                $"Copia disponible para segunda confirmación: " +
                $"{ToSpanish(
                    productiveResult.VerifiedCopyWasPrepared)}");

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
                "=== Fin de la preparación productiva ===");

            if (productiveResult.VerifiedCopyWasPrepared)
            {
                MessageBoxResult confirmationResult =
                    MessageBox.Show(
                        this,
                        "La copia temporal fue preparada y verificada " +
                        "correctamente.\n\n" +
                        "El archivo original permaneció intacto.\n\n" +
                        "¿Desea promover ahora la copia verificada y aplicar " +
                        "los cambios al archivo original?\n\n" +
                        "Seleccione Sí para aplicar los cambios o No para " +
                        "cancelar la promoción.",
                        "Segunda confirmación requerida",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No);

                MetadataPromotionDecision promotionDecision =
                    confirmationResult == MessageBoxResult.Yes
                        ? MetadataPromotionDecision.Approved
                        : MetadataPromotionDecision.Declined;

                AppendLog(
                    promotionDecision ==
                        MetadataPromotionDecision.Approved
                        ? "El usuario aprobó la promoción productiva."
                        : "El usuario rechazó la promoción productiva.");

                MetadataProductiveApplicationResult completedResult =
                    await _metadataProductiveApplicationCoordinator
                        .CompleteAsync(
                            productiveResult,
                            promotionDecision);

                AppendLog(
                    "=== Finalización productiva ===");

                foreach (string message
                    in completedResult.Messages)
                {
                    AppendLog(
                        $"- {message}");
                }

                AppendLog(
                    $"Promoción aprobada: " +
                    $"{ToSpanish(
                        completedResult.PromotionWasApproved)}");

                AppendLog(
                    $"Promoción rechazada: " +
                    $"{ToSpanish(
                        completedResult.PromotionWasDeclined)}");

                AppendLog(
                    $"Promoción completada: " +
                    $"{ToSpanish(
                        completedResult.WasSuccessfullyPromoted)}");

                AppendLog(
                    $"Rechazo seguro: " +
                    $"{ToSpanish(
                        completedResult.WasSafelyDeclined)}");

                AppendLog(
                    $"Archivo original en estado seguro: " +
                    $"{ToSpanish(
                        completedResult.OriginalEndedInSafeState)}");

                AppendLog(
                    $"Estado final controlado: " +
                    $"{ToSpanish(
                        completedResult.EndedInControlledState)}");

                if (!string.IsNullOrWhiteSpace(
                        completedResult.ErrorMessage))
                {
                    AppendLog(
                        $"Error: {completedResult.ErrorMessage}");
                }

                AppendLog(
                    $"Resumen: {completedResult.Summary}");

                AppendLog(
                    "=== Fin de la finalización productiva ===");

                if (completedResult.WasSuccessfullyPromoted)
                {
                    bool interfaceWasRefreshed =
                        RefreshAppliedAudioFile(
                            request.FilePath);

                    AppendLog(
                        $"Interfaz actualizada desde el archivo aplicado: " +
                        $"{ToSpanish(
                            interfaceWasRefreshed)}");

                    if (interfaceWasRefreshed)
                    {
                        await AnalyzeSelectedFileAsync(
                            FileAnalysisInvocation.PostApplicationRefresh);

                        AppendLog(
                            "El plan de simulación fue reconstruido desde " +
                            "el archivo aplicado.");
                    }

                    MessageBox.Show(
                        this,
                        "Los cambios fueron aplicados correctamente.\n\n" +
                        "Se creó y verificó un respaldo antes de modificar " +
                        "el archivo original.\n\n" +
                        "La escritura final también fue verificada.",
                        "Cambios aplicados",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else if (completedResult.WasSafelyDeclined)
                {
                    MessageBox.Show(
                        this,
                        "La promoción fue cancelada.\n\n" +
                        "El archivo original permaneció intacto y el entorno " +
                        "temporal fue eliminado de forma segura.",
                        "Aplicación cancelada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        this,
                        "La finalización productiva no terminó correctamente.\n\n" +
                        "El sistema intentó mantener el archivo en un estado " +
                        "seguro.\n\n" +
                        "Revise el Registro de actividad.",
                        "Finalización productiva incompleta",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                    this,
                    "La preparación productiva no superó todas las " +
                    "comprobaciones.\n\n" +
                    "Revise el Registro de actividad para obtener " +
                    "más información.\n\n" +
                    "El archivo original permaneció protegido.",
                    "Preparación productiva incompleta",
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
        await AnalyzeSelectedFileAsync(
            FileAnalysisInvocation.UserRequested);
    }

    /// <summary>
    /// Analiza el archivo seleccionado y reconstruye su plan de simulación.
    /// </summary>
    private async Task AnalyzeSelectedFileAsync(
        FileAnalysisInvocation invocation)
    {
        bool isPostApplicationRefresh =
            invocation ==
            FileAnalysisInvocation.PostApplicationRefresh;

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

            if (!isPostApplicationRefresh)
            {
                LogTextBox.AppendText(
                    Environment.NewLine +
                    pipelineReport +
                    Environment.NewLine);
            }

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

            if (!isPostApplicationRefresh)
            {
                LogTextBox.AppendText(
                    Environment.NewLine +
                    candidateRankingReport +
                    Environment.NewLine);
            }

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

            if (!isPostApplicationRefresh)
            {
                LogTextBox.AppendText(
                    Environment.NewLine +
                    consensusReport +
                    Environment.NewLine);
            }

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

            if (!isPostApplicationRefresh)
            {
                LogTextBox.AppendText(
                    Environment.NewLine +
                    changePlanReport +
                    Environment.NewLine);
            }

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
            await _audioDiagnosticsOrchestrator
                .RunFullDiagnosticAsync(
                    filePath,
                    _productiveBatchSelection,
                    AppendLog);
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