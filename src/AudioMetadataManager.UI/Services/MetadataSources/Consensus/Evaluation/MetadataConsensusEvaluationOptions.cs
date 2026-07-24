namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Evaluation;

/// <summary>
/// Contiene los umbrales utilizados para decidir el consenso
/// de un campo de metadatos.
///
/// Mantener estas reglas en una configuración independiente
/// permite ajustar la política sin modificar el evaluador.
/// </summary>
public sealed class MetadataConsensusEvaluationOptions
{
    /// <summary>
    /// Cantidad mínima de fuentes diferentes necesaria para
    /// considerar que existe consenso real entre plataformas.
    /// </summary>
    public int MinimumSourcesForConsensus { get; init; } =
        2;

    /// <summary>
    /// Proporción mínima del soporte total que debe obtener el
    /// grupo ganador para considerarse una mayoría clara.
    ///
    /// Se expresa como un valor entre 0 y 1.
    /// </summary>
    public double MajoritySupportShareThreshold { get; init; } =
        0.60;

    /// <summary>
    /// Diferencia mínima entre el grupo ganador y el segundo
    /// grupo, calculada sobre el soporte total.
    ///
    /// Evita declarar una mayoría cuando los dos primeros
    /// valores están prácticamente empatados.
    /// </summary>
    public double MinimumSupportLead { get; init; } =
        0.15;

    /// <summary>
    /// Confianza máxima permitida cuando sólo una fuente
    /// respalda el valor.
    ///
    /// Un resultado de una sola plataforma puede ser muy
    /// bueno, pero todavía no constituye consenso.
    /// </summary>
    public double SingleSourceConfidenceCap { get; init; } =
        0.85;

    /// <summary>
    /// Indica si los parámetros contienen valores utilizables.
    /// </summary>
    public bool IsValid =>
        MinimumSourcesForConsensus >= 2 &&
        MajoritySupportShareThreshold is > 0 and <= 1 &&
        MinimumSupportLead is >= 0 and <= 1 &&
        SingleSourceConfidenceCap is > 0 and <= 1;
}