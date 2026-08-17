using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Renaming;
using AudioMetadataManager.UI.Services.Renaming.Models;
using System.Windows;
using System.Windows.Controls;

namespace AudioMetadataManager.UI.Views;

/// <summary>
/// Presenta la simulación y ejecución del renombrado seguro de archivos individual y en lote.
/// </summary>
public partial class RenamePreviewView : UserControl
{
    public event EventHandler<AudioFile>? RenameRequested;
    public event EventHandler<FileRenameBatchPreparationResult>? BatchRenameRequested;

    private readonly FileRenameBatchService _batchService = new();
    private FileRenameBatchPreparationResult? _currentBatchPreparation;
    private IEnumerable<AudioFile>? _currentLibraryFiles;

    public RenamePreviewView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Actualiza el contexto de la biblioteca musical para el análisis de renombrado por lote.
    /// </summary>
    public void SetLibraryContext(IEnumerable<AudioFile>? files)
    {
        _currentLibraryFiles = files;

        if (files is null || !files.Any())
        {
            _currentBatchPreparation = null;
            BatchReadyCountTextBlock.Text = "0";
            BatchUnchangedCountTextBlock.Text = "0";
            BatchCollisionCountTextBlock.Text = "0";
            BatchCandidatesDataGrid.ItemsSource = null;
            ExecuteBatchRenameButton.IsEnabled = false;
            ExecuteBatchRenameButton.Content = "Renombrar lote seleccionado";
            return;
        }

        _currentBatchPreparation = _batchService.PrepareBatch(files);

        BatchReadyCountTextBlock.Text = _currentBatchPreparation.ReadyToRenameCount.ToString();
        BatchUnchangedCountTextBlock.Text = _currentBatchPreparation.UnchangedCount.ToString();
        BatchCollisionCountTextBlock.Text = _currentBatchPreparation.CollisionCount.ToString();

        BatchCandidatesDataGrid.ItemsSource = _currentBatchPreparation.Items;

        UpdateBatchButtonState();
    }

    private void UpdateBatchButtonState()
    {
        if (_currentBatchPreparation is null)
        {
            ExecuteBatchRenameButton.IsEnabled = false;
            ExecuteBatchRenameButton.Content = "Renombrar lote seleccionado";
            return;
        }

        int selectedReady = _currentBatchPreparation.SelectedReadyCount;
        ExecuteBatchRenameButton.IsEnabled = selectedReady > 0;
        ExecuteBatchRenameButton.Content = selectedReady > 0
            ? $"Renombrar lote seleccionado ({selectedReady} archivo{(selectedReady > 1 ? "s" : "")})"
            : "Renombrar lote seleccionado";
    }

    private void RenameSingleFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AudioFile audioFile)
        {
            RenameRequested?.Invoke(this, audioFile);
        }
    }

    private void SelectAllReadyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentBatchPreparation is not null)
        {
            foreach (var item in _currentBatchPreparation.Items)
            {
                if (item.CanRename)
                {
                    item.IsSelected = true;
                }
            }
            BatchCandidatesDataGrid.Items.Refresh();
            UpdateBatchButtonState();
        }
    }

    private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentBatchPreparation is not null)
        {
            foreach (var item in _currentBatchPreparation.Items)
            {
                item.IsSelected = false;
            }
            BatchCandidatesDataGrid.Items.Refresh();
            UpdateBatchButtonState();
        }
    }

    private void RefreshBatchAnalysisButton_Click(object sender, RoutedEventArgs e)
    {
        SetLibraryContext(_currentLibraryFiles);
    }

    private void ExecuteBatchRenameButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentBatchPreparation is not null && _currentBatchPreparation.SelectedReadyCount > 0)
        {
            BatchRenameRequested?.Invoke(this, _currentBatchPreparation);
        }
    }
}
