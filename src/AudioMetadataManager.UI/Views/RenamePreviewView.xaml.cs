using AudioMetadataManager.UI.Models;
using System.Windows;
using System.Windows.Controls;

namespace AudioMetadataManager.UI.Views;

/// <summary>
/// Presenta la simulación y ejecución del renombrado seguro de archivos.
/// </summary>
public partial class RenamePreviewView : UserControl
{
    public event EventHandler<AudioFile>? RenameRequested;

    public RenamePreviewView()
    {
        InitializeComponent();
    }

    private void RenameSingleFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AudioFile audioFile)
        {
            RenameRequested?.Invoke(this, audioFile);
        }
    }
}
