namespace AudioMetadataManager.UI.Models;

public class ComparisonResult
{
    public bool ArtistMatches { get; set; }

    public bool TitleMatches { get; set; }

    public bool AlbumMatches { get; set; }

    public bool GenreMatches { get; set; }

    public bool YearMatches { get; set; }

    public bool NeedsUpdate { get; set; }

    public int Score { get; set; }

    public string Summary { get; set; } = string.Empty;
}