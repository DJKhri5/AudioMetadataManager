namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Representa el resultado general del análisis técnico
/// y acústico de un archivo de audio.
///
/// Cada tipo de análisis mantiene su propio resultado.
/// Esta clase solamente los reúne y entrega un estado global.
/// </summary>
public class AudioAnalysisResult
{
    /// <summary>
    /// Ruta completa del archivo analizado.
    /// </summary>
    public string FilePath { get; set; } =
        string.Empty;

    /// <summary>
    /// Nombre del archivo analizado.
    /// </summary>
    public string FileName { get; set; } =
        string.Empty;

    /// <summary>
    /// Fecha y hora de inicio del análisis.
    /// </summary>
    public DateTime StartedAt { get; set; } =
        DateTime.Now;

    /// <summary>
    /// Fecha y hora de finalización del análisis.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Resultado del análisis de silencio inicial y final.
    /// </summary>
    public AudioSilenceAnalysisResult Silence { get; set; } =
        new();

    /// <summary>
    /// Resultado del análisis de envolvente energética.
    /// </summary>
    public AudioEnvelopeAnalysisResult Envelope { get; set; } =
        new();

    /// <summary>
    /// Resultado resumido del análisis espectral.
    /// </summary>
    public AudioSpectrumAnalysisResult Spectrum { get; set; } =
        new();

    /// <summary>
    /// Medición objetiva de la extensión y caída superior
    /// del espectro ya analizado.
    /// </summary>
    public AudioSpectrumCutoffMeasurement SpectrumCutoff { get; set; } =
        new();

    /// <summary>
    /// Perfil tonal derivado del perfil espectral.
    ///
    /// Este objeto reutiliza los bins FFT ya calculados.
    /// No implica otra lectura del archivo ni una segunda FFT.
    /// </summary>
    public AudioToneProfile ToneProfile { get; set; } =
        new();

    /// <summary>
    /// Perfil de balance tonal derivado del perfil tonal.
    ///
    /// Agrupa la energía en regiones bajas, medias y altas
    /// sin volver a leer el archivo ni ejecutar otra FFT.
    /// </summary>
    public AudioToneBalanceProfile ToneBalanceProfile { get; set; } =
        new();

    /// <summary>
    /// Caracterización tonal simplificada del archivo.
    /// </summary>
    public AudioToneCharacterResult ToneCharacterResult { get; set; } =
        new();

    /// <summary>
    /// Información técnica declarada o identificada desde
    /// el archivo, su contenedor y su códec.
    ///
    /// Incluye datos como extensión, bitrate informado,
    /// frecuencia de muestreo, canales y profundidad de bits.
    /// </summary>
    public AudioTechnicalFormatInfo TechnicalFormat { get; set; } =
        new();

    /// <summary>
    /// Resultado general producido por el motor de evaluación
    /// técnica de calidad.
    ///
    /// Este objeto reúne las conclusiones de las reglas
    /// aplicables sin volver a leer ni procesar el archivo.
    /// </summary>
    public AudioQualityAnalysisResult Quality { get; set; } =
        new();

    /// <summary>
    /// Indica si el motor terminó de ejecutar todos los
    /// analizadores habilitados.
    /// </summary>
    public bool AnalysisCompleted { get; set; }

    /// <summary>
    /// Indica si el análisis fue cancelado por el usuario.
    /// </summary>
    public bool WasCancelled { get; set; }

    /// <summary>
    /// Indica si ocurrió un error general que impidió
    /// continuar con el análisis.
    /// </summary>
    public bool HasFatalError { get; set; }

    /// <summary>
    /// Mensaje del error general.
    ///
    /// Los errores específicos de cada módulo deben quedar
    /// guardados en el resultado de ese analizador.
    /// </summary>
    public string ErrorMessage { get; set; } =
        string.Empty;

    /// <summary>
    /// Advertencias generales producidas durante el análisis.
    /// </summary>
    public List<string> Warnings { get; set; } =
        new();

    /// <summary>
    /// Explicación general del resultado.
    /// </summary>
    public string Summary { get; set; } =
        string.Empty;

    /// <summary>
    /// Tiempo total empleado por el motor.
    /// </summary>
    public TimeSpan ElapsedTime
    {
        get
        {
            DateTime end =
                CompletedAt ?? DateTime.Now;

            TimeSpan elapsed =
                end - StartedAt;

            return elapsed < TimeSpan.Zero
                ? TimeSpan.Zero
                : elapsed;
        }
    }

    /// <summary>
    /// Indica si el análisis de silencio pudo completarse.
    /// </summary>
    public bool HasSilenceAnalysis =>
        Silence.AnalysisCompleted;

    /// <summary>
    /// En la versión actual solamente un error grave o una
    /// comparación futura podrán solicitar revisión manual.
    ///
    /// El análisis de silencio ya no toma esa decisión.
    /// </summary>
    public bool RequiresManualReview =>
        false;

    /// <summary>
    /// Indica si existe algún problema registrado.
    /// </summary>
    public bool HasProblems =>
        HasFatalError ||
        Silence.HasError;

    /// <summary>
    /// Cantidad de advertencias generales.
    /// </summary>
    public int WarningCount =>
        Warnings.Count;

    /// <summary>
    /// Texto legible con el tiempo empleado.
    /// </summary>
    public string ElapsedTimeDisplay =>
        FormatElapsedTime(ElapsedTime);

    /// <summary>
    /// Estado global del análisis.
    /// </summary>
    public string StatusDisplay
    {
        get
        {
            if (WasCancelled)
            {
                return "Cancelado";
            }

            if (HasFatalError)
            {
                return "Error";
            }

            if (!AnalysisCompleted)
            {
                return "En proceso";
            }

            if (RequiresManualReview)
            {
                return "Revisión recomendada";
            }

            return "Análisis completado";
        }
    }

    /// <summary>
    /// Resumen automático para la interfaz.
    ///
    /// Si Summary ya contiene un texto personalizado,
    /// ese texto tendrá prioridad.
    /// </summary>
    public string SummaryDisplay
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Summary))
            {
                return Summary;
            }

            if (WasCancelled)
            {
                return
                    "El análisis fue cancelado antes de finalizar.";
            }

            if (HasFatalError)
            {
                return string.IsNullOrWhiteSpace(ErrorMessage)
                    ? "El análisis terminó con un error."
                    : ErrorMessage;
            }

            if (!AnalysisCompleted)
            {
                return
                    "El análisis todavía no ha finalizado.";
            }

            if (WarningCount > 0)
            {
                return
                    $"Análisis completado con " +
                    $"{WarningCount} advertencia(s).";
            }

            return
                "El análisis técnico finalizó correctamente. " +
                "Las mediciones obtenidas están disponibles para comparación con otras fuentes.";
        }
    }

    /// <summary>
    /// Registra la finalización del análisis.
    /// </summary>
    public void MarkAsCompleted()
    {
        CompletedAt = DateTime.Now;
        AnalysisCompleted = true;
        WasCancelled = false;
    }

    /// <summary>
    /// Registra que el usuario canceló el análisis.
    /// </summary>
    public void MarkAsCancelled()
    {
        CompletedAt = DateTime.Now;
        AnalysisCompleted = false;
        WasCancelled = true;
    }

    /// <summary>
    /// Registra un error general.
    /// </summary>
    public void MarkAsFailed(string? errorMessage)
    {
        CompletedAt = DateTime.Now;
        AnalysisCompleted = false;
        WasCancelled = false;
        HasFatalError = true;

        ErrorMessage =
            string.IsNullOrWhiteSpace(errorMessage)
                ? "Se produjo un error no especificado."
                : errorMessage.Trim();
    }

    /// <summary>
    /// Agrega una advertencia evitando duplicados.
    /// </summary>
    public void AddWarning(string? warning)
    {
        if (string.IsNullOrWhiteSpace(warning))
        {
            return;
        }

        string normalizedWarning =
            warning.Trim();

        if (Warnings.Contains(
                normalizedWarning,
                StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        Warnings.Add(normalizedWarning);
    }

    /// <summary>
    /// Formatea el tiempo empleado.
    /// </summary>
    private static string FormatElapsedTime(
        TimeSpan elapsed)
    {
        if (elapsed.TotalHours >= 1)
        {
            return elapsed.ToString(
                @"h\:mm\:ss\.fff");
        }

        if (elapsed.TotalMinutes >= 1)
        {
            return elapsed.ToString(
                @"m\:ss\.fff");
        }

        return
            $"{elapsed.TotalSeconds:0.000} s";
    }
}