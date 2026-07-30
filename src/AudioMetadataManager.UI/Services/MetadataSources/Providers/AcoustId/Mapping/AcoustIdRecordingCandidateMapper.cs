using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Dtos;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Mapping;

/// <summary>
/// Convierte los DTO recibidos desde AcoustID en grabaciones
/// normalizadas utilizadas por la aplicación.
/// </summary>
public sealed class AcoustIdRecordingCandidateMapper
{
    /// <summary>
    /// Convierte todos los resultados utilizables, ordenados
    /// de mayor a menor confianza.
    /// </summary>
    public IReadOnlyList<AcoustIdRecordingCandidate> Map(
        IEnumerable<AcoustIdResultDto>? results)
    {
        if (results is null)
        {
            return Array.Empty<AcoustIdRecordingCandidate>();
        }

        return results
            .SelectMany(MapResult)
            .Where(candidate =>
                candidate.HasUsableMetadata)
            .OrderByDescending(candidate =>
                candidate.Score)
            .ToArray();
    }

    private static IEnumerable<AcoustIdRecordingCandidate> MapResult(
        AcoustIdResultDto result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Recordings is null)
        {
            yield break;
        }

        string acoustId =
            NormalizeText(result.Id) ??
            string.Empty;

        foreach (AcoustIdRecordingDto recording in result.Recordings)
        {
            string recordingId =
                NormalizeText(recording.Id) ??
                string.Empty;

            if (string.IsNullOrEmpty(recordingId))
            {
                continue;
            }

            yield return new AcoustIdRecordingCandidate
            {
                AcoustId =
                    acoustId,

                Score =
                    result.Score,

                RecordingId =
                    recordingId,

                Title =
                    NormalizeText(recording.Title),

                ArtistName =
                    JoinArtists(recording.Artists),

                Duration =
                    recording.Duration.HasValue &&
                    recording.Duration.Value > 0
                        ? TimeSpan.FromSeconds(
                            recording.Duration.Value)
                        : null
            };
        }
    }

    private static string? JoinArtists(
        IEnumerable<AcoustIdArtistDto>? artists)
    {
        if (artists is null)
        {
            return null;
        }

        string[] names =
            artists
                .Select(artist =>
                    NormalizeText(artist.Name))
                .Where(name =>
                    name is not null)
                .Cast<string>()
                .ToArray();

        return names.Length == 0
            ? null
            : string.Join(
                " & ",
                names);
    }

    private static string? NormalizeText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
