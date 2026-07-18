namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Clase base para los resultados producidos por los
/// módulos especializados del motor de análisis.
///
/// Centraliza el estado común de todos los analizadores:
/// finalización, confiabilidad, errores y resumen.
///
/// Cada módulo conserva en su clase derivada únicamente
/// las mediciones propias de su especialidad.
/// </summary>
public abstract class AnalysisModuleResult
{
    /// <summary>
    /// Indica si el módulo terminó su ejecución.
    /// </summary>
    public bool AnalysisCompleted { get; set; }

    /// <summary>
    /// Indica si las mediciones obtenidas son suficientemente
    /// consistentes para ser utilizadas posteriormente.
    /// </summary>
    public bool IsReliable { get; set; }

    /// <summary>
    /// Explicación descriptiva del resultado obtenido.
    /// </summary>
    public string Summary { get; set; } =
        string.Empty;

    /// <summary>
    /// Mensaje de error específico del módulo.
    /// </summary>
    public string ErrorMessage { get; set; } =
        string.Empty;

    /// <summary>
    /// Indica si el módulo registró un error.
    /// </summary>
    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);

    /// <summary>
    /// Indica si el módulo dispone de mediciones válidas
    /// para participar en comparaciones posteriores.
    ///
    /// Cada resultado especializado debe definir sus propios
    /// requisitos mínimos.
    /// </summary>
    public abstract bool HasComparisonData { get; }

    /// <summary>
    /// Estado legible y uniforme del módulo.
    /// </summary>
    public virtual string StatusDisplay
    {
        get
        {
            if (HasError)
            {
                return "Error";
            }

            if (!AnalysisCompleted)
            {
                return "Pendiente";
            }

            if (!IsReliable)
            {
                return
                    "Análisis completado con datos limitados";
            }

            return "Análisis completado";
        }
    }
}