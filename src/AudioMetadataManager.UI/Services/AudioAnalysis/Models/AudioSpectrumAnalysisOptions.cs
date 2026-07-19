namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Configuración técnica utilizada por el algoritmo
/// de análisis espectral.
///
/// Estos parámetros controlan la construcción de ventanas FFT
/// y la detección descriptiva de contenido frecuencial.
/// No determinan por sí solos la calidad del archivo.
/// </summary>
public class AudioSpectrumAnalysisOptions
{
    /// <summary>
    /// Tamaño de la transformada rápida de Fourier.
    ///
    /// Debe ser una potencia de dos.
    /// Con 4096 muestras y 44.100 Hz se obtiene una resolución
    /// aproximada de 10,77 Hz por bin.
    /// </summary>
    public int FftSize { get; set; } =
        4096;

    /// <summary>
    /// Solapamiento entre ventanas FFT consecutivas.
    ///
    /// El valor debe encontrarse entre 0 y menos de 1.
    /// </summary>
    public double WindowOverlap { get; set; } =
        0.50;

    /// <summary>
    /// Nivel mínimo que debe alcanzar un bin para considerarse
    /// contenido energético significativo.
    ///
    /// Esta medición es descriptiva y no constituye todavía
    /// un diagnóstico de corte espectral.
    /// </summary>
    public double SignificantEnergyThresholdDb { get; set; } =
        -75.0;

    /// <summary>
    /// Umbral lineal utilizado para contar en cuántas ventanas
    /// cada bin contiene energía significativa.
    ///
    /// Esta medición permite diferenciar contenido persistente
    /// de ruido, artefactos aislados o energía residual.
    /// </summary>
    public double SignificantMagnitudeThreshold { get; set; } =
        0.001;

    /// <summary>
    /// Proporción mínima de ventanas en las que un bin debe
    /// superar el umbral significativo para considerarse
    /// contenido espectral persistente.
    ///
    /// Los valores válidos se encuentran entre 0 y 1.
    /// </summary>
    public double MinimumSignificantWindowRatio { get; set; } =
        0.05;

    /// <summary>
    /// Proporción mínima de ventanas necesaria para considerar
    /// que una frecuencia presenta contenido fuertemente
    /// persistente.
    ///
    /// Esta medición complementa la persistencia mínima y no
    /// reemplaza el análisis del espectro medio.
    /// </summary>
    public double MinimumStrongWindowRatio { get; set; } =
        0.25;

    /// <summary>
    /// Nivel mínimo utilizado para estudiar la zona superior
    /// del espectro al estimar una caída persistente.
    /// </summary>
    public double HighFrequencyRolloffThresholdDb { get; set; } =
        -60.0;

    /// <summary>
    /// Frecuencia mínima desde la cual podrá buscarse una
    /// caída persistente de altas frecuencias.
    ///
    /// Evita interpretar como rolloff las variaciones normales
    /// de las frecuencias medias.
    /// </summary>
    public double MinimumRolloffSearchFrequencyHz { get; set; } =
        8000.0;

    /// <summary>
    /// Cantidad mínima de ventanas FFT procesadas para
    /// considerar confiable el resultado global.
    /// </summary>
    public int MinimumProcessedWindows { get; set; } =
        10;

    /// <summary>
    /// Cantidad de ventanas FFT que podrán omitirse entre
    /// ventanas procesadas.
    ///
    /// Un valor de cero analiza todas las ventanas.
    /// Más adelante podrá utilizarse para acelerar el análisis
    /// de bibliotecas grandes sin volver a leer el archivo.
    /// </summary>
    public int SkippedWindowsBetweenAnalyses { get; set; } =
        0;

    /// <summary>
    /// Límite inferior utilizado para representar valores
    /// espectrales extremadamente pequeños.
    /// </summary>
    public double MinimumDecibels { get; set; } =
        -120.0;

    /// <summary>
    /// Valida todos los parámetros técnicos.
    /// </summary>
    public void Validate()
    {
        if (FftSize < 256 ||
            FftSize > 65_536)
        {
            throw new ArgumentOutOfRangeException(
                nameof(FftSize),
                FftSize,
                "El tamaño FFT debe encontrarse entre " +
                "256 y 65.536 muestras.");
        }

        if (!IsPowerOfTwo(FftSize))
        {
            throw new ArgumentException(
                "El tamaño FFT debe ser una potencia de dos.",
                nameof(FftSize));
        }

        if (WindowOverlap < 0 ||
            WindowOverlap >= 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(WindowOverlap),
                WindowOverlap,
                "El solapamiento debe encontrarse entre " +
                "0 y un valor menor que 1.");
        }

        if (SignificantEnergyThresholdDb >= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SignificantEnergyThresholdDb),
                SignificantEnergyThresholdDb,
                "El umbral de energía significativa debe " +
                "ser menor que 0 dBFS.");
        }

        if (SignificantMagnitudeThreshold <= 0 ||
            SignificantMagnitudeThreshold > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SignificantMagnitudeThreshold),
                SignificantMagnitudeThreshold,
                "El umbral lineal de magnitud significativa debe " +
                "ser mayor que cero y menor o igual que uno.");
        }

        if (MinimumSignificantWindowRatio <= 0 ||
            MinimumSignificantWindowRatio > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumSignificantWindowRatio),
                MinimumSignificantWindowRatio,
                "La proporción mínima de persistencia debe ser " +
                "mayor que cero y menor o igual que uno.");
        }

        if (MinimumStrongWindowRatio <= 0 ||
            MinimumStrongWindowRatio > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumStrongWindowRatio),
                MinimumStrongWindowRatio,
                "La proporción de persistencia fuerte debe ser " +
                "mayor que cero y menor o igual que uno.");
        }

        if (MinimumStrongWindowRatio <
            MinimumSignificantWindowRatio)
        {
            throw new ArgumentException(
                "La persistencia fuerte no puede ser menor que " +
                "la persistencia mínima.",
                nameof(MinimumStrongWindowRatio));
        }

        if (HighFrequencyRolloffThresholdDb >= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(HighFrequencyRolloffThresholdDb),
                HighFrequencyRolloffThresholdDb,
                "El umbral de rolloff debe ser menor " +
                "que 0 dBFS.");
        }

        if (MinimumRolloffSearchFrequencyHz < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumRolloffSearchFrequencyHz),
                MinimumRolloffSearchFrequencyHz,
                "La frecuencia mínima de búsqueda no " +
                "puede ser negativa.");
        }

        if (MinimumProcessedWindows < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumProcessedWindows),
                MinimumProcessedWindows,
                "Debe procesarse al menos una ventana FFT.");
        }

        if (SkippedWindowsBetweenAnalyses < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SkippedWindowsBetweenAnalyses),
                SkippedWindowsBetweenAnalyses,
                "La cantidad de ventanas omitidas no puede " +
                "ser negativa.");
        }

        if (MinimumDecibels >= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumDecibels),
                MinimumDecibels,
                "El límite inferior debe ser menor " +
                "que 0 dBFS.");
        }
    }

    /// <summary>
    /// Comprueba si un número entero es una potencia de dos.
    /// </summary>
    private static bool IsPowerOfTwo(
        int value)
    {
        return value > 0 &&
            (value & (value - 1)) == 0;
    }
}