namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

/// <summary>
/// Describe el resultado de ejecutar una regla individual
/// del motor de confianza.
/// </summary>
public sealed class ConfidenceRuleResult
{
    /// <summary>
    /// Nombre estable de la regla ejecutada.
    /// </summary>
    public string RuleName { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la regla pudo ejecutarse correctamente.
    /// </summary>
    public bool EvaluationCompleted { get; init; }

    /// <summary>
    /// Indica si la regla detectó una condición que requiere
    /// atención o revisión.
    /// </summary>
    public bool HasWarning { get; init; }

    /// <summary>
    /// Indica si la regla detectó una condición crítica.
    /// </summary>
    public bool HasCriticalIssue { get; init; }

    /// <summary>
    /// Explicación legible del resultado de la regla.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Resultado satisfactorio sin advertencias.
    /// </summary>
    public static ConfidenceRuleResult Success(
        string ruleName,
        string message)
    {
        return new ConfidenceRuleResult
        {
            RuleName = ruleName,
            EvaluationCompleted = true,
            Message = message
        };
    }

    /// <summary>
    /// Resultado satisfactorio que incluye una advertencia.
    /// </summary>
    public static ConfidenceRuleResult Warning(
        string ruleName,
        string message)
    {
        return new ConfidenceRuleResult
        {
            RuleName = ruleName,
            EvaluationCompleted = true,
            HasWarning = true,
            Message = message
        };
    }

    /// <summary>
    /// Resultado que representa una condición crítica.
    /// </summary>
    public static ConfidenceRuleResult Critical(
        string ruleName,
        string message)
    {
        return new ConfidenceRuleResult
        {
            RuleName = ruleName,
            EvaluationCompleted = true,
            HasWarning = true,
            HasCriticalIssue = true,
            Message = message
        };
    }

    /// <summary>
    /// Resultado producido cuando la regla no pudo evaluarse.
    /// </summary>
    public static ConfidenceRuleResult NotEvaluated(
        string ruleName,
        string message)
    {
        return new ConfidenceRuleResult
        {
            RuleName = ruleName,
            EvaluationCompleted = false,
            HasWarning = true,
            Message = message
        };
    }
}