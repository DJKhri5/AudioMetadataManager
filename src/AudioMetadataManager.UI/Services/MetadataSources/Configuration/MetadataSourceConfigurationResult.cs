namespace AudioMetadataManager.UI.Services.MetadataSources
    .Configuration;

/// <summary>
/// Contiene el resultado de una operación de configuración
/// realizada sobre una fuente externa de metadatos.
/// </summary>
public sealed class MetadataSourceConfigurationResult
{
    /// <summary>
    /// Nombre de la fuente evaluada.
    /// </summary>
    public string SourceName { get; init; } =
        string.Empty;

    /// <summary>
    /// Estado resultante de la configuración.
    /// </summary>
    public MetadataSourceConfigurationState State { get; init; } =
        MetadataSourceConfigurationState.Unknown;

    /// <summary>
    /// Indica si la operación solicitada terminó correctamente.
    /// </summary>
    public bool OperationSucceeded { get; init; }

    /// <summary>
    /// Mensaje apto para mostrar en la interfaz.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si existe una configuración local guardada.
    /// </summary>
    public bool IsConfigured =>
        State is
            MetadataSourceConfigurationState.Configured or
            MetadataSourceConfigurationState.ConnectionVerified;

    /// <summary>
    /// Indica si la configuración fue validada mediante una
    /// conexión real con la plataforma.
    /// </summary>
    public bool IsConnectionVerified =>
        State ==
        MetadataSourceConfigurationState.ConnectionVerified;

    /// <summary>
    /// Construye un resultado satisfactorio.
    /// </summary>
    public static MetadataSourceConfigurationResult Success(
        string sourceName,
        MetadataSourceConfigurationState state,
        string message)
    {
        return new MetadataSourceConfigurationResult
        {
            SourceName = sourceName,
            State = state,
            OperationSucceeded = true,
            Message = message
        };
    }

    /// <summary>
    /// Construye un resultado fallido.
    /// </summary>
    public static MetadataSourceConfigurationResult Failure(
        string sourceName,
        MetadataSourceConfigurationState state,
        string message)
    {
        return new MetadataSourceConfigurationResult
        {
            SourceName = sourceName,
            State = state,
            OperationSucceeded = false,
            Message = message
        };
    }
}