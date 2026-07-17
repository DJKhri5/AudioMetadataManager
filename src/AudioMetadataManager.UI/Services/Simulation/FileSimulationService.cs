using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Simulation;

public class FileSimulationService
{
    public FileSimulationResult Build(AudioFile audioFile)
    {
        FileSimulationResult simulation = new();

        string parsedArtist =
            audioFile.ParsedName?.Artist ?? string.Empty;

        string parsedTitle =
            audioFile.ParsedName?.Title ?? string.Empty;

        string parsedVersion =
            audioFile.ParsedName?.Version ?? string.Empty;

        simulation.OriginalFileName =
            audioFile.FileName;

        simulation.ProposedFileName =
            BuildNewFileName(
                audioFile,
                parsedArtist,
                parsedTitle,
                parsedVersion);

        simulation.ConfidenceScore =
            audioFile.Analysis?.ConfidenceScore ?? 0;

        simulation.RequiresManualReview =
            simulation.ConfidenceScore < 80;

        simulation.CanApplyAutomatically =
            !simulation.RequiresManualReview;

        simulation.Changes.Add(
            CreateChange(
                "Artista",
                audioFile.Artist,
                parsedArtist));

        simulation.Changes.Add(
            CreateChange(
                "Título",
                audioFile.Title,
                parsedTitle));

        simulation.Changes.Add(
            CreateChange(
                "Versión",
                GetCurrentVersion(audioFile.Title),
                parsedVersion));

        simulation.Changes.Add(
            CreateChange(
                "Nombre de archivo",
                audioFile.FileName,
                simulation.ProposedFileName));

        simulation.Summary =
            BuildSummary(simulation);

        return simulation;
    }

    private static SimulationChange CreateChange(
        string propertyName,
        string? currentValue,
        string? proposedValue)
    {
        string current =
            currentValue?.Trim() ?? string.Empty;

        string proposed =
            proposedValue?.Trim() ?? string.Empty;

        return new SimulationChange
        {
            PropertyName = propertyName,
            CurrentValue = current,
            ProposedValue = proposed,

            /*
             * No seleccionamos automáticamente una propuesta vacía.
             * Esto evita borrar metadatos válidos cuando el parser
             * no consiguió obtener un valor confiable.
             */
            IsSelected =
                !string.IsNullOrWhiteSpace(proposed),

            Description =
                BuildChangeDescription(
                    propertyName,
                    current,
                    proposed)
        };
    }

    private static string BuildNewFileName(
        AudioFile audioFile,
        string parsedArtist,
        string parsedTitle,
        string parsedVersion)
    {
        /*
         * Si el parser no obtuvo artista o título, conservamos
         * el nombre original. No debemos inventar "Artista"
         * o "Título", porque más adelante podría aplicarse por error.
         */
        if (string.IsNullOrWhiteSpace(parsedArtist) ||
            string.IsNullOrWhiteSpace(parsedTitle))
        {
            return audioFile.FileName;
        }

        string versionPart =
            string.IsNullOrWhiteSpace(parsedVersion)
                ? string.Empty
                : $" ({parsedVersion.Trim()})";

        string extension =
            NormalizeExtension(audioFile.Extension);

        return
            $"{parsedArtist.Trim()} - " +
            $"{parsedTitle.Trim()}" +
            $"{versionPart}" +
            $"{extension}";
    }

    private static string GetCurrentVersion(string taggedTitle)
    {
        /*
         * En el modelo actual no existe una etiqueta Version
         * independiente. Por ahora dejamos este valor vacío.
         * Más adelante podremos extraerla de la etiqueta Title.
         */
        return string.Empty;
    }

    private static string BuildChangeDescription(
        string propertyName,
        string currentValue,
        string proposedValue)
    {
        if (string.IsNullOrWhiteSpace(proposedValue))
        {
            return
                $"No existe una propuesta confiable para {propertyName}.";
        }

        if (string.Equals(
                currentValue,
                proposedValue,
                StringComparison.Ordinal))
        {
            return
                $"{propertyName} no requiere cambios.";
        }

        return
            $"Se propone actualizar {propertyName}.";
    }

    private static string BuildSummary(
        FileSimulationResult simulation)
    {
        if (!simulation.HasChanges)
        {
            return "No se detectaron cambios aplicables.";
        }

        string reviewText =
            simulation.RequiresManualReview
                ? "Requiere revisión manual."
                : "Puede revisarse para aplicación automática.";

        return
            $"{simulation.ChangeCount} cambio(s) detectado(s). " +
            reviewText;
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        string normalized =
            extension.Trim().ToLowerInvariant();

        return normalized.StartsWith('.')
            ? normalized
            : $".{normalized}";
    }
}