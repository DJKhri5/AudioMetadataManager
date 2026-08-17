namespace AudioMetadataManager.UI.Services.Duplicates.Models;

/// <summary>
/// Contiene el diagnóstico consolidado del análisis de duplicados de la biblioteca.
/// </summary>
public sealed class DuplicateDetectionResult
{
    public required IReadOnlyList<DuplicateGroup> Groups { get; init; }

    public int TotalDuplicateGroups => Groups.Count;

    public int TotalDuplicateFiles => Groups.Sum(g => g.FileCount);

    public int ExactDuplicateGroupsCount =>
        Groups.Count(g => g.MatchKind == DuplicateMatchKind.ExactBinary);

    public int ProbableDuplicateGroupsCount =>
        Groups.Count(g => g.MatchKind == DuplicateMatchKind.ProbableMetadata);

    public long TotalPotentialReclaimableBytes =>
        Groups.Sum(g => g.PotentialReclaimableBytes);

    public string TotalPotentialReclaimableDisplay
    {
        get
        {
            double mb = TotalPotentialReclaimableBytes / (1024.0 * 1024.0);
            return mb >= 1024.0 ? $"{mb / 1024.0:F2} GB" : $"{mb:F2} MB";
        }
    }
}
