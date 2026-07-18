using AudioMetadataManager.UI.Services.AudioAnalysis.Models;
using AudioMetadataManager.UI.Services.AudioAnalysis.Reporting;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Diagnostics;

/// <summary>
/// Ejecuta una prueba controlada del AudioAnalysisEngine
/// sobre un único archivo de audio.
///
/// Esta clase se utiliza durante el desarrollo para comprobar:
///
/// - que NAudio pueda decodificar el archivo;
/// - que el flujo PCM sea válido;
/// - que AudioSilenceAnalyzer termine correctamente;
/// - que las duraciones calculadas sean coherentes;
/// - que las advertencias se registren correctamente.
///
/// No modifica el archivo analizado.
/// </summary>
public class AudioAnalysisTestRunner
{
    private readonly AudioAnalysisEngine _analysisEngine;
    private readonly AudioAnalysisReportBuilder _reportBuilder;

    /// <summary>
    /// Crea el ejecutor utilizando el motor predeterminado.
    /// </summary>
    public AudioAnalysisTestRunner()
        : this(new AudioAnalysisEngine())
    {
    }

    /// <summary>
    /// Crea el ejecutor utilizando un motor proporcionado
    /// externamente.
    ///
    /// Este constructor será útil para pruebas automatizadas
    /// o configuraciones especiales.
    /// </summary>
    public AudioAnalysisTestRunner(
        AudioAnalysisEngine analysisEngine)
    {
        _analysisEngine =
            analysisEngine ??
            throw new ArgumentNullException(
                nameof(analysisEngine));
        _reportBuilder =
            new AudioAnalysisReportBuilder();
    }

    /// <summary>
    /// Ejecuta el análisis de un archivo y devuelve tanto
    /// el resultado estructurado como el informe legible.
    /// </summary>
    public async Task<AudioAnalysisTestReport> RunAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        DateTime startedAt =
            DateTime.Now;

        AudioAnalysisResult analysisResult =
            await _analysisEngine.AnalyzeAsync(
                filePath,
                cancellationToken);

        DateTime completedAt =
            DateTime.Now;

        string reportText =
            _reportBuilder.Build(
                analysisResult);

        return new AudioAnalysisTestReport
        {
            FilePath =
                filePath?.Trim() ?? string.Empty,

            StartedAt =
                startedAt,

            CompletedAt =
                completedAt,

            AnalysisResult =
                analysisResult,

            ReportText =
                reportText
        };
    }
}

/// <summary>
/// Reúne el resultado estructurado y el texto generado
/// durante la prueba controlada.
/// </summary>
public class AudioAnalysisTestReport
{
    /// <summary>
    /// Ruta recibida por el ejecutor.
    /// </summary>
    public string FilePath { get; set; } =
        string.Empty;

    /// <summary>
    /// Momento en que comenzó la prueba.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Momento en que terminó la prueba.
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Resultado completo producido por AudioAnalysisEngine.
    /// </summary>
    public AudioAnalysisResult AnalysisResult { get; set; } =
        new();

    /// <summary>
    /// Informe legible para mostrar en la interfaz,
    /// guardar en un registro o copiar durante el desarrollo.
    /// </summary>
    public string ReportText { get; set; } =
        string.Empty;

    /// <summary>
    /// Tiempo total medido por el ejecutor de pruebas.
    /// </summary>
    public TimeSpan TotalElapsedTime
    {
        get
        {
            TimeSpan elapsed =
                CompletedAt - StartedAt;

            return elapsed < TimeSpan.Zero
                ? TimeSpan.Zero
                : elapsed;
        }
    }

    /// <summary>
    /// Indica si la prueba produjo un resultado válido
    /// y no terminó con un error fatal.
    /// </summary>
    public bool WasSuccessful =>
        AnalysisResult.AnalysisCompleted &&
        !AnalysisResult.HasFatalError &&
        !AnalysisResult.WasCancelled;
}