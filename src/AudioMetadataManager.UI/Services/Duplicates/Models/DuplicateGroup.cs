namespace AudioMetadataManager.UI.Services.Duplicates.Models;

/// <summary>
/// Agrupa dos o más archivos que constituyen copias duplicadas o versiones redundantes.
/// </summary>
public sealed class DuplicateGroup
{
    public required string GroupKey { get; init; }

    public required string DisplayTitle { get; init; }

    public required DuplicateMatchKind MatchKind { get; init; }

    public required IReadOnlyList<DuplicateGroupItem> Items { get; init; }

    public int FileCount => Items.Count;

    public string MatchKindDisplay => MatchKind switch
    {
        DuplicateMatchKind.ExactBinary => "Duplicado exacto (Binario)",
        DuplicateMatchKind.ProbableMetadata => "Duplicado probable (Mismos metadatos)",
        DuplicateMatchKind.SimilarTitle => "Título similar",
        _ => "Coincidencia"
    };

    /// <summary>
    /// Espacio en bytes que podría liberarse si se conservara únicamente la mejor copia.
    /// </summary>
    public long PotentialReclaimableBytes
    {
        get
        {
            if (Items.Count <= 1)
            {
                return 0;
            }

            long total = 0;
            foreach (var item in Items)
            {
                if (!item.IsBestQualityCandidate)
                {
                    total += item.FileSizeBytes;
                }
            }

            return total;
        }
    }

    public string PotentialReclaimableDisplay
    {
        get
        {
            double mb = PotentialReclaimableBytes / (1024.0 * 1024.0);
            return mb >= 1024.0 ? $"{mb / 1024.0:F2} GB" : $"{mb:F2} MB";
        }
    }
}
