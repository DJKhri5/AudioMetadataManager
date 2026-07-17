namespace AudioMetadataManager.UI.Services.MetadataSources.Consensus;

/// <summary>
/// Representa un valor encontrado durante el proceso
/// de consenso.
/// </summary>
public class MetadataConsensusValue
{
    /// <summary>
    /// Valor textual encontrado.
    /// </summary>
    public string Value { get; set; } =
        string.Empty;

    /// <summary>
    /// Plataformas que entregaron este mismo valor.
    /// </summary>
    public List<string> Sources { get; set; } =
        new();

    /// <summary>
    /// Cantidad de fuentes que respaldan el valor.
    /// </summary>
    public int VoteCount =>
        Sources.Count;

    /// <summary>
    /// Indica si SoundCloud participa.
    /// </summary>
    public bool RequiresSourceApproval =>
        Sources.Contains(
            "SoundCloud",
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Texto legible.
    /// </summary>
    public string Summary =>
        $"{Value} ({VoteCount} voto(s))";
}