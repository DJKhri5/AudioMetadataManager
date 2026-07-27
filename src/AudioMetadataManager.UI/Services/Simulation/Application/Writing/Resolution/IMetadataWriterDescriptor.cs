namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Resolution;

/// <summary>
/// Expone metadatos de selección para un escritor.
/// </summary>
public interface IMetadataWriterDescriptor
{
    /// <summary>
    /// Tipo operativo del escritor.
    /// </summary>
    MetadataWriterKind WriterKind { get; }

    /// <summary>
    /// Prioridad de resolución.
    ///
    /// Un valor mayor tiene preferencia.
    /// </summary>
    int ResolutionPriority { get; }
}