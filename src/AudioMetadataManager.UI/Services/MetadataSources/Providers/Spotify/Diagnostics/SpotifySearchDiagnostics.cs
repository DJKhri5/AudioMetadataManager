using System.Text;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Diagnostics;

/// <summary>
/// Ejecuta una búsqueda controlada contra Spotify y produce
/// un informe legible.
/// </summary>
public sealed class SpotifySearchDiagnostics
{
    private readonly SpotifyMetadataProvider
        _provider;

    public SpotifySearchDiagnostics()
        : this(
            new SpotifyMetadataProvider(
                SpotifyOptionsFactory.CreateDefault()))
    {
    }

    public SpotifySearchDiagnostics(
        SpotifyMetadataProvider provider)
    {
        _provider =
            provider ??
            throw new ArgumentNullException(
                nameof(provider));
    }

    /// <summary>
    /// Busca la pista indicada y devuelve un informe con las
    /// grabaciones encontradas.
    /// </summary>
    public async Task<string> RunAsync(
        string artist,
        string title,
        CancellationToken cancellationToken = default)
    {
        SpotifySearchRequest request =
            new()
            {
                Artist =
                    artist,

                Title =
                    title
            };

        SpotifySearchResult result =
            await _provider.SearchAsync(
                request,
                cancellationToken);

        return BuildReport(
            result);
    }

    private static string BuildReport(
        SpotifySearchResult result)
    {
        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de SpotifyMetadataProvider ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Consulta: " +
            $"{result.Request?.SearchDisplay ?? "(sin solicitud)"}");

        builder.AppendLine(
            $"Estado: " +
            $"{result.Status}");

        builder.AppendLine(
            $"Mensaje: " +
            $"{result.Message}");

        builder.AppendLine(
            $"Candidatos encontrados: " +
            $"{result.Candidates.Count}");

        builder.AppendLine();

        foreach (SpotifySearchCandidate candidate
            in result.Candidates)
        {
            builder.AppendLine(
                $"- {candidate.DisplayName} " +
                $"(popularidad: {candidate.Popularity})");
        }

        builder.AppendLine();

        builder.AppendLine(
            "=== Fin del diagnóstico ===");

        return builder.ToString();
    }
}
