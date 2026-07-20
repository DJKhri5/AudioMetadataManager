namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison.Comparers;

/// <summary>
/// Define el contrato común de un comparador especializado
/// de campos de metadatos.
///
/// Cada implementación se responsabiliza de comparar un
/// tipo concreto de información, por ejemplo artista,
/// título, versión, género o sello.
/// </summary>
public interface IMetadataFieldComparer
{
    /// <summary>
    /// Nombre estable del campo que compara.
    ///
    /// Ejemplos:
    /// Artist
    /// Title
    /// Version
    /// Album
    /// </summary>
    string FieldName { get; }

    /// <summary>
    /// Orden de ejecución dentro de una comparación completa.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Compara el valor local con el valor obtenido desde
    /// una fuente de referencia.
    ///
    /// El comparador solamente describe la relación entre
    /// ambos valores. No aplica cambios al archivo.
    /// </summary>
    MetadataFieldComparisonResult Compare(
        string? localValue,
        string? referenceValue);
}