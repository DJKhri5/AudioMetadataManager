using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AudioMetadataManager.UI.Services.Duplicates.Models;

namespace AudioMetadataManager.UI.Views;

public partial class DuplicateDetectionView : UserControl
{
    private DuplicateDetectionResult? _currentResult;

    public DuplicateDetectionView()
    {
        InitializeComponent();
    }

    public void SetDuplicateResult(DuplicateDetectionResult? result)
    {
        _currentResult = result;

        if (result is null || result.TotalDuplicateGroups == 0)
        {
            GroupsCountTextBlock.Text = "0";
            DuplicateFilesCountTextBlock.Text = "0";
            ReclaimableSpaceTextBlock.Text = "0.00 MB";
            DuplicateGroupsListBox.ItemsSource = null;
            GroupItemsDataGrid.ItemsSource = null;
            SelectedGroupHeaderBorder.Visibility = Visibility.Collapsed;
            NoSelectionTextBlock.Visibility = Visibility.Visible;
            NoSelectionTextBlock.Text = "No se detectaron archivos duplicados en la biblioteca analizada.";
            return;
        }

        GroupsCountTextBlock.Text = result.TotalDuplicateGroups.ToString();
        DuplicateFilesCountTextBlock.Text = result.TotalDuplicateFiles.ToString();
        ReclaimableSpaceTextBlock.Text = result.TotalPotentialReclaimableDisplay;

        DuplicateGroupsListBox.ItemsSource = result.Groups;

        if (result.Groups.Count > 0)
        {
            DuplicateGroupsListBox.SelectedIndex = 0;
        }
    }

    private void DuplicateGroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DuplicateGroupsListBox.SelectedItem is DuplicateGroup group)
        {
            SelectedGroupHeaderBorder.Visibility = Visibility.Visible;
            NoSelectionTextBlock.Visibility = Visibility.Collapsed;
            GroupItemsDataGrid.Visibility = Visibility.Visible;

            SelectedGroupTitleTextBlock.Text = group.DisplayTitle;
            SelectedGroupReclaimableTextBlock.Text = $"Espacio redundante: {group.PotentialReclaimableDisplay}";

            GroupItemsDataGrid.ItemsSource = group.Items;
        }
        else
        {
            SelectedGroupHeaderBorder.Visibility = Visibility.Collapsed;
            NoSelectionTextBlock.Visibility = Visibility.Visible;
            NoSelectionTextBlock.Text = "Selecciona un grupo a la izquierda para comparar sus versiones.";
            GroupItemsDataGrid.ItemsSource = null;
        }
    }

    private void OpenContainingFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string fullPath && File.Exists(fullPath))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{fullPath}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo abrir el explorador de archivos: {ex.Message}",
                    "Abrir carpeta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }

    private void CopyPathButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string fullPath && !string.IsNullOrWhiteSpace(fullPath))
        {
            try
            {
                Clipboard.SetText(fullPath);
                MessageBox.Show(
                    $"Ruta copiada al portapapeles:\n{fullPath}",
                    "Ruta copiada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo copiar la ruta: {ex.Message}",
                    "Copiar ruta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
