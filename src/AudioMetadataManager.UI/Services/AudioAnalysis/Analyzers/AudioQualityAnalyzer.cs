using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Ejecuta y combina las reglas reutilizables del motor
/// de evaluación técnica del audio.
///
/// Este analizador trabaja exclusivamente con información
/// ya almacenada en AudioAnalysisContext.
///
/// No abre archivos, no decodifica PCM y no ejecuta FFT.
/// </summary>
public class AudioQualityAnalyzer
{
    private readonly IReadOnlyList<IAudioQualityRule> _rules;

    /// <summary>
    /// Crea el analizador utilizando las reglas indicadas.
    /// </summary>
    public AudioQualityAnalyzer(
        IEnumerable<IAudioQualityRule> rules)
    {
        ArgumentNullException.ThrowIfNull(
            rules);

        _rules =
            rules
                .Where(
                    rule =>
                        rule is not null)
                .OrderBy(
                    rule =>
                        rule.Order)
                .ThenBy(
                    rule =>
                        rule.Name)
                .ToList()
                .AsReadOnly();
    }

    /// <summary>
    /// Ejecuta las reglas aplicables y combina sus
    /// resultados en una única evaluación técnica.
    /// </summary>
    public AudioQualityAnalysisResult Analyze(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioQualityAnalysisResult result =
            new();

        if (_rules.Count == 0)
        {
            result.AnalysisCompleted =
                true;

            result.IsApplicable =
                false;

            result.Status =
                AudioQualityAssessmentStatus.NotApplicable;

            result.Summary =
                "No existen reglas de calidad registradas.";

            return result;
        }

        List<AudioQualityRuleResult> applicableResults =
            new();

        List<string> controlledErrors =
            new();

        foreach (IAudioQualityRule rule in _rules)
        {
            bool isApplicable;

            try
            {
                isApplicable =
                    rule.IsApplicable(
                        context);
            }
            catch (Exception exception)
            {
                controlledErrors.Add(
                    $"{rule.Name}: " +
                    $"{exception.Message}");

                continue;
            }

            if (!isApplicable)
            {
                continue;
            }

            try
            {
                AudioQualityRuleResult ruleResult =
                    rule.Evaluate(
                        context);

                if (ruleResult is null)
                {
                    controlledErrors.Add(
                        $"{rule.Name}: " +
                        "la regla no devolvió un resultado.");

                    continue;
                }

                if (!ruleResult.IsApplicable)
                {
                    continue;
                }

                if (ruleResult.HasError)
                {
                    controlledErrors.Add(
                        $"{rule.Name}: " +
                        $"{ruleResult.ErrorMessage}");

                    continue;
                }

                applicableResults.Add(
                    ruleResult);
            }
            catch (Exception exception)
            {
                controlledErrors.Add(
                    $"{rule.Name}: " +
                    $"{exception.Message}");
            }
        }

        result.IsApplicable =
            applicableResults.Count > 0;

        if (!result.IsApplicable)
        {
            result.AnalysisCompleted =
                true;

            result.Status =
                controlledErrors.Count > 0
                    ? AudioQualityAssessmentStatus.InsufficientData
                    : AudioQualityAssessmentStatus.NotApplicable;

            result.ErrorMessage =
                controlledErrors.Count > 0
                    ? string.Join(
                        " | ",
                        controlledErrors)
                    : null;

            result.Summary =
                controlledErrors.Count > 0
                    ? "Las reglas aplicables no pudieron " +
                      "producir una evaluación técnica."
                    : "Ninguna regla registrada resulta " +
                      "aplicable al archivo.";

            return result;
        }

        foreach (
            AudioQualityRuleResult ruleResult
            in applicableResults)
        {
            foreach (
                AudioQualityIssueType issue
                in ruleResult.Issues)
            {
                result.AddIssue(
                    issue);
            }
        }

        result.Status =
            DetermineFinalStatus(
                applicableResults);

        result.AnalysisCompleted =
            true;

        result.Summary =
            BuildSummary(
                result,
                applicableResults);

        return result;
    }

    /// <summary>
    /// Determina el estado final conservando la conclusión
    /// más severa propuesta por las reglas aplicables.
    /// </summary>
    private static AudioQualityAssessmentStatus
        DetermineFinalStatus(
            IReadOnlyCollection<AudioQualityRuleResult>
                ruleResults)
    {
        if (ruleResults.Count == 0)
        {
            return
                AudioQualityAssessmentStatus.InsufficientData;
        }

        AudioQualityAssessmentStatus finalStatus =
            AudioQualityAssessmentStatus.Consistent;

        foreach (
            AudioQualityRuleResult ruleResult
            in ruleResults)
        {
            if (GetSeverity(
                    ruleResult.SuggestedStatus) >
                GetSeverity(
                    finalStatus))
            {
                finalStatus =
                    ruleResult.SuggestedStatus;
            }
        }

        return finalStatus;
    }

    /// <summary>
    /// Devuelve el nivel de severidad utilizado para
    /// combinar estados parciales.
    /// </summary>
    private static int GetSeverity(
        AudioQualityAssessmentStatus status)
    {
        return status switch
        {
            AudioQualityAssessmentStatus.NotApplicable =>
                0,

            AudioQualityAssessmentStatus.InsufficientData =>
                1,

            AudioQualityAssessmentStatus.Consistent =>
                2,

            AudioQualityAssessmentStatus.SlightlySuspicious =>
                3,

            AudioQualityAssessmentStatus.Suspicious =>
                4,

            AudioQualityAssessmentStatus.LikelyTranscoded =>
                5,

            _ =>
                1
        };
    }

    /// <summary>
    /// Construye un resumen general utilizando los
    /// resultados parciales ya calculados.
    /// </summary>
    private static string BuildSummary(
        AudioQualityAnalysisResult result,
        IEnumerable<AudioQualityRuleResult> ruleResults)
    {
        List<string> details =
            new()
            {
                $"Estado técnico: " +
                $"{GetStatusDisplayName(result.Status)}",

                $"Reglas aplicadas: " +
                $"{ruleResults.Count()}",

                $"Incoherencias detectadas: " +
                $"{result.IssueCount}"
            };

        List<string> ruleSummaries =
            ruleResults
                .Select(
                    ruleResult =>
                        ruleResult.Summary)
                .Where(
                    summary =>
                        !string.IsNullOrWhiteSpace(
                            summary))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        if (ruleSummaries.Count > 0)
        {
            details.AddRange(
                ruleSummaries);
        }

        return string.Join(
            " · ",
            details);
    }

    /// <summary>
    /// Convierte el estado general a un texto legible.
    /// </summary>
    public static string GetStatusDisplayName(
        AudioQualityAssessmentStatus status)
    {
        return status switch
        {
            AudioQualityAssessmentStatus.NotApplicable =>
                "No aplicable",

            AudioQualityAssessmentStatus.Consistent =>
                "Coherente",

            AudioQualityAssessmentStatus.SlightlySuspicious =>
                "Levemente sospechoso",

            AudioQualityAssessmentStatus.Suspicious =>
                "Sospechoso",

            AudioQualityAssessmentStatus.LikelyTranscoded =>
                "Probable transcodificación",

            _ =>
                "Información insuficiente"
        };
    }
}