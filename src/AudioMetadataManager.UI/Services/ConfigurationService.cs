using System;
using System.IO;
using System.Text.Json;

namespace AudioMetadataManager.UI.Services
{
    /// <summary>
    /// Servicio para cargar y proporcionar acceso a la configuración de la aplicación desde appsettings.json.
    /// </summary>
    public class ConfigurationService
    {
        private static readonly Lazy<ConfigurationService> _lazy =
            new Lazy<ConfigurationService>(() => new ConfigurationService());

        public static ConfigurationService Instance => _lazy.Value;

        public BackupSettings Backup { get; private set; }
        public ScanSettings Scan { get; private set; }
        public NamingSettings Naming { get; private set; }
        public ApplicationSettings Application { get; private set; }

        private ConfigurationService()
        {
            // Establecer valores por defecto (en caso de que el archivo de configuración falte o tenga errores)
            Backup = new BackupSettings();
            Scan = new ScanSettings();
            Naming = new NamingSettings();
            Application = new ApplicationSettings();

            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ConfigurationRoot>(json);

                    if (config != null)
                    {
                        Backup = config.Backup ?? new BackupSettings();
                        Scan = config.Scan ?? new ScanSettings();
                        Naming = config.Naming ?? new NamingSettings();
                        Application = config.Application ?? new ApplicationSettings();
                    }
                }
                else
                {
                    // El archivo no existe, se usan los valores por defecto ya establecidos.
                    // Podemos registrar una advertencia aquí si tuviéramos un logger.
                    // Por ahora, no hacemos nada para no romper la ejecución.
                }
            }
            catch (Exception ex)
            {
                // En caso de error al leer o deserializar, se usan los valores por defecto.
                // En una aplicación real, se registraría el error.
                // Por ahora, ignoramos el error para no romper el inicio de la aplicación.
                // Pero al menos podemos escribir a la consola de depuración si estamos en modo debug.
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Error al cargar la configuración: {ex.Message}");
#endif
            }
        }

        /// <summary>
        /// Clase raíz que representa la estructura completa del archivo appsettings.json.
        /// </summary>
        private class ConfigurationRoot
        {
            public BackupSettings Backup { get; set; }
            public ScanSettings Scan { get; set; }
            public NamingSettings Naming { get; set; }
            public ApplicationSettings Application { get; set; }
        }
    }

    /// <summary>
    /// Configuración relacionada con los backups.
    /// </summary>
    public class BackupSettings
    {
        public string RootFolderName { get; set; } = "AudioMetadataManager_Backup";
        public int RetentionDays { get; set; } = 30;
        public int MaxTotalSizeMB { get; set; } = 1024;
    }

    /// <summary>
    /// Configuración relacionada con el escaneo de archivos.
    /// </summary>
    public class ScanSettings
    {
        public string[] SupportedExtensions { get; set; } = new[]
        {
            ".mp3", ".flac", ".wav", ".aac", ".m4a", ".ogg", ".wma", ".opus", ".ape", ".aiff"
        };
        public int MaxDepth { get; set; } = 5;
        public string[] ExcludeFolders { get; set; } = new[]
        {
            "$Recycle.Bin", "System Volume Information", "Temp", "TMP"
        };
    }

    /// <summary>
    /// Configuración relacionada con la generación de nombres de archivo.
    /// </summary>
    public class NamingSettings
    {
        public string DefaultTemplate { get; set; } = "{artist} - {title} ({version}){extension}";
        public bool UsePartialDataWhenMissing { get; set; } = true;
        public bool RemoveLeadingTrackNumbers { get; set; } = true;
        public bool RemoveSiteTags { get; set; } = true;
        public bool RemoveEncoderTags { get; set; } = true;
    }

    /// <summary>
    /// Configuración general de la aplicación.
    /// </summary>
    public class ApplicationSettings
    {
        public string Version { get; set; } = "0.4.0";
        public string LogLevel { get; set; } = "Info";
    }
}