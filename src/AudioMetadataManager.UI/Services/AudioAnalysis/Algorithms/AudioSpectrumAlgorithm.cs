using AudioMetadataManager.UI.Services.AudioAnalysis.Models;
using NAudio.Dsp;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;

/// <summary>
/// Calcula un perfil espectral acumulado mediante FFT
/// utilizando bloques PCM previamente decodificados.
///
/// Esta clase no abre archivos ni modifica audio.
/// El perfil generado puede ser reutilizado por el detector
/// de bitrate efectivo, el motor de calidad y el comparador
/// técnico de duplicados.
/// </summary>
public class AudioSpectrumAlgorithm
{
    private readonly AudioSpectrumAnalysisOptions _options;

    private AudioSpectrumAnalysisResult _result =
        new();

    private AudioSpectrumProfile _profile =
        new();

    private AudioStreamInfo? _streamInfo;
    private AudioSpectrumProcessingState? _state;

    private bool _isInitialized;
    private bool _isCompleted;

    /// <summary>
    /// Crea el algoritmo con la configuración indicada.
    /// </summary>
    public AudioSpectrumAlgorithm(
        AudioSpectrumAnalysisOptions? options = null)
    {
        _options =
            options ??
            new AudioSpectrumAnalysisOptions();

        _options.Validate();
    }

    /// <summary>
    /// Resultado resumido del análisis espectral.
    /// </summary>
    public AudioSpectrumAnalysisResult Result =>
        _result;

    /// <summary>
    /// Perfil espectral reutilizable producido por
    /// el algoritmo.
    /// </summary>
    public AudioSpectrumProfile Profile =>
        _profile;

    /// <summary>
    /// Prepara el algoritmo antes de recibir bloques PCM.
    /// </summary>
    public void Initialize(
        AudioStreamInfo streamInfo)
    {
        ArgumentNullException.ThrowIfNull(
            streamInfo);

        if (!streamInfo.IsValid)
        {
            throw new ArgumentException(
                "La información del flujo PCM no es válida.",
                nameof(streamInfo));
        }

        _streamInfo =
            streamInfo;

        _state =
            new AudioSpectrumProcessingState(
                streamInfo,
                _options);

        _result =
            new AudioSpectrumAnalysisResult
            {
                TechnicalDuration =
                    streamInfo.DecodedDuration,

                SampleRate =
                    streamInfo.SampleRate,

                FftSize =
                    _options.FftSize,

                WindowDuration =
                    _state.WindowDuration
            };

        _profile =
            new AudioSpectrumProfile();

        _isInitialized =
            true;

        _isCompleted =
            false;
    }

    /// <summary>
    /// Recibe un bloque PCM perteneciente a la lectura
    /// compartida y agrega sus muestras al análisis.
    /// </summary>
    public void ProcessBlock(
        AudioSampleBlock block)
    {
        ArgumentNullException.ThrowIfNull(
            block);

        EnsureReadyForProcessing();

        if (!block.IsValid)
        {
            return;
        }

        AudioSpectrumProcessingState state =
            _state!;

        if (block.Channels !=
            state.Channels)
        {
            throw new InvalidOperationException(
                "La cantidad de canales del bloque PCM " +
                "no coincide con la información del flujo.");
        }

        AppendMonoSamples(
            block,
            state);

        state.TotalReceivedFrames +=
            block.FrameCount;

        ProcessAvailableWindows(
            state);
    }

    /// <summary>
    /// Finaliza el análisis y construye tanto el perfil
    /// reutilizable como el resultado resumido.
    /// </summary>
    public AudioSpectrumAnalysisResult Complete()
    {
        EnsureReadyForProcessing();

        AudioSpectrumProcessingState state =
            _state!;

        if (!state.HasProcessedData)
        {
            return Fail(
                "No se obtuvieron ventanas FFT utilizables.");
        }

        double[] averageMagnitudeDb =
            ConvertAverageMagnitudesToDecibels(
                state);

        double[] peakMagnitudeDb =
            ConvertPeakMagnitudesToDecibels(
                state);

        double[] significantWindowRatios =
            CalculateSignificantWindowRatios(
                state);

        _profile =
            new AudioSpectrumProfile
            {
                SampleRate =
                    state.SampleRate,

                FftSize =
                    state.FftSize,

                ProcessedWindowCount =
                    state.ProcessedWindowCount,

                FrequenciesHz =
                    Array.AsReadOnly(
                        (double[])state.FrequenciesHz.Clone()),

                AverageMagnitudeDb =
                    Array.AsReadOnly(
                        averageMagnitudeDb),

                PeakMagnitudeDb =
                    Array.AsReadOnly(
                        peakMagnitudeDb),

                SignificantWindowRatios =
                    Array.AsReadOnly(
                        significantWindowRatios)
            };

        _result.ProcessedWindowCount =
            state.ProcessedWindowCount;

        _result.AverageSpectrumEnergyDb =
            CalculateAverageSpectrumEnergyDb(
                averageMagnitudeDb);

        _result.PeakSpectrumEnergyDb =
            peakMagnitudeDb.Max();

        _result.HighestSignificantFrequencyHz =
            FindHighestSignificantFrequency(
                state.FrequenciesHz,
                peakMagnitudeDb,
                _options.SignificantEnergyThresholdDb);

        _result.HighestPersistentFrequencyHz =
            FindHighestPersistentFrequency(
                state.FrequenciesHz,
                significantWindowRatios,
                _options.MinimumSignificantWindowRatio);

        _result.HighestStrongPersistentFrequencyHz =
            FindHighestPersistentFrequency(
                state.FrequenciesHz,
                significantWindowRatios,
                _options.MinimumStrongWindowRatio);

        _result.EstimatedHighFrequencyRolloffHz =
            EstimateHighFrequencyRolloff(
                state.FrequenciesHz,
                averageMagnitudeDb,
                _options.MinimumRolloffSearchFrequencyHz,
                _options.HighFrequencyRolloffThresholdDb);

        _result.AnalysisCompleted =
            true;

        _result.IsReliable =
            state.ProcessedWindowCount >=
            _options.MinimumProcessedWindows &&
            _profile.IsValid;

        _result.Summary =
            BuildSummary(
                _result);

        _isCompleted =
            true;

        return _result;
    }

    /// <summary>
    /// Registra un error controlado.
    /// </summary>
    public AudioSpectrumAnalysisResult Fail(
        string? errorMessage)
    {
        _result.AnalysisCompleted =
            false;

        _result.IsReliable =
            false;

        _result.ErrorMessage =
            string.IsNullOrWhiteSpace(errorMessage)
                ? "El análisis espectral no pudo completarse."
                : errorMessage.Trim();

        _result.Summary =
            "El análisis espectral terminó con un error.";

        _profile =
            new AudioSpectrumProfile();

        _isCompleted =
            true;

        return _result;
    }

    /// <summary>
    /// Convierte los frames intercalados del bloque PCM
    /// en una señal mono para el análisis espectral.
    /// </summary>
    private static void AppendMonoSamples(
        AudioSampleBlock block,
        AudioSpectrumProcessingState state)
    {
        for (int frameIndex = 0;
            frameIndex < block.FrameCount;
            frameIndex++)
        {
            int firstSampleIndex =
                frameIndex *
                block.Channels;

            double sum = 0;
            int validChannels = 0;

            for (int channelIndex = 0;
                channelIndex < block.Channels;
                channelIndex++)
            {
                int sampleIndex =
                    firstSampleIndex +
                    channelIndex;

                if (sampleIndex >=
                    block.Samples.Length)
                {
                    break;
                }

                sum +=
                    block.Samples[sampleIndex];

                validChannels++;
            }

            if (validChannels == 0)
            {
                continue;
            }

            state.PendingMonoSamples.Add(
                (float)(sum / validChannels));
        }
    }

    /// <summary>
    /// Procesa todas las ventanas FFT completas actualmente
    /// disponibles en el buffer de muestras.
    /// </summary>
    private void ProcessAvailableWindows(
        AudioSpectrumProcessingState state)
    {
        while (state.PendingMonoSamples.Count >=
            state.FftSize)
        {
            state.AvailableWindowCount++;

            if (state.ShouldProcessWindow(
                    _options.SkippedWindowsBetweenAnalyses))
            {
                ProcessWindow(
                    state);

                state.ProcessedWindowCount++;
            }
            else
            {
                state.SkippedWindowCount++;
            }

            state.PendingMonoSamples.RemoveRange(
                0,
                Math.Min(
                    state.HopSize,
                    state.PendingMonoSamples.Count));

            state.NextWindowIndex++;
        }
    }

    /// <summary>
    /// Aplica ventana Hann, ejecuta FFT y acumula
    /// las magnitudes positivas.
    /// </summary>
    private void ProcessWindow(
        AudioSpectrumProcessingState state)
    {
        Complex[] fftBuffer =
            new Complex[state.FftSize];

        double coherentGain =
            0.5;

        for (int index = 0;
            index < state.FftSize;
            index++)
        {
            double windowValue =
                0.5 -
                0.5 *
                Math.Cos(
                    2.0 *
                    Math.PI *
                    index /
                    (state.FftSize - 1));

            double sample =
                state.PendingMonoSamples[index];

            state.WindowBuffer[index] =
                sample *
                windowValue;

            fftBuffer[index].X =
                (float)state.WindowBuffer[index];

            fftBuffer[index].Y =
                0;
        }

        int fftExponent =
            CalculateFftExponent(
                state.FftSize);

        FastFourierTransform.FFT(
            true,
            fftExponent,
            fftBuffer);

        /*
        * NAudio normaliza la FFT directa por el tamaño de la
        * transformación. Por esa razón aquí solo debemos compensar
        * la ganancia coherente de la ventana Hann.
        *
        * Dividir nuevamente por FftSize desplazaría todo el espectro
        * aproximadamente 72 dB hacia abajo para una FFT de 4096.
        */
        double normalization =
            coherentGain;

        for (int binIndex = 0;
            binIndex < state.PositiveBinCount;
            binIndex++)
        {
            double real =
                fftBuffer[binIndex].X;

            double imaginary =
                fftBuffer[binIndex].Y;

            double magnitude =
                Math.Sqrt(
                    real * real +
                    imaginary * imaginary) /
                normalization;

            /*
             * Se duplica la magnitud de los bins interiores
             * porque utilizamos un espectro de un solo lado.
             * DC y Nyquist no se duplican.
             */
            if (binIndex > 0 &&
                binIndex <
                state.PositiveBinCount - 1)
            {
                magnitude *=
                    2.0;
            }

            state.AverageMagnitudeLinearSums[binIndex] +=
                magnitude;

            if (magnitude >
                state.PeakMagnitudeLinear[binIndex])
            {
                state.PeakMagnitudeLinear[binIndex] =
                    magnitude;
            }

            if (magnitude >=
                _options.SignificantMagnitudeThreshold)
            {
                state.SignificantWindowCounts[binIndex]++;
            }
        }
    }

    /// <summary>
    /// Convierte los promedios lineales por bin a dBFS.
    /// </summary>
    private double[] ConvertAverageMagnitudesToDecibels(
        AudioSpectrumProcessingState state)
    {
        double[] values =
            new double[state.PositiveBinCount];

        for (int index = 0;
            index < values.Length;
            index++)
        {
            double average =
                state.AverageMagnitudeLinearSums[index] /
                state.ProcessedWindowCount;

            values[index] =
                ConvertAmplitudeToDecibels(
                    average);
        }

        return values;
    }

    /// <summary>
    /// Convierte las magnitudes máximas por bin a dBFS.
    /// </summary>
    private double[] ConvertPeakMagnitudesToDecibels(
        AudioSpectrumProcessingState state)
    {
        double[] values =
            new double[state.PositiveBinCount];

        for (int index = 0;
            index < values.Length;
            index++)
        {
            values[index] =
                ConvertAmplitudeToDecibels(
                    state.PeakMagnitudeLinear[index]);
        }

        return values;
    }

    /// <summary>
    /// Convierte los contadores de persistencia por bin
    /// en proporciones comprendidas entre 0 y 1.
    /// </summary>
    private static double[] CalculateSignificantWindowRatios(
        AudioSpectrumProcessingState state)
    {
        double[] ratios =
            new double[state.PositiveBinCount];

        if (state.ProcessedWindowCount <= 0)
        {
            return ratios;
        }

        for (int index = 0;
            index < ratios.Length;
            index++)
        {
            ratios[index] =
                Math.Clamp(
                    (double)state.SignificantWindowCounts[index] /
                    state.ProcessedWindowCount,
                    0,
                    1);
        }

        return ratios;
    }

    /// <summary>
    /// Convierte una amplitud lineal a dBFS respetando
    /// el límite inferior configurado.
    /// </summary>
    private double ConvertAmplitudeToDecibels(
        double amplitude)
    {
        if (amplitude <= 0 ||
            double.IsNaN(amplitude) ||
            double.IsInfinity(amplitude))
        {
            return _options.MinimumDecibels;
        }

        return Math.Max(
            _options.MinimumDecibels,
            20.0 *
            Math.Log10(
                amplitude));
    }

    /// <summary>
    /// Calcula la energía espectral media global.
    /// </summary>
    private double CalculateAverageSpectrumEnergyDb(
        IReadOnlyList<double> averageMagnitudesDb)
    {
        if (averageMagnitudesDb.Count == 0)
        {
            return _options.MinimumDecibels;
        }

        double linearSum = 0;

        foreach (double valueDb in averageMagnitudesDb)
        {
            linearSum +=
                Math.Pow(
                    10.0,
                    valueDb / 20.0);
        }

        double averageLinear =
            linearSum /
            averageMagnitudesDb.Count;

        return ConvertAmplitudeToDecibels(
            averageLinear);
    }

    /// <summary>
    /// Encuentra la frecuencia más alta que supera
    /// el umbral energético configurado.
    /// </summary>
    private static double FindHighestSignificantFrequency(
        IReadOnlyList<double> frequenciesHz,
        IReadOnlyList<double> peakMagnitudesDb,
        double thresholdDb)
    {
        int count =
            Math.Min(
                frequenciesHz.Count,
                peakMagnitudesDb.Count);

        for (int index = count - 1;
            index >= 0;
            index--)
        {
            if (peakMagnitudesDb[index] >=
                thresholdDb)
            {
                return frequenciesHz[index];
            }
        }

        return 0;
    }

    /// <summary>
    /// Encuentra la frecuencia más alta cuya energía significativa
    /// aparece en una proporción mínima de las ventanas.
    /// </summary>
    private static double FindHighestPersistentFrequency(
        IReadOnlyList<double> frequenciesHz,
        IReadOnlyList<double> significantWindowRatios,
        double minimumRatio)
    {
        int count =
            Math.Min(
                frequenciesHz.Count,
                significantWindowRatios.Count);

        for (int index = count - 1;
            index >= 0;
            index--)
        {
            if (significantWindowRatios[index] >=
                minimumRatio)
            {
                return frequenciesHz[index];
            }
        }

        return 0;
    }

    /// <summary>
    /// Estima el primer punto de la zona alta desde el cual
    /// la energía permanece por debajo del umbral configurado.
    /// </summary>
    private static double EstimateHighFrequencyRolloff(
        IReadOnlyList<double> frequenciesHz,
        IReadOnlyList<double> averageMagnitudesDb,
        double minimumSearchFrequencyHz,
        double thresholdDb)
    {
        int count =
            Math.Min(
                frequenciesHz.Count,
                averageMagnitudesDb.Count);

        if (count == 0)
        {
            return 0;
        }

        int minimumPersistentBins =
            Math.Max(
                3,
                count / 200);

        for (int index = 0;
            index < count;
            index++)
        {
            if (frequenciesHz[index] <
                minimumSearchFrequencyHz)
            {
                continue;
            }

            if (averageMagnitudesDb[index] >
                thresholdDb)
            {
                continue;
            }

            bool remainsBelowThreshold =
                true;

            int endIndex =
                Math.Min(
                    count,
                    index +
                    minimumPersistentBins);

            for (int checkIndex = index;
                checkIndex < endIndex;
                checkIndex++)
            {
                if (averageMagnitudesDb[checkIndex] >
                    thresholdDb)
                {
                    remainsBelowThreshold =
                        false;

                    break;
                }
            }

            if (remainsBelowThreshold)
            {
                return frequenciesHz[index];
            }
        }

        return 0;
    }

    /// <summary>
    /// Calcula el exponente requerido por la FFT de NAudio.
    /// </summary>
    private static int CalculateFftExponent(
        int fftSize)
    {
        int exponent = 0;
        int value = fftSize;

        while (value > 1)
        {
            value >>= 1;
            exponent++;
        }

        return exponent;
    }

    /// <summary>
    /// Comprueba que el algoritmo pueda procesar
    /// o finalizar bloques.
    /// </summary>
    private void EnsureReadyForProcessing()
    {
        if (!_isInitialized ||
            _streamInfo is null ||
            _state is null)
        {
            throw new InvalidOperationException(
                "El algoritmo espectral no fue inicializado.");
        }

        if (_isCompleted)
        {
            throw new InvalidOperationException(
                "El algoritmo espectral ya fue finalizado.");
        }
    }

    /// <summary>
    /// Construye un resumen descriptivo.
    /// </summary>
    private static string BuildSummary(
        AudioSpectrumAnalysisResult result)
    {
        List<string> details = new()
        {
            $"Duración técnica: " +
            $"{result.TechnicalDurationDisplay}",

            $"Frecuencia de muestreo: " +
            $"{result.SampleRate} Hz",

            $"Tamaño FFT: " +
            $"{result.FftSize}",

            $"Resolución frecuencial: " +
            $"{result.FrequencyResolutionDisplay}",

            $"Ventanas procesadas: " +
            $"{result.ProcessedWindowCount}",

            $"Frecuencia significativa más alta: " +
            $"{result.HighestSignificantFrequencyDisplay}",

            $"Frecuencia persistente más alta: " +
            $"{result.HighestPersistentFrequencyDisplay}",

            $"Frecuencia con persistencia fuerte: " +
            $"{result.HighestStrongPersistentFrequencyDisplay}",

            $"Caída superior estimada: " +
            $"{result.EstimatedHighFrequencyRolloffDisplay}",

            $"Energía espectral media: " +
            $"{result.AverageSpectrumEnergyDisplay}",

            $"Energía espectral máxima: " +
            $"{result.PeakSpectrumEnergyDisplay}"
        };

        if (result.HasComparisonData)
        {
            details.Add(
                "Perfil disponible para otros módulos");
        }

        return string.Join(
            " · ",
            details);
    }
}