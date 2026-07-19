namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Describe la distribución energética general del audio
/// entre regiones bajas, medias y altas.
///
/// Este modelo no determina por sí mismo si el balance tonal
/// es correcto, incorrecto o apropiado para un género.
/// </summary>
public class AudioToneBalanceProfile
{
    /// <summary>
    /// Participación energética conjunta de subgraves
    /// y graves.
    ///
    /// Los valores se encuentran entre 0 y 1.
    /// </summary>
    public double LowFrequencyEnergyRatio { get; init; }

    /// <summary>
    /// Participación energética conjunta de medios bajos,
    /// medios y medios altos.
    ///
    /// Los valores se encuentran entre 0 y 1.
    /// </summary>
    public double MidFrequencyEnergyRatio { get; init; }

    /// <summary>
    /// Participación energética conjunta de agudos y aire.
    ///
    /// Los valores se encuentran entre 0 y 1.
    /// </summary>
    public double HighFrequencyEnergyRatio { get; init; }

    /// <summary>
    /// Relación entre la energía baja y la energía media.
    ///
    /// Un valor superior a uno indica mayor participación
    /// energética en la región baja que en la región media.
    /// </summary>
    public double LowToMidEnergyRatio =>
        DivideSafely(
            LowFrequencyEnergyRatio,
            MidFrequencyEnergyRatio);

    /// <summary>
    /// Relación entre la energía alta y la energía media.
    ///
    /// Un valor superior a uno indica mayor participación
    /// energética en la región alta que en la región media.
    /// </summary>
    public double HighToMidEnergyRatio =>
        DivideSafely(
            HighFrequencyEnergyRatio,
            MidFrequencyEnergyRatio);

    /// <summary>
    /// Relación entre la energía baja y la energía alta.
    ///
    /// Un valor superior a uno indica mayor participación
    /// energética en la región baja que en la región alta.
    /// </summary>
    public double LowToHighEnergyRatio =>
        DivideSafely(
            LowFrequencyEnergyRatio,
            HighFrequencyEnergyRatio);

    /// <summary>
    /// Suma de las tres regiones energéticas.
    ///
    /// En un perfil completo debería ser cercana a uno.
    /// </summary>
    public double TotalEnergyRatio =>
        LowFrequencyEnergyRatio +
        MidFrequencyEnergyRatio +
        HighFrequencyEnergyRatio;

    /// <summary>
    /// Indica si el perfil contiene proporciones válidas
    /// y correctamente normalizadas.
    /// </summary>
    public bool IsValid =>
        IsValidRatio(
            LowFrequencyEnergyRatio) &&
        IsValidRatio(
            MidFrequencyEnergyRatio) &&
        IsValidRatio(
            HighFrequencyEnergyRatio) &&
        Math.Abs(
            TotalEnergyRatio - 1.0) <=
            0.001;

    /// <summary>
    /// Nombre descriptivo de la región con mayor
    /// participación energética.
    ///
    /// Este valor no representa una evaluación de calidad.
    /// </summary>
    public string DominantRegionDisplay
    {
        get
        {
            if (!IsValid)
            {
                return "Sin información";
            }

            double maximum =
                Math.Max(
                    LowFrequencyEnergyRatio,
                    Math.Max(
                        MidFrequencyEnergyRatio,
                        HighFrequencyEnergyRatio));

            if (maximum ==
                LowFrequencyEnergyRatio)
            {
                return "Región baja";
            }

            if (maximum ==
                MidFrequencyEnergyRatio)
            {
                return "Región media";
            }

            return "Región alta";
        }
    }

    /// <summary>
    /// Participación de frecuencias bajas en formato legible.
    /// </summary>
    public string LowFrequencyEnergyDisplay =>
        FormatPercentage(
            LowFrequencyEnergyRatio);

    /// <summary>
    /// Participación de frecuencias medias en formato legible.
    /// </summary>
    public string MidFrequencyEnergyDisplay =>
        FormatPercentage(
            MidFrequencyEnergyRatio);

    /// <summary>
    /// Participación de frecuencias altas en formato legible.
    /// </summary>
    public string HighFrequencyEnergyDisplay =>
        FormatPercentage(
            HighFrequencyEnergyRatio);

    /// <summary>
    /// Suma energética en formato legible.
    /// </summary>
    public string TotalEnergyRatioDisplay =>
        FormatPercentage(
            TotalEnergyRatio);

    /// <summary>
    /// Relación bajas/medias en formato legible.
    /// </summary>
    public string LowToMidEnergyRatioDisplay =>
        FormatRatio(
            LowToMidEnergyRatio);

    /// <summary>
    /// Relación altas/medias en formato legible.
    /// </summary>
    public string HighToMidEnergyRatioDisplay =>
        FormatRatio(
            HighToMidEnergyRatio);

    /// <summary>
    /// Relación bajas/altas en formato legible.
    /// </summary>
    public string LowToHighEnergyRatioDisplay =>
        FormatRatio(
            LowToHighEnergyRatio);

    /// <summary>
    /// Comprueba que una proporción se encuentre
    /// entre cero y uno.
    /// </summary>
    private static bool IsValidRatio(
        double value)
    {
        return !double.IsNaN(value) &&
            !double.IsInfinity(value) &&
            value >= 0 &&
            value <= 1;
    }

    /// <summary>
    /// Divide dos valores evitando resultados infinitos.
    /// </summary>
    private static double DivideSafely(
        double numerator,
        double denominator)
    {
        if (double.IsNaN(numerator) ||
            double.IsInfinity(numerator) ||
            double.IsNaN(denominator) ||
            double.IsInfinity(denominator) ||
            denominator <= 0)
        {
            return 0;
        }

        return Math.Max(
            0,
            numerator / denominator);
    }

    /// <summary>
    /// Formatea una proporción como porcentaje.
    /// </summary>
    private static string FormatPercentage(
        double value)
    {
        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            return "Sin información";
        }

        return
            $"{Math.Clamp(value, 0, 1) * 100.0:0.00}%";
    }

    /// <summary>
    /// Formatea una relación numérica.
    /// </summary>
    private static string FormatRatio(
        double value)
    {
        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            return "Sin información";
        }

        return
            $"{Math.Max(0, value):0.000}";
    }
}