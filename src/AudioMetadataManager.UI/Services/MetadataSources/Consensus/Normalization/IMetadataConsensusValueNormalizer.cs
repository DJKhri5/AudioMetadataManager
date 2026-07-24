using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Normalization;

/// <summary>
/// Define el contrato para normalizar valores antes de
/// agruparlos dentro del motor de consenso.
/// </summary>
public interface IMetadataConsensusValueNormalizer
{
    /// <summary>
    /// Normaliza un valor según el campo al que pertenece.
    /// </summary>
    string Normalize(
        MetadataField field,
        string? value);
}