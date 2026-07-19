using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;

/// <summary>
/// Construye un perfil de balance tonal a partir de un
/// AudioToneProfile previamente calculado.
///
/// Esta clase no abre archivos, no procesa PCM y no ejecuta FFT.
/// Solo agrupa la participación energética de las bandas
/// existentes en regiones bajas, medias y altas.
/// </summary>
public class AudioToneBalanceCalculator
{
    /// <summary>
    /// Construye el balance tonal general.
    /// </summary>
    public AudioToneBalanceProfile Calculate(
        AudioToneProfile toneProfile)
    {
        ArgumentNullException.ThrowIfNull(
            toneProfile);

        if (!toneProfile.IsValid)
        {
            throw new ArgumentException(
                "El perfil tonal no contiene datos válidos.",
                nameof(toneProfile));
        }

        double lowFrequencyEnergyRatio =
            GetEnergyRatio(
                toneProfile,
                AudioFrequencyBand.SubBass) +
            GetEnergyRatio(
                toneProfile,
                AudioFrequencyBand.Bass);

        double midFrequencyEnergyRatio =
            GetEnergyRatio(
                toneProfile,
                AudioFrequencyBand.LowMidrange) +
            GetEnergyRatio(
                toneProfile,
                AudioFrequencyBand.Midrange) +
            GetEnergyRatio(
                toneProfile,
                AudioFrequencyBand.UpperMidrange);

        double highFrequencyEnergyRatio =
            GetEnergyRatio(
                toneProfile,
                AudioFrequencyBand.Treble) +
            GetEnergyRatio(
                toneProfile,
                AudioFrequencyBand.Air);

        double totalEnergyRatio =
            lowFrequencyEnergyRatio +
            midFrequencyEnergyRatio +
            highFrequencyEnergyRatio;

        if (totalEnergyRatio <= 0)
        {
            return new AudioToneBalanceProfile();
        }

        /*
         * Normalizamos nuevamente las tres regiones para
         * evitar pequeñas desviaciones provocadas por
         * redondeos acumulados.
         */
        lowFrequencyEnergyRatio /=
            totalEnergyRatio;

        midFrequencyEnergyRatio /=
            totalEnergyRatio;

        highFrequencyEnergyRatio /=
            totalEnergyRatio;

        return new AudioToneBalanceProfile
        {
            LowFrequencyEnergyRatio =
                Math.Clamp(
                    lowFrequencyEnergyRatio,
                    0,
                    1),

            MidFrequencyEnergyRatio =
                Math.Clamp(
                    midFrequencyEnergyRatio,
                    0,
                    1),

            HighFrequencyEnergyRatio =
                Math.Clamp(
                    highFrequencyEnergyRatio,
                    0,
                    1)
        };
    }

    /// <summary>
    /// Obtiene la participación energética de una banda.
    ///
    /// Cuando una banda no está disponible, devuelve cero.
    /// Esto permite trabajar con perfiles cuyo Nyquist no
    /// alcanza todas las regiones definidas.
    /// </summary>
    private static double GetEnergyRatio(
        AudioToneProfile toneProfile,
        AudioFrequencyBand band)
    {
        AudioFrequencyBandMeasurement? measurement =
            toneProfile.Find(
                band);

        if (measurement is null ||
            !measurement.IsValid)
        {
            return 0;
        }

        return Math.Clamp(
            measurement.TotalEnergyRatio,
            0,
            1);
    }
}