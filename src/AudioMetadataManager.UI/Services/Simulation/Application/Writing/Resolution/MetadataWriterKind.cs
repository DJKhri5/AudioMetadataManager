namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Resolution;

/// <summary>
/// Clasifica el propósito operativo de un escritor.
/// </summary>
public enum MetadataWriterKind
{
    /// <summary>
    /// Tipo no identificado.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Escritor que sólo simula o diagnostica.
    /// </summary>
    Diagnostic = 1,

    /// <summary>
    /// Escritor capaz de modificar realmente el archivo.
    /// </summary>
    Real = 2
}