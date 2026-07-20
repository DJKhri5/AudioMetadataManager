using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;

/// <summary>
/// Define una regla reutilizable del motor de evaluación
/// técnica del audio.
///
/// Las reglas consumen exclusivamente información ya
/// disponible en AudioAnalysisContext.
///
/// No pueden abrir nuevamente el archivo, decodificar PCM
/// ni ejecutar una segunda FFT.
/// </summary>
public interface IAudioQualityRule
{
    /// <summary>
    /// Nombre legible de la regla.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Orden de ejecución dentro del motor de calidad.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Determina si la regla puede aplicarse utilizando
    /// la información disponible en el contexto.
    /// </summary>
    bool IsApplicable(
        AudioAnalysisContext context);

    /// <summary>
    /// Evalúa el contexto y devuelve el resultado parcial
    /// producido por la regla.
    ///
    /// La regla no modifica directamente el resultado general.
    /// </summary>
    AudioQualityRuleResult Evaluate(
        AudioAnalysisContext context);
}