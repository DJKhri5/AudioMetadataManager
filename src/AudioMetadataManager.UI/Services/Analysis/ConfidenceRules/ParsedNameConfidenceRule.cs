using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Analysis.ConfidenceRules;

public class ParsedNameConfidenceRule : IConfidenceRule
{
    public int Priority => 100;

    public ConfidenceRuleResult Evaluate(AudioFile audioFile)
    {
        bool passed =
            audioFile.ParsedName?.WasParsedSuccessfully == true;

        return new ConfidenceRuleResult
        {
            RuleName = nameof(ParsedNameConfidenceRule),
            Points = passed ? 20 : 0,
            MaximumPoints = 20,
            Passed = passed,
            Message = passed
                ? "El nombre del archivo fue analizado correctamente."
                : "No fue posible separar correctamente artista y título."
        };
    }
}