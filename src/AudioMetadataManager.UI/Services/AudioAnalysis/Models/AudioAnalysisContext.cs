using System.IO;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Contiene la información compartida durante la ejecución
/// del pipeline de análisis técnico y acústico.
///
/// El contexto permite que las distintas etapas consulten
/// información ya obtenida sin duplicar responsabilidades.
///
/// Esta primera versión no conserva todas las muestras PCM
/// en memoria. La reutilización eficiente del audio
/// decodificado se implementará posteriormente mediante
/// una estrategia de lectura o caché controlada.
/// </summary>
public class AudioAnalysisContext
{
    /// <summary>
    /// Crea un contexto para el archivo indicado.
    /// </summary>
    public AudioAnalysisContext(
        string filePath,
        AudioAnalysisResult analysisResult)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "La ruta del archivo no puede estar vacía.",
                nameof(filePath));
        }

        ArgumentNullException.ThrowIfNull(
            analysisResult);

        FilePath =
            filePath.Trim();

        AnalysisResult =
            analysisResult;
    }

    /// <summary>
    /// Ruta completa del archivo que está siendo analizado.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Nombre del archivo, incluyendo su extensión.
    /// </summary>
    public string FileName =>
        Path.GetFileName(FilePath);

    /// <summary>
    /// Extensión normalizada en minúsculas.
    /// </summary>
    public string Extension =>
        Path.GetExtension(FilePath)
            .ToLowerInvariant();

    /// <summary>
    /// Resultado general compartido por todas las etapas.
    /// </summary>
    public AudioAnalysisResult AnalysisResult { get; }

    /// <summary>
    /// Información técnica del flujo PCM.
    ///
    /// Podrá ser registrada por la primera etapa que lea
    /// correctamente el archivo y reutilizada por etapas
    /// posteriores.
    /// </summary>
    public AudioStreamInfo? StreamInfo { get; set; }

    /// <summary>
    /// Indica si existe información válida del flujo PCM.
    /// </summary>
    public bool HasValidStreamInfo =>
        StreamInfo is not null &&
        StreamInfo.IsValid;

    /// <summary>
    /// Fecha y hora en que se creó el contexto.
    /// </summary>
    public DateTime CreatedAt { get; } =
        DateTime.Now;

    /// <summary>
    /// Espacio compartido para datos auxiliares producidos
    /// por las etapas del pipeline.
    ///
    /// Se utiliza para información especializada que todavía
    /// no justifica una propiedad formal en este modelo.
    /// </summary>
    public IDictionary<string, object> Items { get; } =
        new Dictionary<string, object>(
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Guarda o reemplaza un valor auxiliar.
    /// </summary>
    public void SetItem<T>(
        string key,
        T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(
                "La clave no puede estar vacía.",
                nameof(key));
        }

        Items[key.Trim()] =
            value!;
    }

    /// <summary>
    /// Intenta recuperar un valor auxiliar del tipo indicado.
    /// </summary>
    public bool TryGetItem<T>(
        string key,
        out T? value)
    {
        value =
            default;

        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (!Items.TryGetValue(
                key.Trim(),
                out object? storedValue))
        {
            return false;
        }

        if (storedValue is not T typedValue)
        {
            return false;
        }

        value =
            typedValue;

        return true;
    }
}