using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

/// <summary>
/// Define la importancia relativa de un campo dentro del
/// cálculo de confianza global.
///
/// Este modelo no compara valores. Únicamente describe cuánto
/// aporta un campo y si debe considerarse crítico.
/// </summary>
public sealed class MetadataFieldWeight
{
    /// <summary>
    /// Campo de metadatos asociado a esta configuración.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Peso relativo del campo dentro del cálculo global.
    ///
    /// Se expresa como un valor entre 0 y 1.
    /// Ejemplo:
    /// 0.30 = 30 %.
    /// </summary>
    public double Weight { get; init; }

    /// <summary>
    /// Indica si un conflicto en este campo debe influir
    /// directamente en la decisión final.
    /// </summary>
    public bool IsCritical { get; init; }

    /// <summary>
    /// Indica si la configuración contiene un campo y un peso
    /// utilizables.
    /// </summary>
    public bool IsValid =>
        Field != MetadataField.Unknown &&
        Weight > 0 &&
        Weight <= 1;

    /// <summary>
    /// Nombre legible del campo.
    /// </summary>
    public string FieldDisplay =>
        Field switch
        {
            MetadataField.Artist => "Artist",
            MetadataField.Title => "Title",
            MetadataField.Version => "Version",
            MetadataField.Album => "Album",
            MetadataField.Label => "Label",
            MetadataField.Genre => "Genre",
            _ => "Unknown"
        };

    /// <summary>
    /// Peso preparado para mostrarse como porcentaje.
    /// </summary>
    public string WeightDisplay =>
        $"{Math.Clamp(Weight, 0, 1) * 100.0:0.##}%";
}