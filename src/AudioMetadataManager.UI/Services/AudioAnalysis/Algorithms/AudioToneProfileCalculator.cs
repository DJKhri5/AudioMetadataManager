using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;

/// <summary>
/// Construye un perfil tonal reutilizable a partir de un
/// AudioSpectrumProfile previamente calculado.
///
/// Esta clase no abre archivos, no lee PCM y no ejecuta FFT.
/// Solo transforma los bins espectrales existentes en
/// mediciones organizadas por bandas de frecuencia.
/// </summary>
public class AudioToneProfileCalculator
{
    /// <summary>
    /// Construye el perfil tonal utilizando el catálogo
    /// predeterminado de bandas.
    /// </summary>
    public AudioToneProfile Calculate(
        AudioSpectrumProfile spectrumProfile)
    {
        ArgumentNullException.ThrowIfNull(
            spectrumProfile);

        if (!spectrumProfile.IsValid)
        {
            throw new ArgumentException(
                "El perfil espectral no contiene datos válidos.",
                nameof(spectrumProfile));
        }

        IReadOnlyList<AudioFrequencyBandDefinition>
            definitions =
                AudioFrequencyBandCatalog.CreateDefault(
                    spectrumProfile.NyquistFrequencyHz);

        return Calculate(
            spectrumProfile,
            definitions);
    }

    /// <summary>
    /// Construye el perfil tonal utilizando una colección
    /// personalizada de bandas.
    /// </summary>
    public AudioToneProfile Calculate(
        AudioSpectrumProfile spectrumProfile,
        IReadOnlyList<AudioFrequencyBandDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(
            spectrumProfile);

        ArgumentNullException.ThrowIfNull(
            definitions);

        if (!spectrumProfile.IsValid)
        {
            throw new ArgumentException(
                "El perfil espectral no contiene datos válidos.",
                nameof(spectrumProfile));
        }

        if (definitions.Count == 0)
        {
            throw new ArgumentException(
                "La colección de bandas está vacía.",
                nameof(definitions));
        }

        List<PendingBandMeasurement> pendingMeasurements =
            new();

        foreach (
            AudioFrequencyBandDefinition definition
            in definitions)
        {
            definition.Validate();

            PendingBandMeasurement pendingMeasurement =
                CalculateBand(
                    spectrumProfile,
                    definition);

            if (pendingMeasurement.BinCount > 0)
            {
                pendingMeasurements.Add(
                    pendingMeasurement);
            }
        }

        if (pendingMeasurements.Count == 0)
        {
            return new AudioToneProfile();
        }

        double totalLinearEnergy =
            pendingMeasurements.Sum(
                measurement =>
                    measurement.LinearEnergy);

        List<AudioFrequencyBandMeasurement> measurements =
            pendingMeasurements
                .Select(
                    measurement =>
                        BuildMeasurement(
                            measurement,
                            totalLinearEnergy))
                .OrderBy(
                    measurement =>
                        measurement.Definition
                            .MinimumFrequencyHz)
                .ToList();

        return new AudioToneProfile
        {
            Measurements =
                measurements.AsReadOnly()
        };
    }

    /// <summary>
    /// Calcula los acumuladores de una banda concreta.
    /// </summary>
    private static PendingBandMeasurement CalculateBand(
        AudioSpectrumProfile spectrumProfile,
        AudioFrequencyBandDefinition definition)
    {
        double linearMagnitudeSum = 0;
        double linearEnergySum = 0;
        double peakMagnitudeDb =
            double.NegativeInfinity;

        double persistenceSum = 0;
        double peakPersistenceRatio = 0;

        double dominantMagnitudeDb =
            double.NegativeInfinity;

        double dominantFrequencyHz = 0;

        int binCount = 0;

        int availableBinCount =
            Math.Min(
                spectrumProfile.FrequenciesHz.Count,
                Math.Min(
                    spectrumProfile.AverageMagnitudeDb.Count,
                    Math.Min(
                        spectrumProfile.PeakMagnitudeDb.Count,
                        spectrumProfile
                            .SignificantWindowRatios.Count)));

        for (int index = 0;
            index < availableBinCount;
            index++)
        {
            double frequencyHz =
                spectrumProfile.FrequenciesHz[index];

            if (!definition.Contains(
                    frequencyHz))
            {
                continue;
            }

            double averageMagnitudeDb =
                spectrumProfile.AverageMagnitudeDb[index];

            double peakBinMagnitudeDb =
                spectrumProfile.PeakMagnitudeDb[index];

            double persistenceRatio =
                Math.Clamp(
                    spectrumProfile
                        .SignificantWindowRatios[index],
                    0,
                    1);

            double linearMagnitude =
                DecibelsToLinearAmplitude(
                    averageMagnitudeDb);

            /*
             * Para calcular la participación energética
             * utilizamos potencia proporcional a amplitud².
             */
            double linearEnergy =
                linearMagnitude *
                linearMagnitude;

            linearMagnitudeSum +=
                linearMagnitude;

            linearEnergySum +=
                linearEnergy;

            persistenceSum +=
                persistenceRatio;

            if (peakBinMagnitudeDb >
                peakMagnitudeDb)
            {
                peakMagnitudeDb =
                    peakBinMagnitudeDb;
            }

            if (persistenceRatio >
                peakPersistenceRatio)
            {
                peakPersistenceRatio =
                    persistenceRatio;
            }

            if (averageMagnitudeDb >
                dominantMagnitudeDb)
            {
                dominantMagnitudeDb =
                    averageMagnitudeDb;

                dominantFrequencyHz =
                    frequencyHz;
            }

            binCount++;
        }

        double averageMagnitudeLinear =
            binCount > 0
                ? linearMagnitudeSum /
                    binCount
                : 0;

        double averagePersistenceRatio =
            binCount > 0
                ? persistenceSum /
                    binCount
                : 0;

        return new PendingBandMeasurement
        {
            Definition =
                definition,

            BinCount =
                binCount,

            AverageMagnitudeDb =
                LinearAmplitudeToDecibels(
                    averageMagnitudeLinear),

            PeakMagnitudeDb =
                double.IsNegativeInfinity(
                    peakMagnitudeDb)
                    ? -120
                    : peakMagnitudeDb,

            DominantFrequencyHz =
                dominantFrequencyHz,

            AveragePersistenceRatio =
                Math.Clamp(
                    averagePersistenceRatio,
                    0,
                    1),

            PeakPersistenceRatio =
                Math.Clamp(
                    peakPersistenceRatio,
                    0,
                    1),

            LinearEnergy =
                Math.Max(
                    0,
                    linearEnergySum)
        };
    }

    /// <summary>
    /// Construye la medición pública y normaliza su
    /// participación energética respecto del total.
    /// </summary>
    private static AudioFrequencyBandMeasurement BuildMeasurement(
        PendingBandMeasurement pendingMeasurement,
        double totalLinearEnergy)
    {
        double totalEnergyRatio =
            totalLinearEnergy > 0
                ? pendingMeasurement.LinearEnergy /
                    totalLinearEnergy
                : 0;

        return new AudioFrequencyBandMeasurement
        {
            Definition =
                pendingMeasurement.Definition,

            BinCount =
                pendingMeasurement.BinCount,

            AverageMagnitudeDb =
                pendingMeasurement.AverageMagnitudeDb,

            PeakMagnitudeDb =
                pendingMeasurement.PeakMagnitudeDb,

            DominantFrequencyHz =
                pendingMeasurement.DominantFrequencyHz,

            AveragePersistenceRatio =
                pendingMeasurement
                    .AveragePersistenceRatio,

            PeakPersistenceRatio =
                pendingMeasurement
                    .PeakPersistenceRatio,

            TotalEnergyRatio =
                Math.Clamp(
                    totalEnergyRatio,
                    0,
                    1)
        };
    }

    /// <summary>
    /// Convierte dBFS en amplitud lineal.
    /// </summary>
    private static double DecibelsToLinearAmplitude(
        double valueDb)
    {
        if (double.IsNaN(valueDb) ||
            double.IsInfinity(valueDb))
        {
            return 0;
        }

        return Math.Pow(
            10.0,
            valueDb / 20.0);
    }

    /// <summary>
    /// Convierte amplitud lineal en dBFS.
    /// </summary>
    private static double LinearAmplitudeToDecibels(
        double amplitude)
    {
        if (amplitude <= 0 ||
            double.IsNaN(amplitude) ||
            double.IsInfinity(amplitude))
        {
            return -120.0;
        }

        return Math.Max(
            -120.0,
            20.0 *
            Math.Log10(
                amplitude));
    }

    /// <summary>
    /// Estado interno utilizado antes de normalizar
    /// la participación energética entre bandas.
    /// </summary>
    private sealed class PendingBandMeasurement
    {
        public AudioFrequencyBandDefinition Definition
        { get; init; } =
                new();

        public int BinCount { get; init; }

        public double AverageMagnitudeDb { get; init; }

        public double PeakMagnitudeDb { get; init; }

        public double DominantFrequencyHz { get; init; }

        public double AveragePersistenceRatio { get; init; }

        public double PeakPersistenceRatio { get; init; }

        public double LinearEnergy { get; init; }
    }
}