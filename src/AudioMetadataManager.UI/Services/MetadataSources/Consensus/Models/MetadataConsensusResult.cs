using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;

/// <summary>
/// Representa el resultado completo del consenso de metadatos
/// para una pista.
///
/// Contiene una decisión independiente por campo y un resumen
/// global de confianza y revisión.
/// </summary>
public sealed class MetadataConsensusResult
{
    /// <summary>
    /// Identificador único de la evaluación.
    /// </summary>
    public Guid EvaluationId { get; init; } =
        Guid.NewGuid();

    /// <summary>
    /// Momento UTC en que se generó el resultado.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Resultados individuales por campo.
    /// </summary>
    public IReadOnlyList<MetadataConsensusFieldResult>
        Fields
    { get; init; } =
            Array.Empty<MetadataConsensusFieldResult>();

    /// <summary>
    /// Confianza global del consenso, entre 0 y 1.
    /// </summary>
    public double OverallConfidence { get; init; }

    /// <summary>
    /// Explicaciones y advertencias generales.
    /// </summary>
    public IReadOnlyList<string> Reasons { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Busca el resultado de un campo concreto.
    /// </summary>
    public MetadataConsensusFieldResult? GetField(
        MetadataField field)
    {
        return Fields.FirstOrDefault(
            result =>
                result.Field == field);
    }

    /// <summary>
    /// Campos para los que se seleccionó un valor.
    /// </summary>
    public IReadOnlyList<MetadataConsensusFieldResult>
        SelectedFields =>
            Fields
                .Where(
                    field =>
                        field.HasSelectedValue)
                .ToArray();

    /// <summary>
    /// Campos con conflictos sin resolver.
    /// </summary>
    public IReadOnlyList<MetadataConsensusFieldResult>
        ConflictedFields =>
            Fields
                .Where(
                    field =>
                        field.HasConflict)
                .ToArray();

    /// <summary>
    /// Cantidad de campos evaluados.
    /// </summary>
    public int EvaluatedFieldCount =>
        Fields.Count;

    /// <summary>
    /// Cantidad de campos con un valor seleccionado.
    /// </summary>
    public int SelectedFieldCount =>
        SelectedFields.Count;

    /// <summary>
    /// Cantidad de conflictos detectados.
    /// </summary>
    public int ConflictCount =>
        ConflictedFields.Count;

    /// <summary>
    /// Indica si existe al menos una decisión utilizable.
    /// </summary>
    public bool HasConsensusData =>
        SelectedFieldCount > 0;

    /// <summary>
    /// Indica si el resultado completo requiere revisión
    /// manual.
    /// </summary>
    public bool RequiresManualReview =>
        Fields.Any(
            field =>
                field.RequiresManualReview);

    /// <summary>
    /// Confianza global preparada para mostrarse.
    /// </summary>
    public string OverallConfidenceDisplay =>
        $"{Math.Clamp(OverallConfidence, 0, 1) * 100:0.00}%";

    /// <summary>
    /// Resumen compacto del resultado global.
    /// </summary>
    public string Summary
    {
        get
        {
            if (!HasConsensusData)
            {
                return
                    "No se obtuvo información suficiente para " +
                    "generar un consenso de metadatos.";
            }

            return
                $"Campos evaluados: {EvaluatedFieldCount}. " +
                $"Campos seleccionados: {SelectedFieldCount}. " +
                $"Conflictos: {ConflictCount}. " +
                $"Confianza global: " +
                $"{OverallConfidenceDisplay}. " +
                $"Revisión manual: " +
                $"{(RequiresManualReview ? "Sí" : "No")}.";
        }
    }
}