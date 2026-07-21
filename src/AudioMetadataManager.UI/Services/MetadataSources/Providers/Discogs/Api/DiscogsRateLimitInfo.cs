namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Api;

/// <summary>
/// Representa la información de límites de solicitudes
/// comunicada por Discogs mediante encabezados HTTP.
/// </summary>
public sealed class DiscogsRateLimitInfo
{
    /// <summary>
    /// Cantidad total de solicitudes permitidas en la ventana
    /// actual, cuando Discogs informa este valor.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Cantidad de solicitudes utilizadas durante la ventana
    /// actual.
    /// </summary>
    public int? Used { get; init; }

    /// <summary>
    /// Cantidad de solicitudes restantes.
    /// </summary>
    public int? Remaining { get; init; }

    /// <summary>
    /// Indica si se obtuvo al menos un dato de límite.
    /// </summary>
    public bool HasInformation =>
        Limit.HasValue ||
        Used.HasValue ||
        Remaining.HasValue;

    /// <summary>
    /// Indica si el servidor informa que no quedan solicitudes
    /// disponibles.
    /// </summary>
    public bool IsExhausted =>
        Remaining.HasValue &&
        Remaining.Value <= 0;

    /// <summary>
    /// Descripción legible para diagnósticos.
    /// </summary>
    public string DisplayText
    {
        get
        {
            if (!HasInformation)
            {
                return
                    "Información de límite no disponible.";
            }

            string limit =
                Limit?.ToString() ??
                "?";

            string used =
                Used?.ToString() ??
                "?";

            string remaining =
                Remaining?.ToString() ??
                "?";

            return
                $"Límite: {limit} · " +
                $"Utilizadas: {used} · " +
                $"Restantes: {remaining}";
        }
    }
}