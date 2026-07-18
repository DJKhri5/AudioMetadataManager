namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Configuración utilizada por AudioEnvelopeAnalyzer.
///
/// Todos los parámetros son técnicos y afectan únicamente
/// la forma en que se calcula la envolvente energética.
/// </summary>
public class AudioEnvelopeAnalysisOptions
{
    /// <summary>
    /// Duración de cada ventana RMS.
    /// </summary>
    public TimeSpan WindowDuration { get; set; } =
        TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Solapamiento entre ventanas consecutivas.
    /// </summary>
    public double WindowOverlap { get; set; } =
        0.50;

    /// <summary>
    /// Umbral energético mínimo considerado como
    /// contenido musical.
    /// </summary>
    public double EnergyThresholdDb { get; set; } =
        -45.0;

    /// <summary>
    /// Cantidad mínima de ventanas consecutivas para
    /// confirmar una región musical.
    /// </summary>
    public int MinimumConsecutiveWindows { get; set; } =
        3;

    /// <summary>
    /// Habilita la búsqueda de posibles fade-in.
    /// </summary>
    public bool DetectFadeIn { get; set; } = true;

    /// <summary>
    /// Habilita la búsqueda de posibles fade-out.
    /// </summary>
    public bool DetectFadeOut { get; set; } = true;

    /// <summary>
    /// Habilita la detección de posibles colas de
    /// reverberación.
    /// </summary>
    public bool DetectReverbTail { get; set; } = true;

    /// <summary>
    /// Comprueba que la configuración sea válida.
    /// </summary>
    public void Validate()
    {
        if (WindowDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(WindowDuration));
        }

        if (WindowOverlap < 0 ||
            WindowOverlap >= 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(WindowOverlap));
        }

        if (MinimumConsecutiveWindows < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumConsecutiveWindows));
        }
    }
}