using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Writers;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Testing;

/// <summary>
/// Adaptador compatible para la prueba aislada MP3.
///
/// La ejecución común pertenece ahora a
/// TagLibIsolatedWriteTestRunner.
/// </summary>
public sealed class TagLibMp3IsolatedWriteTestRunner
{
    private readonly TagLibIsolatedWriteTestRunner
        _commonRunner =
            new();

    /// <summary>
    /// Ejecuta la prueba aislada MP3 manteniendo el mismo tipo
    /// de resultado que ya utiliza la interfaz actual.
    /// </summary>
    public async Task<TagLibMp3IsolatedWriteTestResult>
        RunAsync(
            string? originalFilePath,
            string requestedGenre = "Electronic",
            CancellationToken cancellationToken = default)
    {
        TagLibIsolatedWriteTestResult commonResult =
            await _commonRunner.RunAsync(
                originalFilePath,
                writer:
                    new TagLibMp3MetadataWriter(),
                formatDisplayName:
                    "MP3",
                testFolderName:
                    "TagLibMp3WriteTests",
                requestedGenre:
                    requestedGenre,
                cancellationToken:
                    cancellationToken);

        return MapResult(
            commonResult);
    }

    /// <summary>
    /// Convierte el resultado común al modelo MP3 que ya usa
    /// MainWindow.xaml.cs y los diagnósticos existentes.
    /// </summary>
    private static TagLibMp3IsolatedWriteTestResult MapResult(
        TagLibIsolatedWriteTestResult source)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        return new TagLibMp3IsolatedWriteTestResult
        {
            OriginalFilePath =
                source.OriginalFilePath,

            WorkingCopyPath =
                source.WorkingCopyPath,

            WorkingBackupPath =
                source.WorkingBackupPath,

            TestDirectoryPath =
                source.TestDirectoryPath,

            OriginalGenre =
                source.OriginalGenre,

            RequestedGenre =
                source.RequestedGenre,

            PersistedGenre =
                source.PersistedGenre,

            PictureCountBefore =
                source.PictureCountBefore,

            PictureCountAfter =
                source.PictureCountAfter,

            OriginalHashBefore =
                source.OriginalHashBefore,

            OriginalHashAfter =
                source.OriginalHashAfter,

            WorkingCopyHashBefore =
                source.WorkingCopyHashBefore,

            WorkingCopyHashAfter =
                source.WorkingCopyHashAfter,

            WorkingBackupHash =
                source.WorkingBackupHash,

            WriteResult =
                source.WriteResult,

            Messages =
                source.Messages.ToArray()
        };
    }
}