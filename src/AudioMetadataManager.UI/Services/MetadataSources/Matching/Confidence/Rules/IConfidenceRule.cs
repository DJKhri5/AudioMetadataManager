using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Rules;

/// <summary>
/// Define una regla independiente del motor de confianza.
/// </summary>
public interface IConfidenceRule
{
    /// <summary>
    /// Nombre estable de la regla.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Orden de ejecución.
    ///
    /// Los valores menores se ejecutan primero.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Evalúa y actualiza el contexto compartido.
    /// </summary>
    ConfidenceRuleResult Evaluate(
        ConfidenceContext context);
}