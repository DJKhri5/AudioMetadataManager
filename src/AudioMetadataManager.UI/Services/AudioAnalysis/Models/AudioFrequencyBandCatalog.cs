namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Proporciona las definiciones frecuenciales utilizadas
/// por el perfil tonal.
///
/// El catálogo adapta automáticamente las bandas a la
/// frecuencia de Nyquist disponible en cada archivo.
/// No calcula energía ni interpreta calidad.
/// </summary>
public static class AudioFrequencyBandCatalog
{
    /// <summary>
    /// Frecuencia inferior predeterminada del perfil tonal.
    /// </summary>
    public const double DefaultMinimumFrequencyHz =
        20.0;

    /// <summary>
    /// Construye las definiciones predeterminadas ajustadas
    /// a la frecuencia de Nyquist indicada.
    /// </summary>
    public static IReadOnlyList<AudioFrequencyBandDefinition>
        CreateDefault(
            double nyquistFrequencyHz)
    {
        ValidateNyquistFrequency(
            nyquistFrequencyHz);

        List<AudioFrequencyBandDefinition> definitions =
            new();

        AddIfAvailable(
            definitions,
            AudioFrequencyBand.SubBass,
            "Subgraves",
            20,
            60,
            nyquistFrequencyHz);

        AddIfAvailable(
            definitions,
            AudioFrequencyBand.Bass,
            "Graves",
            60,
            250,
            nyquistFrequencyHz);

        AddIfAvailable(
            definitions,
            AudioFrequencyBand.LowMidrange,
            "Medios bajos",
            250,
            500,
            nyquistFrequencyHz);

        AddIfAvailable(
            definitions,
            AudioFrequencyBand.Midrange,
            "Medios",
            500,
            2000,
            nyquistFrequencyHz);

        AddIfAvailable(
            definitions,
            AudioFrequencyBand.UpperMidrange,
            "Medios altos",
            2000,
            6000,
            nyquistFrequencyHz);

        AddIfAvailable(
            definitions,
            AudioFrequencyBand.Treble,
            "Agudos",
            6000,
            12000,
            nyquistFrequencyHz);

        AddIfAvailable(
            definitions,
            AudioFrequencyBand.Air,
            "Aire",
            12000,
            nyquistFrequencyHz,
            nyquistFrequencyHz);

        ValidateDefinitions(
            definitions);

        return definitions
            .AsReadOnly();
    }

    /// <summary>
    /// Obtiene una definición concreta desde una colección.
    /// </summary>
    public static AudioFrequencyBandDefinition?
        Find(
            IEnumerable<AudioFrequencyBandDefinition> definitions,
            AudioFrequencyBand band)
    {
        ArgumentNullException.ThrowIfNull(
            definitions);

        return definitions.FirstOrDefault(
            definition =>
                definition.Band ==
                band);
    }

    /// <summary>
    /// Obtiene una definición concreta y genera una excepción
    /// cuando la banda no está disponible.
    /// </summary>
    public static AudioFrequencyBandDefinition
        GetRequired(
            IEnumerable<AudioFrequencyBandDefinition> definitions,
            AudioFrequencyBand band)
    {
        AudioFrequencyBandDefinition? definition =
            Find(
                definitions,
                band);

        if (definition is not null)
        {
            return definition;
        }

        throw new InvalidOperationException(
            $"La banda \"{band}\" no está disponible " +
            "para el rango frecuencial actual.");
    }

    /// <summary>
    /// Agrega una banda únicamente cuando existe al menos
    /// una parte válida dentro del espectro disponible.
    /// </summary>
    private static void AddIfAvailable(
        ICollection<AudioFrequencyBandDefinition> definitions,
        AudioFrequencyBand band,
        string displayName,
        double requestedMinimumFrequencyHz,
        double requestedMaximumFrequencyHz,
        double nyquistFrequencyHz)
    {
        double minimumFrequencyHz =
            Math.Max(
                0,
                requestedMinimumFrequencyHz);

        double maximumFrequencyHz =
            Math.Min(
                requestedMaximumFrequencyHz,
                nyquistFrequencyHz);

        if (maximumFrequencyHz <=
            minimumFrequencyHz)
        {
            return;
        }

        AudioFrequencyBandDefinition definition =
            new()
            {
                Band =
                    band,

                DisplayName =
                    displayName,

                MinimumFrequencyHz =
                    minimumFrequencyHz,

                MaximumFrequencyHz =
                    maximumFrequencyHz
            };

        definition.Validate();

        definitions.Add(
            definition);
    }

    /// <summary>
    /// Comprueba que Nyquist sea un valor utilizable.
    /// </summary>
    private static void ValidateNyquistFrequency(
        double nyquistFrequencyHz)
    {
        if (nyquistFrequencyHz <= 0 ||
            double.IsNaN(nyquistFrequencyHz) ||
            double.IsInfinity(nyquistFrequencyHz))
        {
            throw new ArgumentOutOfRangeException(
                nameof(nyquistFrequencyHz),
                nyquistFrequencyHz,
                "La frecuencia de Nyquist debe ser " +
                "finita y mayor que cero.");
        }
    }

    /// <summary>
    /// Comprueba que las definiciones no estén repetidas
    /// ni superpuestas.
    /// </summary>
    private static void ValidateDefinitions(
        IReadOnlyList<AudioFrequencyBandDefinition> definitions)
    {
        if (definitions.Count == 0)
        {
            throw new InvalidOperationException(
                "No fue posible construir ninguna banda " +
                "para el rango frecuencial disponible.");
        }

        HashSet<AudioFrequencyBand> registeredBands =
            new();

        double previousMaximumFrequencyHz =
            double.NegativeInfinity;

        foreach (
            AudioFrequencyBandDefinition definition
            in definitions)
        {
            definition.Validate();

            if (!registeredBands.Add(
                    definition.Band))
            {
                throw new InvalidOperationException(
                    $"La banda \"{definition.Band}\" " +
                    "está definida más de una vez.");
            }

            if (definition.MinimumFrequencyHz <
                previousMaximumFrequencyHz)
            {
                throw new InvalidOperationException(
                    "Las definiciones de bandas " +
                    "frecuenciales se superponen.");
            }

            previousMaximumFrequencyHz =
                definition.MaximumFrequencyHz;
        }
    }
}