namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Describe una región frecuencial utilizada por el perfil
/// tonal.
///
/// Esta clase solo contiene identidad, límites y nombre.
/// No calcula energía ni determina la calidad del audio.
/// </summary>
public class AudioFrequencyBandDefinition
{
    /// <summary>
    /// Identificador estable de la banda.
    /// </summary>
    public AudioFrequencyBand Band { get; init; }

    /// <summary>
    /// Nombre legible utilizado por informes e interfaz.
    /// </summary>
    public string DisplayName { get; init; } =
        string.Empty;

    /// <summary>
    /// Frecuencia inferior inclusiva de la banda.
    /// </summary>
    public double MinimumFrequencyHz { get; init; }

    /// <summary>
    /// Frecuencia superior exclusiva de la banda.
    ///
    /// Para la última banda puede utilizarse la frecuencia
    /// de Nyquist del perfil.
    /// </summary>
    public double MaximumFrequencyHz { get; init; }

    /// <summary>
    /// Ancho total de la banda.
    /// </summary>
    public double BandwidthHz =>
        Math.Max(
            0,
            MaximumFrequencyHz -
            MinimumFrequencyHz);

    /// <summary>
    /// Frecuencia central aritmética de la banda.
    ///
    /// Se utiliza solo como dato descriptivo.
    /// </summary>
    public double CenterFrequencyHz =>
        MinimumFrequencyHz +
        BandwidthHz / 2.0;

    /// <summary>
    /// Indica si los límites forman una definición válida.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(
            DisplayName) &&
        MinimumFrequencyHz >= 0 &&
        MaximumFrequencyHz >
            MinimumFrequencyHz;

    /// <summary>
    /// Comprueba si una frecuencia pertenece a la banda.
    /// El límite inferior es inclusivo y el superior exclusivo.
    /// </summary>
    public bool Contains(
        double frequencyHz)
    {
        if (!IsValid ||
            double.IsNaN(frequencyHz) ||
            double.IsInfinity(frequencyHz))
        {
            return false;
        }

        return frequencyHz >=
                MinimumFrequencyHz &&
            frequencyHz <
                MaximumFrequencyHz;
    }

    /// <summary>
    /// Comprueba la validez de la definición y genera
    /// una excepción clara cuando existe un error.
    /// </summary>
    public void Validate()
    {
        if (!Enum.IsDefined(
                Band))
        {
            throw new ArgumentOutOfRangeException(
                nameof(Band),
                Band,
                "El identificador de banda no es válido.");
        }

        if (string.IsNullOrWhiteSpace(
                DisplayName))
        {
            throw new ArgumentException(
                "El nombre de la banda está vacío.",
                nameof(DisplayName));
        }

        if (MinimumFrequencyHz < 0 ||
            double.IsNaN(MinimumFrequencyHz) ||
            double.IsInfinity(MinimumFrequencyHz))
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumFrequencyHz),
                MinimumFrequencyHz,
                "La frecuencia mínima debe ser un valor " +
                "finito mayor o igual que cero.");
        }

        if (MaximumFrequencyHz <=
                MinimumFrequencyHz ||
            double.IsNaN(MaximumFrequencyHz) ||
            double.IsInfinity(MaximumFrequencyHz))
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumFrequencyHz),
                MaximumFrequencyHz,
                "La frecuencia máxima debe ser finita y " +
                "mayor que la frecuencia mínima.");
        }
    }
}