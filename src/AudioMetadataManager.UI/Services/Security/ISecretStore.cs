namespace AudioMetadataManager.UI.Services.Security;

/// <summary>
/// Define un almacenamiento genérico para secretos locales.
///
/// Las implementaciones pueden utilizar el Administrador de
/// credenciales de Windows u otro almacén seguro compatible.
/// </summary>
public interface ISecretStore
{
    /// <summary>
    /// Guarda o reemplaza un secreto asociado a un identificador.
    /// </summary>
    void SaveSecret(
        string targetName,
        string secret);

    /// <summary>
    /// Recupera un secreto.
    ///
    /// Devuelve null cuando el identificador no existe.
    /// </summary>
    string? ReadSecret(
        string targetName);

    /// <summary>
    /// Elimina un secreto.
    ///
    /// Devuelve true si se eliminó o si ya no existía.
    /// </summary>
    bool DeleteSecret(
        string targetName);

    /// <summary>
    /// Comprueba si existe un secreto.
    /// </summary>
    bool ContainsSecret(
        string targetName);
}