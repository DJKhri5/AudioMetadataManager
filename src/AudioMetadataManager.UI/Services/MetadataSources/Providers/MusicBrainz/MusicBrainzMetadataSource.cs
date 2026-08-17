using AudioMetadataManager.UI.Services.MetadataSources.Interfaces;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Providers.MusicBrainz;

/// <summary>
/// Fuente de metadatos oficial basada en la base de datos abierta de MusicBrainz.
/// Está disponible inmediatamente sin requerir configuración de API key.
/// </summary>
public sealed class MusicBrainzMetadataSource : IMetadataSource, IDisposable
{
    private readonly MusicBrainzMetadataProvider _provider;
    private readonly bool _ownsProvider;

    public MusicBrainzMetadataSource(MusicBrainzMetadataProvider? provider = null)
    {
        if (provider != null)
        {
            _provider = provider;
            _ownsProvider = false;
        }
        else
        {
            _provider = new MusicBrainzMetadataProvider();
            _ownsProvider = true;
        }
    }

    public string Name => "MusicBrainz";

    public int Priority => 1;

    public bool IsAvailable => true;

    public bool RequiresManualApproval => false;

    public async Task<MetadataSearchResult> SearchAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string artist = !string.IsNullOrWhiteSpace(request.ParsedArtist)
            ? request.ParsedArtist
            : request.TaggedArtist;

        string title = !string.IsNullOrWhiteSpace(request.ParsedTitle)
            ? request.ParsedTitle
            : request.TaggedTitle;

        var candidates = await _provider.SearchRecordingsAsync(artist, title, cancellationToken).ConfigureAwait(false);

        return new MetadataSearchResult
        {
            SourceName = Name,
            QueryUsed = request.PrimaryQuery,
            WasSuccessful = candidates.Count > 0,
            Candidates = candidates.ToList(),
            ErrorMessage = candidates.Count == 0 ? "No se encontraron coincidencias en MusicBrainz." : string.Empty
        };
    }

    public void Dispose()
    {
        if (_ownsProvider)
        {
            _provider.Dispose();
        }
    }
}
