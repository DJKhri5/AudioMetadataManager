using System.Collections.ObjectModel;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Resultado descriptivo de la caracterización tonal.
///
/// Contiene únicamente el carácter principal y los
/// caracteres secundarios detectados.
/// </summary>
public class AudioToneCharacterResult
{
    /// <summary>
    /// Carácter tonal principal.
    /// </summary>
    public AudioToneCharacter PrimaryCharacter { get; init; } =
        AudioToneCharacter.InsufficientData;

    /// <summary>
    /// Caracteres tonales secundarios detectados.
    /// </summary>
    public Collection<AudioToneCharacter>
        SecondaryCharacters
    { get; } =
            new();

    /// <summary>
    /// Indica si existe una caracterización principal
    /// utilizable.
    /// </summary>
    public bool IsValid =>
        PrimaryCharacter !=
            AudioToneCharacter.InsufficientData;

    /// <summary>
    /// Indica si existen caracteres secundarios.
    /// </summary>
    public bool HasSecondaryCharacters =>
        SecondaryCharacters.Count > 0;
}