namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Dtos;

/// <summary>
/// Representa la salida JSON entregada por fpcalc cuando
/// se invoca con el modificador "-json".
/// </summary>
public sealed class ChromaprintProcessOutputDto
{
    public double Duration { get; set; }

    public string? Fingerprint { get; set; }
}
