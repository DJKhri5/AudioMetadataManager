using System.Windows;
using System.Windows.Controls;
using AudioMetadataManager.UI.Services.MetadataSources
    .Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Configuration;

namespace AudioMetadataManager.UI.Views.Controls;

/// <summary>
/// Administra visualmente la configuración de Discogs.
///
/// Todas las operaciones técnicas se delegan al servicio de
/// configuración correspondiente.
/// </summary>
public partial class DiscogsSettingsControl
    : UserControl
{
    private readonly IMetadataSourceConfigurationService
        _configurationService;

    private bool
        _discogsIsConfigured;

    private bool
    _isTestingConnection;

    /// <summary>
    /// Comprueba de forma asíncrona el token configurado
    /// mediante el endpoint de identidad de Discogs.
    /// </summary>
    private async void TestDiscogsConnectionButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_isTestingConnection)
        {
            return;
        }

        _isTestingConnection =
            true;

        SetConnectionTestState(
            isTesting: true);

        try
        {
            MetadataSourceConfigurationResult result =
                await _configurationService.TestConnectionAsync();

            ApplyConfigurationResult(
                result);
        }
        catch (OperationCanceledException)
        {
            ApplyConfigurationResult(
                MetadataSourceConfigurationResult.Failure(
                    _configurationService.SourceName,
                    MetadataSourceConfigurationState.Error,
                    "La comprobación de Discogs fue cancelada."));
        }
        catch (Exception exception)
        {
            ApplyConfigurationResult(
                MetadataSourceConfigurationResult.Failure(
                    _configurationService.SourceName,
                    MetadataSourceConfigurationState.Error,
                    "Ocurrió un error inesperado al comprobar Discogs: " +
                    exception.Message));
        }
        finally
        {
            _isTestingConnection =
                false;

            SetConnectionTestState(
                isTesting: false);
        }
    }

    /// <summary>
    /// Bloquea temporalmente las acciones mientras se realiza
    /// la comprobación externa.
    /// </summary>
    private void SetConnectionTestState(
        bool isTesting)
    {
        DiscogsTokenPasswordBox.IsEnabled =
            !isTesting;

        SaveDiscogsTokenButton.IsEnabled =
            !isTesting &&
            !string.IsNullOrWhiteSpace(
                DiscogsTokenPasswordBox.Password);

        DeleteDiscogsTokenButton.IsEnabled =
            !isTesting &&
            _discogsIsConfigured;

        TestDiscogsConnectionButton.IsEnabled =
            !isTesting &&
            _discogsIsConfigured;

        TestDiscogsConnectionButton.Content =
            isTesting
                ? "Comprobando..."
                : "Probar conexión";

        if (isTesting)
        {
            DiscogsStatusTextBlock.Text =
                "Comprobando...";

            DiscogsMessageTextBlock.Text =
                "Conectando de forma segura con Discogs.";
        }
    }

    public DiscogsSettingsControl()
        : this(
            new DiscogsConfigurationService())
    {
    }

    public DiscogsSettingsControl(
        IMetadataSourceConfigurationService configurationService)
    {
        InitializeComponent();

        _configurationService =
            configurationService ??
            throw new ArgumentNullException(
                nameof(configurationService));

        Loaded +=
            DiscogsSettingsControl_Loaded;
    }

    private void DiscogsSettingsControl_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        MetadataSourceConfigurationResult result =
            _configurationService.GetStatus();

        ApplyConfigurationResult(
            result);
    }

    private void DiscogsTokenPasswordBox_PasswordChanged(
        object sender,
        RoutedEventArgs e)
    {
        SaveDiscogsTokenButton.IsEnabled =
            !string.IsNullOrWhiteSpace(
                DiscogsTokenPasswordBox.Password);
    }

    private void SaveDiscogsTokenButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string token =
            DiscogsTokenPasswordBox.Password;

        MetadataSourceConfigurationResult result =
            _configurationService.SaveCredential(
                token);

        ClearTokenInput();

        ApplyConfigurationResult(
            result);
    }

    private void DeleteDiscogsTokenButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!_discogsIsConfigured)
        {
            ApplyConfigurationResult(
                _configurationService.GetStatus());

            return;
        }

        Window? ownerWindow =
            Window.GetWindow(
                this);

        MessageBoxResult confirmation =
            MessageBox.Show(
                ownerWindow,
                "¿Deseas eliminar el token de Discogs guardado en Windows?",
                "Eliminar token de Discogs",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

        if (confirmation !=
            MessageBoxResult.Yes)
        {
            return;
        }

        MetadataSourceConfigurationResult result =
            _configurationService.DeleteCredential();

        ClearTokenInput();

        ApplyConfigurationResult(
            result);
    }

    private void ApplyConfigurationResult(
        MetadataSourceConfigurationResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        _discogsIsConfigured =
            result.IsConfigured;

        DiscogsStatusTextBlock.Text =
            GetStatusDisplay(
                result.State);

        DeleteDiscogsTokenButton.IsEnabled =
            !_isTestingConnection &&
            result.IsConfigured;

        TestDiscogsConnectionButton.IsEnabled =
            !_isTestingConnection &&
            result.IsConfigured;

        DiscogsMessageTextBlock.Text =
            string.IsNullOrWhiteSpace(
                result.Message)
                    ? "Sin información adicional."
                    : result.Message;
    }

    private static string GetStatusDisplay(
        MetadataSourceConfigurationState state)
    {
        return state switch
        {
            MetadataSourceConfigurationState.NotConfigured =>
                "No configurado",

            MetadataSourceConfigurationState.Configured =>
                "Configurado",

            MetadataSourceConfigurationState.ConnectionVerified =>
                "Conexión verificada",

            MetadataSourceConfigurationState.AuthenticationFailed =>
                "Credenciales rechazadas",

            MetadataSourceConfigurationState.Error =>
                "Error de configuración",

            _ =>
                "Comprobando..."
        };
    }

    private void ClearTokenInput()
    {
        DiscogsTokenPasswordBox.Clear();

        SaveDiscogsTokenButton.IsEnabled =
            false;
    }
}