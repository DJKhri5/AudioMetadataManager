using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AudioMetadataManager.Models;
using AudioMetadataManager.Services;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace AudioMetadataManager;
public partial class MainWindow : Window
{
    private readonly ObservableCollection<AudioItem> _items = [];
    private SimulationProject _project = new();
    private CancellationTokenSource? _cts;
    public MainWindow() { InitializeComponent(); ItemsGrid.ItemsSource = _items; }

    private async void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog { Description = "Seleccione la biblioteca o carpeta de Google Drive" };
        if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
        await ScanFolderAsync(dialog.SelectedPath);
    }

    private async Task ScanFolderAsync(string folder)
    {
        try
        {
            ToggleBusy(true); _cts = new(); Progress.Value = 0; StatusText.Text = "Preparando escaneo...";
            var total = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Count();
            var progress = new Progress<(int done, string file)>(p => { Progress.Value = total == 0 ? 0 : p.done * 100d / total; StatusText.Text = $"Analizando: {Path.GetFileName(p.file)}"; CountText.Text = $"{p.done} procesados"; });
            var result = await LibraryScanner.ScanAsync(folder, progress, _cts.Token);
            _items.Clear(); foreach (var item in result) _items.Add(item);
            _project = new SimulationProject { ProjectName = Path.GetFileName(folder), RootFolder = folder, Items = result };
            CountText.Text = $"{result.Count} archivos"; StatusText.Text = "Simulación terminada. 0 archivos modificados."; Progress.Value = 100;
        }
        catch (OperationCanceledException) { StatusText.Text = "Escaneo cancelado"; }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); StatusText.Text = "Error"; }
        finally { ToggleBusy(false); }
    }

    private async void SaveProject_Click(object sender, RoutedEventArgs e)
    {
        if (_items.Count == 0) { MessageBox.Show("No hay una simulación para guardar."); return; }
        var dlg = new SaveFileDialog { Filter = "Proyecto AMM (*.ammproj)|*.ammproj", FileName = $"{_project.ProjectName}.ammproj" };
        if (dlg.ShowDialog() != true) return;
        _project.Items = _items.ToList(); await ProjectService.SaveAsync(dlg.FileName, _project); StatusText.Text = "Proyecto guardado";
    }

    private async void OpenProject_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "Proyecto AMM (*.ammproj)|*.ammproj" };
        if (dlg.ShowDialog() != true) return;
        try { _project = await ProjectService.LoadAsync(dlg.FileName); _items.Clear(); foreach (var item in _project.Items) _items.Add(item); CountText.Text = $"{_items.Count} archivos"; StatusText.Text = "Proyecto abierto"; }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Proyecto no válido", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_items.Count == 0) return;
        var dlg = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "simulacion.csv" };
        if (dlg.ShowDialog() != true) return;
        await CsvExporter.ExportAsync(dlg.FileName, _items); StatusText.Text = "CSV exportado";
    }

    private async void Backup_Click(object sender, RoutedEventArgs e)
    {
        var selected = _items.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0 || string.IsNullOrWhiteSpace(_project.RootFolder)) { MessageBox.Show("Seleccione al menos un archivo."); return; }
        if (MessageBox.Show($"Se copiarán {selected.Count} archivos a _Respaldo Audio y se verificará SHA-256. ¿Continuar?", "Crear respaldo", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try { ToggleBusy(true); _cts = new(); var progress = new Progress<string>(s => StatusText.Text = s); var manifest = await BackupService.BackupAsync(_project.RootFolder, selected, progress, _cts.Token); MessageBox.Show($"Respaldo verificado.\n\n{manifest}", "Completado", MessageBoxButton.OK, MessageBoxImage.Information); StatusText.Text = "Respaldo verificado"; }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error de respaldo", MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { ToggleBusy(false); }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => _cts?.Cancel();
    private void ItemsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemsGrid.SelectedItem is not AudioItem x) return;
        PathBox.Text = x.FullPath; ProposedBox.Text = x.ProposedFileName; ArtistBox.Text = x.Artist; TitleBox.Text = x.Title; VersionBox.Text = x.Version; TechnicalBox.Text = x.TechnicalSummary + $" | {x.SizeDisplay} | Carátula: {(x.HasArtwork ? "Sí" : "No")}"; ItemStatusBox.Text = x.Status; WarningsBox.Text = x.Warnings;
    }
    private void ToggleBusy(bool busy) { Mouse.OverrideCursor = busy ? System.Windows.Input.Cursors.Wait : null; }
}
