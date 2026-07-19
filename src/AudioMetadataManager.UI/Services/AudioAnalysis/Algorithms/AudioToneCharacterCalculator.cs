using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;

/// <summary>
/// Determina características tonales descriptivas a partir
/// del perfil tonal y del balance tonal ya calculados.
///
/// Esta clase no abre archivos, no procesa PCM y no ejecuta FFT.
///
/// El resultado público contiene únicamente un carácter
/// principal y caracteres secundarios.
/// </summary>
public class AudioToneCharacterCalculator
{
    /// <summary>
    /// Puntuación mínima necesaria para conservar una
    /// característica como secundaria.
    /// </summary>
    private const double SecondaryScoreThreshold =
        0.45;

    /// <summary>
    /// Calcula la caracterización tonal del audio.
    /// </summary>
    public AudioToneCharacterResult Calculate(
        AudioToneProfile toneProfile,
        AudioToneBalanceProfile balanceProfile)
    {
        ArgumentNullException.ThrowIfNull(
            toneProfile);

        ArgumentNullException.ThrowIfNull(
            balanceProfile);

        if (!toneProfile.IsValid ||
            !balanceProfile.IsValid)
        {
            return new AudioToneCharacterResult();
        }

        Dictionary<AudioToneCharacter, double> scores =
            new();

        EvaluateRegionalCharacter(
            balanceProfile,
            scores);

        EvaluateDarkCharacter(
            toneProfile,
            balanceProfile,
            scores);

        EvaluateBrightCharacter(
            toneProfile,
            balanceProfile,
            scores);

        EvaluateWarmCharacter(
            toneProfile,
            balanceProfile,
            scores);

        EvaluateThinCharacter(
            toneProfile,
            balanceProfile,
            scores);

        if (scores.Count == 0)
        {
            AddScore(
                scores,
                AudioToneCharacter.Balanced,
                1.0);
        }

        List<KeyValuePair<AudioToneCharacter, double>>
            orderedScores =
                scores
                    .OrderByDescending(
                        item =>
                            item.Value)
                    .ThenBy(
                        item =>
                            item.Key)
                    .ToList();

        AudioToneCharacterResult result =
            new()
            {
                PrimaryCharacter =
                    orderedScores[0].Key
            };

        double primaryScore =
            orderedScores[0].Value;

        foreach (
            KeyValuePair<AudioToneCharacter, double> item
            in orderedScores.Skip(1))
        {
            if (item.Value <
                primaryScore *
                SecondaryScoreThreshold)
            {
                continue;
            }

            if (item.Key ==
                result.PrimaryCharacter)
            {
                continue;
            }

            result.SecondaryCharacters.Add(
                item.Key);
        }

        return result;
    }

    /// <summary>
    /// Determina el predominio entre las regiones bajas,
    /// medias y altas.
    /// </summary>
    private static void EvaluateRegionalCharacter(
        AudioToneBalanceProfile balanceProfile,
        IDictionary<AudioToneCharacter, double> scores)
    {
        double low =
            balanceProfile.LowFrequencyEnergyRatio;

        double mid =
            balanceProfile.MidFrequencyEnergyRatio;

        double high =
            balanceProfile.HighFrequencyEnergyRatio;

        double maximum =
            Math.Max(
                low,
                Math.Max(
                    mid,
                    high));

        double minimum =
            Math.Min(
                low,
                Math.Min(
                    mid,
                    high));

        if (maximum - minimum <= 0.15)
        {
            AddScore(
                scores,
                AudioToneCharacter.Balanced,
                1.0);

            return;
        }

        if (low == maximum)
        {
            AddScore(
                scores,
                AudioToneCharacter.BassDominant,
                CalculateDominanceScore(
                    low,
                    mid,
                    high));

            return;
        }

        if (mid == maximum)
        {
            AddScore(
                scores,
                AudioToneCharacter.MidrangeDominant,
                CalculateDominanceScore(
                    mid,
                    low,
                    high));

            return;
        }

        AddScore(
            scores,
            AudioToneCharacter.TrebleDominant,
            CalculateDominanceScore(
                high,
                low,
                mid));
    }

    /// <summary>
    /// Evalúa un posible carácter oscuro.
    /// </summary>
    private static void EvaluateDarkCharacter(
        AudioToneProfile toneProfile,
        AudioToneBalanceProfile balanceProfile,
        IDictionary<AudioToneCharacter, double> scores)
    {
        AudioFrequencyBandMeasurement? treble =
            toneProfile.Find(
                AudioFrequencyBand.Treble);

        AudioFrequencyBandMeasurement? air =
            toneProfile.Find(
                AudioFrequencyBand.Air);

        double score = 0;

        if (balanceProfile.HighFrequencyEnergyRatio <= 0.08)
        {
            score += 0.55;
        }

        if (treble is not null &&
            treble.IsValid &&
            treble.AveragePersistenceRatio <= 0.60)
        {
            score += 0.20;
        }

        if (air is not null &&
            air.IsValid &&
            air.AveragePersistenceRatio <= 0.30)
        {
            score += 0.25;
        }

        AddScoreWhenPositive(
            scores,
            AudioToneCharacter.Dark,
            score);
    }

    /// <summary>
    /// Evalúa un posible carácter brillante.
    /// </summary>
    private static void EvaluateBrightCharacter(
        AudioToneProfile toneProfile,
        AudioToneBalanceProfile balanceProfile,
        IDictionary<AudioToneCharacter, double> scores)
    {
        AudioFrequencyBandMeasurement? treble =
            toneProfile.Find(
                AudioFrequencyBand.Treble);

        AudioFrequencyBandMeasurement? air =
            toneProfile.Find(
                AudioFrequencyBand.Air);

        double score = 0;

        if (balanceProfile.HighFrequencyEnergyRatio >= 0.18)
        {
            score += 0.55;
        }

        if (treble is not null &&
            treble.IsValid &&
            treble.AveragePersistenceRatio >= 0.70)
        {
            score += 0.25;
        }

        if (air is not null &&
            air.IsValid &&
            air.AveragePersistenceRatio >= 0.45)
        {
            score += 0.20;
        }

        AddScoreWhenPositive(
            scores,
            AudioToneCharacter.Bright,
            score);
    }

    /// <summary>
    /// Evalúa un posible carácter cálido.
    /// </summary>
    private static void EvaluateWarmCharacter(
        AudioToneProfile toneProfile,
        AudioToneBalanceProfile balanceProfile,
        IDictionary<AudioToneCharacter, double> scores)
    {
        AudioFrequencyBandMeasurement? bass =
            toneProfile.Find(
                AudioFrequencyBand.Bass);

        AudioFrequencyBandMeasurement? lowMidrange =
            toneProfile.Find(
                AudioFrequencyBand.LowMidrange);

        if (bass is null ||
            lowMidrange is null ||
            !bass.IsValid ||
            !lowMidrange.IsValid)
        {
            return;
        }

        double combinedEnergy =
            bass.TotalEnergyRatio +
            lowMidrange.TotalEnergyRatio;

        double score = 0;

        if (combinedEnergy >= 0.30)
        {
            score += 0.55;
        }

        if (bass.AveragePersistenceRatio >= 0.70)
        {
            score += 0.20;
        }

        if (balanceProfile.HighFrequencyEnergyRatio <= 0.15)
        {
            score += 0.25;
        }

        AddScoreWhenPositive(
            scores,
            AudioToneCharacter.Warm,
            score);
    }

    /// <summary>
    /// Evalúa un posible carácter delgado.
    /// </summary>
    private static void EvaluateThinCharacter(
        AudioToneProfile toneProfile,
        AudioToneBalanceProfile balanceProfile,
        IDictionary<AudioToneCharacter, double> scores)
    {
        AudioFrequencyBandMeasurement? subBass =
            toneProfile.Find(
                AudioFrequencyBand.SubBass);

        AudioFrequencyBandMeasurement? bass =
            toneProfile.Find(
                AudioFrequencyBand.Bass);

        double score = 0;

        if (balanceProfile.LowFrequencyEnergyRatio <= 0.20)
        {
            score += 0.65;
        }

        if (subBass is not null &&
            bass is not null &&
            subBass.IsValid &&
            bass.IsValid)
        {
            double averageLowPersistence =
                (
                    subBass.AveragePersistenceRatio +
                    bass.AveragePersistenceRatio) /
                    2.0;

            if (averageLowPersistence <= 0.35)
            {
                score += 0.35;
            }
        }

        AddScoreWhenPositive(
            scores,
            AudioToneCharacter.Thin,
            score);
    }

    /// <summary>
    /// Calcula una puntuación de predominio regional.
    /// </summary>
    private static double CalculateDominanceScore(
        double dominantValue,
        double firstComparison,
        double secondComparison)
    {
        double strongestComparison =
            Math.Max(
                firstComparison,
                secondComparison);

        double difference =
            Math.Max(
                0,
                dominantValue -
                strongestComparison);

        return Math.Clamp(
            0.50 +
            difference,
            0,
            1);
    }

    /// <summary>
    /// Agrega una puntuación cuando el valor es positivo.
    /// </summary>
    private static void AddScoreWhenPositive(
        IDictionary<AudioToneCharacter, double> scores,
        AudioToneCharacter character,
        double score)
    {
        if (score <= 0)
        {
            return;
        }

        AddScore(
            scores,
            character,
            Math.Clamp(
                score,
                0,
                1));
    }

    /// <summary>
    /// Agrega o acumula puntuación para una característica.
    /// </summary>
    private static void AddScore(
        IDictionary<AudioToneCharacter, double> scores,
        AudioToneCharacter character,
        double score)
    {
        if (character ==
            AudioToneCharacter.InsufficientData)
        {
            return;
        }

        double normalizedScore =
            Math.Clamp(
                score,
                0,
                1);

        if (scores.TryGetValue(
                character,
                out double currentScore))
        {
            scores[character] =
                Math.Clamp(
                    currentScore +
                    normalizedScore,
                    0,
                    1);

            return;
        }

        scores[character] =
            normalizedScore;
    }

    /// <summary>
    /// Convierte una característica tonal a texto en español.
    /// </summary>
    public static string GetDisplayName(
        AudioToneCharacter character)
    {
        return character switch
        {
            AudioToneCharacter.Balanced =>
                "Equilibrado",

            AudioToneCharacter.BassDominant =>
                "Predominio grave",

            AudioToneCharacter.MidrangeDominant =>
                "Predominio medio",

            AudioToneCharacter.TrebleDominant =>
                "Predominio agudo",

            AudioToneCharacter.Dark =>
                "Oscuro",

            AudioToneCharacter.Bright =>
                "Brillante",

            AudioToneCharacter.Warm =>
                "Cálido",

            AudioToneCharacter.Thin =>
                "Delgado",

            _ =>
                "Sin clasificación suficiente"
        };
    }
}