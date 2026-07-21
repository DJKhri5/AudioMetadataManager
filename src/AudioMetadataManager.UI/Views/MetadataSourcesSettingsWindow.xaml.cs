using System.Windows;

namespace AudioMetadataManager.UI.Views;

/// <summary>
/// Contenedor de configuración para las fuentes externas.
/// </summary>
public partial class MetadataSourcesSettingsWindow
    : Window
{
    public MetadataSourcesSettingsWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}