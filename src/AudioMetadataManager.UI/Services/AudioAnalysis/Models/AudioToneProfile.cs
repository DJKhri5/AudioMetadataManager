namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Reúne las mediciones tonales obtenidas a partir
/// de un AudioSpectrumProfile ya calculado.
///
/// Este perfil no abre archivos ni ejecuta FFT.
/// Su función es organizar mediciones reutilizables
/// por banda de frecuencia.
/// </summary>
public class AudioToneProfile
{
    /// <summary>
    /// Mediciones disponibles, ordenadas por frecuencia.
    /// </summary>
    public IReadOnlyList<AudioFrequencyBandMeasurement>
        Measurements
    { get; init; } =
            Array.Empty<AudioFrequencyBandMeasurement>();

    /// <summary>
    /// Cantidad de bandas disponibles.
    /// </summary>
    public int BandCount =>
        Measurements.Count;

    /// <summary>
    /// Indica si el perfil contiene al menos una medición
    /// válida y no presenta bandas repetidas.
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (Measurements.Count == 0)
            {
                return false;
            }

            HashSet<AudioFrequencyBand> registeredBands =
                new();

            foreach (
                AudioFrequencyBandMeasurement measurement
                in Measurements)
            {
                if (measurement is null ||
                    !measurement.IsValid)
                {
                    return false;
                }

                if (!registeredBands.Add(
                        measurement.Definition.Band))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Obtiene una medición concreta cuando está disponible.
    /// </summary>
    public AudioFrequencyBandMeasurement? Find(
        AudioFrequencyBand band)
    {
        return Measurements.FirstOrDefault(
            measurement =>
                measurement.Definition.Band ==
                band);
    }

    /// <summary>
    /// Obtiene una medición concreta y genera una excepción
    /// cuando no está disponible.
    /// </summary>
    public AudioFrequencyBandMeasurement GetRequired(
        AudioFrequencyBand band)
    {
        AudioFrequencyBandMeasurement? measurement =
            Find(
                band);

        if (measurement is not null)
        {
            return measurement;
        }

        throw new InvalidOperationException(
            $"La medición tonal \"{band}\" no está " +
            "disponible en este perfil.");
    }

    /// <summary>
    /// Comprueba si una banda se encuentra disponible.
    /// </summary>
    public bool Contains(
        AudioFrequencyBand band)
    {
        return Find(
            band) is not null;
    }

    /// <summary>
    /// Obtiene la banda con mayor participación
    /// en la energía espectral total.
    /// </summary>
    public AudioFrequencyBandMeasurement?
        DominantEnergyBand
    {
        get
        {
            if (Measurements.Count == 0)
            {
                return null;
            }

            return Measurements
                .OrderByDescending(
                    measurement =>
                        measurement.TotalEnergyRatio)
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Obtiene la banda con mayor persistencia media.
    /// </summary>
    public AudioFrequencyBandMeasurement?
        MostPersistentBand
    {
        get
        {
            if (Measurements.Count == 0)
            {
                return null;
            }

            return Measurements
                .OrderByDescending(
                    measurement =>
                        measurement.AveragePersistenceRatio)
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Suma de las participaciones energéticas.
    ///
    /// En un perfil completo debería ser cercana a uno.
    /// </summary>
    public double TotalEnergyRatioSum =>
        Measurements.Sum(
            measurement =>
                measurement.TotalEnergyRatio);

    /// <summary>
    /// Indica si la distribución energética está
    /// correctamente normalizada.
    /// </summary>
    public bool HasNormalizedEnergyDistribution =>
        IsValid &&
        Math.Abs(
            TotalEnergyRatioSum - 1.0) <=
            0.001;

    /// <summary>
    /// Nombre legible de la banda dominante.
    /// </summary>
    public string DominantEnergyBandDisplay =>
        DominantEnergyBand?.DisplayName ??
        "Sin información";

    /// <summary>
    /// Nombre legible de la banda más persistente.
    /// </summary>
    public string MostPersistentBandDisplay =>
        MostPersistentBand?.DisplayName ??
        "Sin información";

    /// <summary>
    /// Participación energética total en formato legible.
    /// </summary>
    public string TotalEnergyRatioSumDisplay =>
        $"{Math.Clamp(TotalEnergyRatioSum, 0, 1) * 100.0:0.00}%";
}