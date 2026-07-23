using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates;

/// <summary>
/// Convierte un candidato externo normalizado en una entrada
/// neutral compatible con MetadataComparisonEngine.
///
/// El adaptador no conoce detalles de Discogs, Beatport,
/// Spotify o SoundCloud. Trabaja exclusivamente con el modelo
/// común MetadataCandidate.
/// </summary>
public sealed class MetadataCandidateComparisonAdapter
{
    /// <summary>
    /// Construye una entrada de comparación desde un candidato
    /// obtenido de una fuente externa.
    /// </summary>
    public MetadataComparisonInput CreateInput(
        MetadataCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(
            candidate);

        return new MetadataComparisonInput
        {
            SourceName =
                BuildSourceName(
                    candidate),

            Artist =
                Normalize(
                    candidate.Artist),

            Title =
                Normalize(
                    candidate.Title),

            Version =
                Normalize(
                    candidate.Version),

            Album =
                Normalize(
                    candidate.ReleaseTitle),

            Genre =
                Normalize(
                    candidate.Genre),

            Label =
                Normalize(
                    candidate.Label)
        };
    }

    /// <summary>
    /// Construye un nombre trazable para identificar la fuente
    /// y el resultado durante comparaciones y diagnósticos.
    /// </summary>
    private static string BuildSourceName(
        MetadataCandidate candidate)
    {
        string sourceName =
            string.IsNullOrWhiteSpace(
                candidate.SourceName)
                    ? "Fuente externa"
                    : candidate.SourceName.Trim();

        if (string.IsNullOrWhiteSpace(
                candidate.SourceId))
        {
            return sourceName;
        }

        return
            $"{sourceName} · " +
            $"{candidate.SourceId.Trim()}";
    }

    private static string? Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
                ? null
                : value.Trim();
    }
}