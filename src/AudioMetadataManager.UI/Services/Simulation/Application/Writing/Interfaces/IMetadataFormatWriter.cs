using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Interfaces;

/// <summary>
/// Contrato implementado por cada escritor especializado en
/// un formato o familia de formatos de audio.
/// </summary>
public interface IMetadataFormatWriter
{
    /// <summary>
    /// Nombre técnico preparado para diagnósticos.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Extensiones que el escritor declara soportar.
    /// Deben incluir el punto inicial.
    /// </summary>
    IReadOnlySet<string> SupportedExtensions { get; }

    /// <summary>
    /// Indica si el escritor puede procesar la extensión
    /// indicada.
    /// </summary>
    bool CanWrite(
        string extension);

    /// <summary>
    /// Ejecuta la escritura correspondiente al formato.
    ///
    /// La implementación debe preservar la información no
    /// incluida en la solicitud siempre que el contenedor lo
    /// permita.
    /// </summary>
    Task<MetadataWriteResult> WriteAsync(
        MetadataWriteRequest request,
        CancellationToken cancellationToken = default);
}