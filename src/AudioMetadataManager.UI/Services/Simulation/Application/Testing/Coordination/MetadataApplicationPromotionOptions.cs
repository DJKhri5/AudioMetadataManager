namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Configura el comportamiento de una promoción controlada.
///
/// Las opciones de simulación existen únicamente para comprobar
/// los mecanismos de seguridad sobre archivos temporales.
/// </summary>
public sealed class MetadataApplicationPromotionOptions
{
    /// <summary>
    /// Indica si debe simularse un fallo después de sustituir el
    /// destino y antes de aceptar la verificación final.
    ///
    /// Esta opción permite probar la reversión automática sin
    /// corromper archivos ni depender de fallos externos.
    /// </summary>
    public bool SimulatePostReplacementVerificationFailure
    { get; init; }

    /// <summary>
    /// Indica si la copia verificada debe permanecer disponible
    /// después de completar la promoción o la reversión.
    /// </summary>
    public bool PreserveVerifiedWorkingCopy { get; init; } =
        true;

    /// <summary>
    /// Configuración predeterminada para una promoción normal.
    /// </summary>
    public static MetadataApplicationPromotionOptions
        SafeDefault =>
            new()
            {
                SimulatePostReplacementVerificationFailure =
                    false,

                PreserveVerifiedWorkingCopy =
                    true
            };

    /// <summary>
    /// Configuración exclusiva para pruebas temporales de
    /// reversión automática.
    /// </summary>
    public static MetadataApplicationPromotionOptions
        SimulatedVerificationFailure =>
            new()
            {
                SimulatePostReplacementVerificationFailure =
                    true,

                PreserveVerifiedWorkingCopy =
                    true
            };
}