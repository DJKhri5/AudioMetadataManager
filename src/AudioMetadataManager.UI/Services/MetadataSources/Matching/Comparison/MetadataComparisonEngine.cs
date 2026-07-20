using System;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison.Comparers;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;

/// <summary>
/// Motor responsable de comparar
/// dos conjuntos de metadatos.
///
/// Este componente irá creciendo de forma
/// incremental conforme se incorporen nuevos
/// campos y reglas de comparación.
/// </summary>
public sealed class MetadataComparisonEngine
{
    private readonly IReadOnlyList<IMetadataFieldComparer>
        _comparers;

    /// <summary>
    /// Crea el motor con los comparadores predeterminados.
    /// </summary>
    public MetadataComparisonEngine()
        : this(CreateDefaultComparers())
    {
    }

    /// <summary>
    /// Crea el motor con una colección personalizada
    /// de comparadores.
    ///
    /// Este constructor facilita las pruebas y permite
    /// incorporar nuevos comparadores sin modificar
    /// la lógica interna del motor.
    /// </summary>
    public MetadataComparisonEngine(
        IEnumerable<IMetadataFieldComparer> comparers)
    {
        ArgumentNullException.ThrowIfNull(
            comparers);

        List<IMetadataFieldComparer> orderedComparers =
            comparers
                .Where(comparer => comparer is not null)
                .OrderBy(comparer => comparer.Order)
                .ToList();

        if (orderedComparers.Count == 0)
        {
            throw new ArgumentException(
                "Debe registrarse al menos un comparador.",
                nameof(comparers));
        }

        _comparers =
            orderedComparers;
    }

    /// <summary>
    /// Agrega un resultado de comparación al conjunto.
    /// </summary>
    private static void AddField(
        MetadataComparisonResult comparison,
        MetadataFieldComparisonResult field)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        ArgumentNullException.ThrowIfNull(field);

        comparison.Fields.Add(field);
    }

    /// <summary>
    /// Crea un resultado vacío de comparación.
    ///
    /// Este método servirá como punto de partida para
    /// las futuras comparaciones completas.
    /// </summary>
    public MetadataComparisonResult CreateEmpty()
    {
        return new MetadataComparisonResult();
    }

    /// <summary>
    /// Compara dos conjuntos normalizados de metadatos y reúne
    /// los resultados de todos los campos actualmente soportados.
    ///
    /// Este método no modifica archivos ni selecciona
    /// automáticamente qué valor debe conservarse.
    /// </summary>
    public MetadataComparisonResult CompareMetadata(
        MetadataComparisonInput localMetadata,
        MetadataComparisonInput referenceMetadata)
    {
        ArgumentNullException.ThrowIfNull(
            localMetadata);

        ArgumentNullException.ThrowIfNull(
            referenceMetadata);

        MetadataComparisonResult comparison =
            new()
            {
                LocalSourceName =
                    localMetadata.SourceDisplayName,

                ReferenceSourceName =
                    referenceMetadata.SourceDisplayName
            };

        AddField(
            comparison,
            CompareArtist(
                localMetadata.Artist,
                referenceMetadata.Artist));

        AddField(
            comparison,
            CompareTitle(
                localMetadata.Title,
                referenceMetadata.Title));

        AddField(
            comparison,
            CompareVersion(
                localMetadata.Version,
                referenceMetadata.Version));

        AddField(
            comparison,
            CompareAlbum(
                localMetadata.Album,
                referenceMetadata.Album));

        AddField(
            comparison,
            CompareGenre(
                localMetadata.Genre,
                referenceMetadata.Genre));

        AddField(
            comparison,
            CompareLabel(
                localMetadata.Label,
                referenceMetadata.Label));

        return comparison;
    }

    /// <summary>
    /// Compara el artista local con el artista obtenido desde
    /// una fuente de referencia.
    ///
    /// Utiliza el comparador especializado registrado para
    /// el campo Artist.
    /// </summary>
    public MetadataFieldComparisonResult CompareArtist(
        string? localValue,
        string? referenceValue)
    {
        return CompareField(
            "Artist",
            localValue,
            referenceValue);
    }

    /// <summary>
    /// Compara el título local con el título obtenido desde
    /// una fuente de referencia.
    /// </summary>
    public MetadataFieldComparisonResult CompareTitle(
        string? localValue,
        string? referenceValue)
    {
        return CompareField(
            "Title",
            localValue,
            referenceValue);
    }

    /// <summary>
    /// Compara la versión local con la versión obtenida desde
    /// una fuente de referencia.
    /// </summary>
    public MetadataFieldComparisonResult CompareVersion(
        string? localValue,
        string? referenceValue)
    {
        return CompareField(
            "Version",
            localValue,
            referenceValue);
    }

    /// <summary>
    /// Compara el álbum local con el álbum obtenido desde
    /// una fuente de referencia.
    /// </summary>
    public MetadataFieldComparisonResult CompareAlbum(
        string? localValue,
        string? referenceValue)
    {
        return CompareField(
            "Album",
            localValue,
            referenceValue);
    }

    /// <summary>
    /// Compara el género local con el género obtenido desde
    /// una fuente de referencia.
    /// </summary>
    public MetadataFieldComparisonResult CompareGenre(
        string? localValue,
        string? referenceValue)
    {
        return CompareField(
            "Genre",
            localValue,
            referenceValue);
    }

    /// <summary>
    /// Compara el sello local con el sello obtenido desde
    /// una fuente de referencia.
    /// </summary>
    public MetadataFieldComparisonResult CompareLabel(
        string? localValue,
        string? referenceValue)
    {
        return CompareField(
            "Label",
            localValue,
            referenceValue);
    }

    /// <summary>
    /// Compara un campo utilizando el comparador especializado
    /// registrado para su nombre.
    ///
    /// Este método centraliza la búsqueda del comparador y evita
    /// repetir la misma lógica en Artist, Title, Version, Album,
    /// Genre, Label y campos futuros.
    /// </summary>
    public MetadataFieldComparisonResult CompareField(
        string fieldName,
        string? localValue,
        string? referenceValue)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException(
                "El nombre del campo no puede estar vacío.",
                nameof(fieldName));
        }

        string normalizedFieldName =
            fieldName.Trim();

        IMetadataFieldComparer? comparer =
            _comparers
                .FirstOrDefault(
                    registeredComparer =>
                        string.Equals(
                            registeredComparer.FieldName,
                            normalizedFieldName,
                            StringComparison.OrdinalIgnoreCase));

        if (comparer is null)
        {
            throw new InvalidOperationException(
                $"No existe un comparador registrado para " +
                $"el campo \"{normalizedFieldName}\".");
        }

        return comparer.Compare(
            localValue,
            referenceValue);
    }

    /// <summary>
    /// Compara dos valores de texto utilizando igualdad simple.
    /// Este método servirá como base para las comparaciones
    /// especializadas que se implementarán posteriormente.
    /// </summary>
    public MetadataFieldComparisonResult CompareText(
        string fieldName,
        string? localValue,
        string? referenceValue)
    {
        if (string.IsNullOrWhiteSpace(localValue) &&
            string.IsNullOrWhiteSpace(referenceValue))
        {
            return new MetadataFieldComparisonResult
            {
                FieldName = fieldName,
                Status = MetadataFieldComparisonStatus.MissingBothValues,
                LocalValue = localValue ?? string.Empty,
                ReferenceValue = referenceValue ?? string.Empty,
                Similarity = 0,
                Explanation = "Ninguna de las dos fuentes contiene un valor."
            };
        }

        if (string.IsNullOrWhiteSpace(localValue))
        {
            return new MetadataFieldComparisonResult
            {
                FieldName = fieldName,
                Status = MetadataFieldComparisonStatus.MissingLocalValue,
                LocalValue = string.Empty,
                ReferenceValue = referenceValue ?? string.Empty,
                Similarity = 0,
                Explanation = "No existe un valor local."
            };
        }

        if (string.IsNullOrWhiteSpace(referenceValue))
        {
            return new MetadataFieldComparisonResult
            {
                FieldName = fieldName,
                Status = MetadataFieldComparisonStatus.MissingReferenceValue,
                LocalValue = localValue,
                ReferenceValue = string.Empty,
                Similarity = 0,
                Explanation = "La fuente de referencia no contiene un valor."
            };
        }

        bool equals =
            string.Equals(
                localValue,
                referenceValue,
                StringComparison.Ordinal);

        return new MetadataFieldComparisonResult
        {
            FieldName = fieldName,
            LocalValue = localValue,
            ReferenceValue = referenceValue,
            Similarity = equals ? 1.0 : 0.0,
            Status = equals
                ? MetadataFieldComparisonStatus.ExactMatch
                : MetadataFieldComparisonStatus.Conflict,
            Explanation = equals
                ? "Los valores son idénticos."
                : "Los valores son diferentes."
        };
    }

    /// <summary>
    /// Construye la colección predeterminada
    /// de comparadores del motor.
    ///
    /// Los comparadores se ordenarán posteriormente
    /// mediante su propiedad Order.
    /// </summary>
    private static IReadOnlyList<IMetadataFieldComparer>
        CreateDefaultComparers()
    {
        return new List<IMetadataFieldComparer>
        {
            new ArtistComparer(),

            new TextFieldComparer(
                fieldName: "Title",
                order: 20),

            new TextFieldComparer(
                fieldName: "Version",
                order: 30),

            new TextFieldComparer(
                fieldName: "Album",
                order: 40),

            new TextFieldComparer(
                fieldName: "Genre",
                order: 50),

            new TextFieldComparer(
                fieldName: "Label",
                order: 60)
        };
    }
}