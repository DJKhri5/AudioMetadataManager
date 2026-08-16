using System.Windows;

namespace AudioMetadataManager.UI.Views.Controls;

/// <summary>
/// Captura un valor de metadatos introducido explícitamente por
/// el usuario sin modificar todavía el archivo de audio.
/// </summary>
public partial class ManualMetadataValueDialog : Window
{
    public ManualMetadataValueDialog(
        string fieldDisplay,
        string currentValue,
        string proposedValue)
    {
        InitializeComponent();

        FieldTextBlock.Text =
            $"Valor manual para {fieldDisplay}";

        CurrentValueTextBlock.Text =
            $"Valor actual: {currentValue}";

        ProposedValueTextBox.Text =
            proposedValue;

        Loaded +=
            (_, _) =>
            {
                ProposedValueTextBox.Focus();
                ProposedValueTextBox.SelectAll();
            };
    }

    public string EnteredValue =>
        ProposedValueTextBox.Text.Trim();

    private void AcceptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(
                EnteredValue))
        {
            MessageBox.Show(
                this,
                "Ingresa un valor antes de continuar.",
                "Valor requerido",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            ProposedValueTextBox.Focus();

            return;
        }

        DialogResult =
            true;
    }
}
