using System.Windows;
using AudioMetadataManager.UI.Services.MetadataSources.Configuration;

namespace AudioMetadataManager.UI.Views;

public partial class ExternalProvidersSettingsWindow : Window
{
    private readonly MetadataSourceConfigurationRegistry _registry = new();

    public ExternalProvidersSettingsWindow()
    {
        InitializeComponent();
        Loaded += ExternalProvidersSettingsWindow_Loaded;
    }

    private void ExternalProvidersSettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshAllStatuses();
    }

    private void RefreshAllStatuses()
    {
        var mbStatus = _registry.GetService("MusicBrainz")?.GetStatus();
        if (mbStatus != null)
        {
            MusicBrainzStatusTextBlock.Text = mbStatus.Message;
        }

        var discogsStatus = _registry.GetService("Discogs")?.GetStatus();
        if (discogsStatus != null)
        {
            DiscogsStatusTextBlock.Text = discogsStatus.Message;
        }

        var beatportStatus = _registry.GetService("Beatport")?.GetStatus();
        if (beatportStatus != null)
        {
            BeatportStatusTextBlock.Text = beatportStatus.Message;
        }

        var spotifyStatus = _registry.GetService("Spotify")?.GetStatus();
        if (spotifyStatus != null)
        {
            SpotifyStatusTextBlock.Text = spotifyStatus.Message;
        }

        var soundCloudStatus = _registry.GetService("SoundCloud")?.GetStatus();
        if (soundCloudStatus != null)
        {
            SoundCloudStatusTextBlock.Text = soundCloudStatus.Message;
        }
    }

    private async void TestMusicBrainzButton_Click(object sender, RoutedEventArgs e)
    {
        TestMusicBrainzButton.IsEnabled = false;
        MusicBrainzStatusTextBlock.Text = "Probando conexión con MusicBrainz...";

        var service = _registry.GetService("MusicBrainz");
        if (service != null)
        {
            var result = await service.TestConnectionAsync();
            MusicBrainzStatusTextBlock.Text = result.Message;
        }

        TestMusicBrainzButton.IsEnabled = true;
    }

    private async void TestDiscogsButton_Click(object sender, RoutedEventArgs e)
    {
        TestDiscogsButton.IsEnabled = false;
        DiscogsStatusTextBlock.Text = "Probando conexión con Discogs...";

        var service = _registry.GetService("Discogs");
        if (service != null)
        {
            var result = await service.TestConnectionAsync();
            DiscogsStatusTextBlock.Text = result.Message;
        }

        TestDiscogsButton.IsEnabled = true;
    }

    private void SaveDiscogsButton_Click(object sender, RoutedEventArgs e)
    {
        string token = DiscogsTokenPasswordBox.Password;
        var service = _registry.GetService("Discogs");
        if (service != null)
        {
            var result = service.SaveCredential(token);
            DiscogsStatusTextBlock.Text = result.Message;
            if (result.OperationSucceeded)
            {
                DiscogsTokenPasswordBox.Password = string.Empty;
            }
        }
    }

    private void DeleteDiscogsButton_Click(object sender, RoutedEventArgs e)
    {
        var service = _registry.GetService("Discogs");
        if (service != null)
        {
            var result = service.DeleteCredential();
            DiscogsStatusTextBlock.Text = result.Message;
        }
    }

    private async void TestBeatportButton_Click(object sender, RoutedEventArgs e)
    {
        TestBeatportButton.IsEnabled = false;
        BeatportStatusTextBlock.Text = "Probando conexión con Beatport...";

        var service = _registry.GetService("Beatport");
        if (service != null)
        {
            var result = await service.TestConnectionAsync();
            BeatportStatusTextBlock.Text = result.Message;
        }

        TestBeatportButton.IsEnabled = true;
    }

    private void SaveBeatportButton_Click(object sender, RoutedEventArgs e)
    {
        string key = BeatportApiKeyTextBox.Text;
        var service = _registry.GetService("Beatport");
        if (service != null)
        {
            var result = service.SaveCredential(key);
            BeatportStatusTextBlock.Text = result.Message;
            if (result.OperationSucceeded)
            {
                BeatportApiKeyTextBox.Text = string.Empty;
            }
        }
    }

    private void DeleteBeatportButton_Click(object sender, RoutedEventArgs e)
    {
        var service = _registry.GetService("Beatport");
        if (service != null)
        {
            var result = service.DeleteCredential();
            BeatportStatusTextBlock.Text = result.Message;
        }
    }

    private async void TestSpotifyButton_Click(object sender, RoutedEventArgs e)
    {
        TestSpotifyButton.IsEnabled = false;
        SpotifyStatusTextBlock.Text = "Probando conexión con Spotify...";

        var service = _registry.GetService("Spotify");
        if (service != null)
        {
            var result = await service.TestConnectionAsync();
            SpotifyStatusTextBlock.Text = result.Message;
        }

        TestSpotifyButton.IsEnabled = true;
    }

    private void SaveSpotifyButton_Click(object sender, RoutedEventArgs e)
    {
        string key = SpotifyApiKeyTextBox.Text;
        var service = _registry.GetService("Spotify");
        if (service != null)
        {
            var result = service.SaveCredential(key);
            SpotifyStatusTextBlock.Text = result.Message;
            if (result.OperationSucceeded)
            {
                SpotifyApiKeyTextBox.Text = string.Empty;
            }
        }
    }

    private void DeleteSpotifyButton_Click(object sender, RoutedEventArgs e)
    {
        var service = _registry.GetService("Spotify");
        if (service != null)
        {
            var result = service.DeleteCredential();
            SpotifyStatusTextBlock.Text = result.Message;
        }
    }

    private async void TestSoundCloudButton_Click(object sender, RoutedEventArgs e)
    {
        TestSoundCloudButton.IsEnabled = false;
        SoundCloudStatusTextBlock.Text = "Probando conexión con SoundCloud...";

        var service = _registry.GetService("SoundCloud");
        if (service != null)
        {
            var result = await service.TestConnectionAsync();
            SoundCloudStatusTextBlock.Text = result.Message;
        }

        TestSoundCloudButton.IsEnabled = true;
    }

    private void SaveSoundCloudButton_Click(object sender, RoutedEventArgs e)
    {
        string key = SoundCloudApiKeyTextBox.Text;
        var service = _registry.GetService("SoundCloud");
        if (service != null)
        {
            var result = service.SaveCredential(key);
            SoundCloudStatusTextBlock.Text = result.Message;
            if (result.OperationSucceeded)
            {
                SoundCloudApiKeyTextBox.Text = string.Empty;
            }
        }
    }

    private void DeleteSoundCloudButton_Click(object sender, RoutedEventArgs e)
    {
        var service = _registry.GetService("SoundCloud");
        if (service != null)
        {
            var result = service.DeleteCredential();
            SoundCloudStatusTextBlock.Text = result.Message;
        }
    }

    private async void TestAllButton_Click(object sender, RoutedEventArgs e)
    {
        TestAllButton.IsEnabled = false;
        MusicBrainzStatusTextBlock.Text = "Comprobando...";
        DiscogsStatusTextBlock.Text = "Comprobando...";
        BeatportStatusTextBlock.Text = "Comprobando...";
        SpotifyStatusTextBlock.Text = "Comprobando...";
        SoundCloudStatusTextBlock.Text = "Comprobando...";

        var results = await _registry.TestAllAsync();
        foreach (var res in results)
        {
            switch (res.SourceName.ToLowerInvariant())
            {
                case "musicbrainz":
                    MusicBrainzStatusTextBlock.Text = res.Message;
                    break;
                case "discogs":
                    DiscogsStatusTextBlock.Text = res.Message;
                    break;
                case "beatport":
                    BeatportStatusTextBlock.Text = res.Message;
                    break;
                case "spotify":
                    SpotifyStatusTextBlock.Text = res.Message;
                    break;
                case "soundcloud":
                    SoundCloudStatusTextBlock.Text = res.Message;
                    break;
            }
        }

        TestAllButton.IsEnabled = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
