using System.Collections.ObjectModel;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Resultado general del motor de evaluación técnica.
///
/// Este modelo reúne la conclusión del análisis y los tipos
/// de incoherencia detectados, sin ejecutar por sí mismo
/// ninguna regla de evaluación.
/// </summary>
public class AudioQualityAnalysisResult
{
    /// <summary>
    /// Estado general de la evaluación técnica.
    /// </summary>
    public AudioQualityAssessmentStatus Status { get; set; } =
        AudioQualityAssessmentStatus.InsufficientData;

    /// <summary>
    /// Tipos de incoherencia técnica detectados.
    /// </summary>
    public Collection<AudioQualityIssueType> Issues { get; } =
        new();

    /// <summary>
    /// Indica si el motor pudo aplicar al menos una regla
    /// técnica al archivo analizado.
    /// </summary>
    public bool IsApplicable { get; set; }

    /// <summary>
    /// Indica si la evaluación terminó correctamente.
    /// </summary>
    public bool AnalysisCompleted { get; set; }

    /// <summary>
    /// Mensaje de error controlado, cuando corresponda.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Resumen descriptivo del resultado.
    /// </summary>
    public string Summary { get; set; } =
        string.Empty;

    /// <summary>
    /// Indica si existe un error registrado.
    /// </summary>
    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);

    /// <summary>
    /// Indica si se detectó al menos una incoherencia.
    /// </summary>
    public bool HasIssues =>
        Issues.Any(
            issue =>
                issue !=
                AudioQualityIssueType.None);

    /// <summary>
    /// Cantidad de incoherencias técnicas registradas.
    /// </summary>
    public int IssueCount =>
        Issues.Count(
            issue =>
                issue !=
                AudioQualityIssueType.None);

    /// <summary>
    /// Indica si el resultado contiene datos utilizables.
    /// </summary>
    public bool IsValid =>
        AnalysisCompleted &&
        !HasError &&
        Status !=
            AudioQualityAssessmentStatus.InsufficientData;

    /// <summary>
    /// Agrega una incoherencia evitando duplicados y
    /// excluyendo el valor None.
    /// </summary>
    public void AddIssue(
        AudioQualityIssueType issue)
    {
        if (issue ==
            AudioQualityIssueType.None)
        {
            return;
        }

        if (Issues.Contains(
            issue))
        {
            return;
        }

        Issues.Add(
            issue);
    }

    /// <summary>
    /// Elimina todas las incoherencias registradas.
    /// </summary>
    public void ClearIssues()
    {
        Issues.Clear();
    }
}