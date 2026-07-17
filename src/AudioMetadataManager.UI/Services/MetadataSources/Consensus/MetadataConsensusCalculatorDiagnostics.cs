namespace AudioMetadataManager.UI.Services.MetadataSources.Consensus;

/// <summary>
/// Ejecuta pruebas controladas del MetadataConsensusCalculator.
///
/// No consulta Internet ni modifica archivos reales.
/// </summary>
public static class MetadataConsensusCalculatorDiagnostics
{
    public static IReadOnlyList<MetadataConsensusField> Run()
    {
        MetadataConsensusCalculator calculator = new();

        return new List<MetadataConsensusField>
        {
            RunClearConsensusTest(calculator),
            RunTieTest(calculator),
            RunStrongConflictTest(calculator),
            RunSoundCloudApprovalTest(calculator)
        };
    }

    public static string BuildReport()
    {
        IReadOnlyList<MetadataConsensusField> results =
            Run();

        List<string> lines = new()
        {
            "=== Diagnóstico del MetadataConsensusCalculator ==="
        };

        foreach (MetadataConsensusField result in results)
        {
            lines.Add(string.Empty);
            lines.Add($"Campo: {result.FieldName}");
            lines.Add($"Valor seleccionado: {Display(result.SelectedValue)}");
            lines.Add($"Confianza: {result.ConfidenceScore}%");
            lines.Add($"Nivel: {result.ConfidenceLevel}");
            lines.Add($"Fuentes: {result.SupportingSourcesDisplay}");
            lines.Add($"Conflicto: {result.ConflictDisplay}");
            lines.Add($"Revisión manual: {result.ManualReviewDisplay}");
            lines.Add(
                $"Aprobación de fuente: " +
                $"{ToSpanish(result.RequiresSourceApproval)}");
            lines.Add($"Motivo: {result.Reason}");

            if (result.AlternativeValues.Count > 0)
            {
                lines.Add("Alternativas:");

                foreach (
                    KeyValuePair<string, string> alternative
                    in result.AlternativeValues)
                {
                    lines.Add(
                        $"- {alternative.Key}: " +
                        $"{alternative.Value}");
                }
            }
        }

        lines.Add(string.Empty);
        lines.Add("=== Fin del diagnóstico ===");

        return string.Join(
            Environment.NewLine,
            lines);
    }

    private static MetadataConsensusField
        RunClearConsensusTest(
            MetadataConsensusCalculator calculator)
    {
        Dictionary<string, string?> values = new()
        {
            ["Beatport"] =
                "Armin van Buuren",

            ["Discogs"] =
                "Armin Van Buuren",

            ["Spotify"] =
                "Armin van Buuren",

            ["SoundCloud"] =
                "Armin van Buuren Official"
        };

        return calculator.Calculate(
            "Artista - consenso claro",
            values);
    }

    private static MetadataConsensusField
        RunTieTest(
            MetadataConsensusCalculator calculator)
    {
        Dictionary<string, string?> values = new()
        {
            ["Beatport"] =
                "Extended Mix",

            ["Discogs"] =
                "Original Mix"
        };

        return calculator.Calculate(
            "Versión - empate",
            values);
    }

    private static MetadataConsensusField
        RunStrongConflictTest(
            MetadataConsensusCalculator calculator)
    {
        Dictionary<string, string?> values = new()
        {
            ["Beatport"] =
                "Trance",

            ["Discogs"] =
                "Progressive Trance",

            ["Spotify"] =
                "Electronic"
        };

        return calculator.Calculate(
            "Género - conflicto",
            values);
    }

    private static MetadataConsensusField
        RunSoundCloudApprovalTest(
            MetadataConsensusCalculator calculator)
    {
        Dictionary<string, string?> values = new()
        {
            ["SoundCloud"] =
                "Relapse",

            ["Beatport"] =
                "Relapse"
        };

        return calculator.Calculate(
            "Título - SoundCloud",
            values);
    }

    private static string Display(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Sin valor"
            : value;
    }

    private static string ToSpanish(bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}