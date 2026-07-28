using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Contracts;

/// <summary>
/// Define una etapa independiente del pipeline de aplicación
/// de metadatos.
///
/// Cada etapa trabaja sobre una única instancia de
/// MetadataApplicationContext.
/// </summary>
public interface IMetadataApplicationStage
{
    /// <summary>
    /// Identidad funcional de la etapa.
    /// </summary>
    MetadataApplicationStage Stage { get; }

    /// <summary>
    /// Nombre legible utilizado para diagnóstico y auditoría.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Orden predeterminado de ejecución.
    ///
    /// Los valores menores se ejecutan primero.
    /// </summary>
    int ExecutionOrder { get; }

    /// <summary>
    /// Ejecuta la etapa utilizando el contexto compartido.
    /// </summary>
    Task ExecuteAsync(
        MetadataApplicationContext context);
}