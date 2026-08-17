using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Filtering.Models;

namespace AudioMetadataManager.UI.Services.Filtering;

public interface ILibraryFilterService
{
    /// <summary>
    /// Determina si un archivo de audio cumple con los criterios de filtrado y búsqueda.
    /// </summary>
    bool Matches(AudioFile file, LibraryFilterCriteria criteria);

    /// <summary>
    /// Filtra una colección de archivos de audio según los criterios especificados.
    /// </summary>
    IReadOnlyList<AudioFile> Filter(IEnumerable<AudioFile> files, LibraryFilterCriteria criteria);
}
