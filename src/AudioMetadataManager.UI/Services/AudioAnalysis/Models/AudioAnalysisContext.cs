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
    /// Información técnica declarada o identificada desde
    /// el archivo, su contenedor y su códec.
    ///
    /// Este objeto es independiente de AudioStreamInfo, que
    /// representa el flujo realmente decodificado.
    /// </summary>
    public AudioTechnicalFormatInfo? TechnicalFormatInfo { get; set; }

    /// <summary>
    /// Indica si existe información válida del flujo PCM.
    /// </summary>
    public bool HasValidStreamInfo =>
        StreamInfo is not null &&
        StreamInfo.IsValid;

    /// <summary>
    /// Indica si existe información técnica declarada o
    /// identificada utilizable.
    /// </summary>
    public bool HasValidTechnicalFormatInfo =>
        TechnicalFormatInfo is not null &&
        TechnicalFormatInfo.IsValid;

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
    /// Almacena datos compartidos identificados por su tipo.
    ///
    /// Permite que un procesador publique información y que
    /// otros módulos la reutilicen sin depender de claves
    /// de texto ni conocer la implementación del productor.
    /// </summary>
    private IDictionary<Type, object> TypedData { get; } =
        new Dictionary<Type, object>();

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
    /// Guarda o reemplaza un dato compartido utilizando
    /// su tipo como identificador.
    /// </summary>
    public void SetData<T>(
        T value)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(
            value);

        TypedData[typeof(T)] =
            value;
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

    /// <summary>
    /// Intenta recuperar un dato compartido utilizando
    /// su tipo como identificador.
    /// </summary>
    public bool TryGetData<T>(
        out T? value)
        where T : class
    {
        value =
            default;

        if (!TypedData.TryGetValue(
                typeof(T),
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

    /// <summary>
    /// Indica si existe un dato compartido del tipo solicitado.
    /// </summary>
    public bool HasData<T>()
        where T : class
    {
        return TypedData.ContainsKey(
            typeof(T));
    }

    /// <summary>
    /// Recupera un dato compartido obligatorio.
    ///
    /// Genera una excepción clara cuando el dato aún no fue
    /// publicado por una etapa anterior del pipeline.
    /// </summary>
    public T GetRequiredData<T>()
        where T : class
    {
        if (TryGetData<T>(
                out T? value) &&
            value is not null)
        {
            return value;
        }

        throw new InvalidOperationException(
            $"No existe información compartida del tipo " +
            $"\"{typeof(T).Name}\" dentro del contexto.");
    }

    /// <summary>
    /// Elimina un dato compartido del tipo solicitado.
    /// </summary>
    public bool RemoveData<T>()
        where T : class
    {
        return TypedData.Remove(
            typeof(T));
    }
}