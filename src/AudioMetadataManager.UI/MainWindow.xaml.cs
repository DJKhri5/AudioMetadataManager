using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services;
using Microsoft.Win32;
using System.Windows;

namespace AudioMetadataManager.UI;

public partial class MainWindow : Window
{
    private readonly FileScannerService _fileScannerService = new();

    private void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = LibraryPathTextBox.Text;

        LogTextBox.AppendText(
            $"{Environment.NewLine}Iniciando análisis de la biblioteca...");

        List<AudioFile> audioFiles = _fileScannerService.ScanFolder(folderPath);

        AudioFilesDataGrid.ItemsSource = audioFiles;

        LogTextBox.AppendText(
            $"{Environment.NewLine}Análisis finalizado. " +
            $"Se encontraron {audioFiles.Count} archivos compatibles.");

        LogTextBox.ScrollToEnd();

        SaveProjectButton.IsEnabled = audioFiles.Count > 0;
        ExportButton.IsEnabled = audioFiles.Count > 0;
    }
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog dialog = new()
        {
            Title = "Seleccione la biblioteca musical",
            Multiselect = false
        };

        bool? result = dialog.ShowDialog(this);

        if (result != true)
        {
            return;
        }

        LibraryPathTextBox.Text = dialog.FolderName;
        ScanButton.IsEnabled = true;

        LogTextBox.AppendText(
            $"{Environment.NewLine}Biblioteca seleccionada: {dialog.FolderName}");

        LogTextBox.ScrollToEnd();
    }
    
}