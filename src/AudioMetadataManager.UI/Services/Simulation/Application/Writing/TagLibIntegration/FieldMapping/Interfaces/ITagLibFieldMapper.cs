using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping.Interfaces;

/// <summary>
/// Define la traducción entre los campos internos del programa
/// y las propiedades expuestas por TagLibSharp.
/// </summary>
public interface ITagLibFieldMapper
{
    /// <summary>
    /// Indica si el campo puede ser administrado por este
    /// mapper.
    /// </summary>
    bool IsSupported(
        MetadataField field);

    /// <summary>
    /// Lee el valor actual de un campo desde una etiqueta
    /// TagLibSharp.
    /// </summary>
    string ReadValue(
        TagLib.Tag tag,
        MetadataField field);

    /// <summary>
    /// Prepara un cambio dentro de la representación TagLibSharp
    /// y comprueba inmediatamente el valor asignado en memoria.
    ///
    /// Este método no ejecuta Save().
    /// </summary>
    TagLibFieldMappingResult PrepareChange(
        TagLib.Tag tag,
        MetadataFieldChange change);

    /// <summary>
    /// Prepara un cambio utilizando el archivo TagLibSharp
    /// completo cuando el formato requiere una etiqueta
    /// específica.
    ///
    /// Este método no ejecuta Save().
    /// </summary>
    TagLibFieldMappingResult PrepareChange(
        TagLib.File file,
        MetadataFieldChange change);

    /// <summary>
    /// Normaliza un valor para lectura, escritura y comparación.
    /// </summary>
    string NormalizeValue(
        string? value);

    /// <summary>
    /// Compara dos valores utilizando las reglas comunes del
    /// mapper.
    /// </summary>
    bool ValuesEqual(
        string? firstValue,
        string? secondValue);
}
