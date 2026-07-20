using System.Collections.ObjectModel;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Resultado parcial producido por una regla del motor
/// de evaluación técnica.
///
/// Este modelo no representa la conclusión final del motor.
/// Sus resultados serán combinados posteriormente por
/// AudioQualityAnalyzer.
/// </summary>
public class AudioQualityRuleResult
{
    /// <summary>
    /// Nombre de la regla que produjo el resultado.
    /// </summary>
    public string RuleName { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la regla fue aplicable.
    /// </summary>
    public bool IsApplicable { get; init; }

    /// <summary>
    /// Estado técnico propuesto por la regla.
    /// </summary>
    public AudioQualityAssessmentStatus SuggestedStatus { get; init; } =
        AudioQualityAssessmentStatus.InsufficientData;

    /// <summary>
    /// Incoherencias detectadas por la regla.
    /// </summary>
    public Collection<AudioQualityIssueType> Issues { get; } =
        new();

    /// <summary>
    /// Resumen técnico breve.
    /// </summary>
    public string Summary { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensaje de error controlado, cuando corresponda.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Indica si la regla registró un error.
    /// </summary>
    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);

    /// <summary>
    /// Indica si la regla detectó al menos una incoherencia.
    /// </summary>
    public bool HasIssues =>
        Issues.Any(
            issue =>
                issue !=
                AudioQualityIssueType.None);

    /// <summary>
    /// Agrega una incoherencia evitando duplicados.
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
}