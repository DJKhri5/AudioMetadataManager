using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Renaming;
using AudioMetadataManager.UI.Services.Simulation.Planning.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;

namespace AudioMetadataManager.UI.Services.Simulation;

/// <summary>
/// Servicio puente que sincroniza los metadatos canónicos obtenidos desde las fuentes externas
/// (o editados manualmente por el usuario en la simulación) con el modelo de renombrado seguro del archivo.
/// </summary>
public sealed class SimulationPlanToRenamingSynchronizer
{
    private readonly SafeFileNameSanitizer _sanitizer;

    public SimulationPlanToRenamingSynchronizer(SafeFileNameSanitizer? sanitizer = null)
    {
        _sanitizer = sanitizer ?? new SafeFileNameSanitizer();
    }

    /// <summary>
    /// Sincroniza los metadatos de un MetadataChangePlan hacia el modelo AudioFile.Simulation
    /// y recalcula el nombre de archivo propuesto.
    /// </summary>
    public void Synchronize(AudioFile audioFile, MetadataChangePlan changePlan)
    {
        if (audioFile == null || changePlan == null)
        {
            return;
        }

        string effectiveArtist = ExtractValue(changePlan, MetadataField.Artist, audioFile.Artist);
        string effectiveTitle = ExtractValue(changePlan, MetadataField.Title, audioFile.Title);
        string effectiveVersion = ExtractValue(changePlan, MetadataField.Version, audioFile.Version);
        string effectiveLabel = ExtractValue(changePlan, MetadataField.Label, audioFile.Label);

        UpdateSimulationModel(audioFile, effectiveArtist, effectiveTitle, effectiveVersion, effectiveLabel);
    }

    /// <summary>
    /// Sincroniza las modificaciones interactivas de la vista SimulationPlanViewModel
    /// hacia el modelo AudioFile.Simulation.
    /// </summary>
    public void Synchronize(AudioFile audioFile, SimulationPlanViewModel planViewModel)
    {
        if (audioFile == null || planViewModel == null)
        {
            return;
        }

        string effectiveArtist = ExtractValue(planViewModel, MetadataField.Artist, audioFile.Artist);
        string effectiveTitle = ExtractValue(planViewModel, MetadataField.Title, audioFile.Title);
        string effectiveVersion = ExtractValue(planViewModel, MetadataField.Version, audioFile.Version);
        string effectiveLabel = ExtractValue(planViewModel, MetadataField.Label, audioFile.Label);

        UpdateSimulationModel(audioFile, effectiveArtist, effectiveTitle, effectiveVersion, effectiveLabel);
    }

    private void UpdateSimulationModel(
        AudioFile audioFile,
        string artist,
        string title,
        string version,
        string label)
    {
        string rawProposedFileName = BuildCanonicalFileName(audioFile, artist, title, version);
        string sanitizedProposedFileName = _sanitizer.Sanitize(rawProposedFileName, audioFile.Extension);

        var simulation = audioFile.Simulation ?? new FileSimulationResult();

        simulation.OriginalFileName = audioFile.FileName;
        simulation.ProposedFileName = sanitizedProposedFileName;

        // Actualizar los cambios en la simulación
        simulation.Changes.Clear();

        if (!string.IsNullOrWhiteSpace(artist) && !string.Equals(audioFile.Artist, artist, StringComparison.Ordinal))
        {
            simulation.Changes.Add(new SimulationChange
            {
                PropertyName = "Artista",
                CurrentValue = audioFile.Artist,
                ProposedValue = artist,
                IsSelected = true,
                Description = "Actualizado desde fuentes externas."
            });
        }

        if (!string.IsNullOrWhiteSpace(title) && !string.Equals(audioFile.Title, title, StringComparison.Ordinal))
        {
            simulation.Changes.Add(new SimulationChange
            {
                PropertyName = "Título",
                CurrentValue = audioFile.Title,
                ProposedValue = title,
                IsSelected = true,
                Description = "Actualizado desde fuentes externas."
            });
        }

        if (!string.IsNullOrWhiteSpace(version) && !string.Equals(audioFile.Version, version, StringComparison.Ordinal))
        {
            simulation.Changes.Add(new SimulationChange
            {
                PropertyName = "Versión",
                CurrentValue = audioFile.Version,
                ProposedValue = version,
                IsSelected = true,
                Description = "Actualizado desde fuentes externas."
            });
        }

        if (!string.IsNullOrWhiteSpace(label) && !string.Equals(audioFile.Label, label, StringComparison.Ordinal))
        {
            simulation.Changes.Add(new SimulationChange
            {
                PropertyName = "Sello",
                CurrentValue = audioFile.Label,
                ProposedValue = label,
                IsSelected = true,
                Description = "Actualizado desde fuentes externas."
            });
        }

        if (!string.Equals(audioFile.FileName, sanitizedProposedFileName, StringComparison.OrdinalIgnoreCase))
        {
            simulation.Changes.Add(new SimulationChange
            {
                PropertyName = "Nombre de archivo",
                CurrentValue = audioFile.FileName,
                ProposedValue = sanitizedProposedFileName,
                IsSelected = true,
                Description = "Nombre canónico generado a partir de metadatos externos."
            });
        }

        simulation.Summary = simulation.HasChanges
            ? $"{simulation.ChangeCount} cambio(s) propuesto(s) desde fuentes externas."
            : "No se detectaron cambios aplicables.";

        audioFile.Simulation = simulation;
    }

    private static string BuildCanonicalFileName(
        AudioFile audioFile,
        string artist,
        string title,
        string version)
    {
        if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(title))
        {
            return audioFile.FileName;
        }

        string ext = (audioFile.Extension ?? string.Empty).Trim();
        if (!ext.StartsWith('.'))
        {
            ext = "." + ext;
        }

        string versionPart = string.IsNullOrWhiteSpace(version)
            ? string.Empty
            : $" ({version.Trim()})";

        return $"{artist.Trim()} - {title.Trim()}{versionPart}{ext}";
    }

    private static string ExtractValue(MetadataChangePlan plan, MetadataField field, string fallback)
    {
        var proposal = plan.Proposals.FirstOrDefault(p => p.Field == field);
        if (proposal != null && !string.IsNullOrWhiteSpace(proposal.ProposedValue))
        {
            return proposal.ProposedValue.Trim();
        }

        return fallback;
    }

    private static string ExtractValue(SimulationPlanViewModel viewModel, MetadataField field, string fallback)
    {
        var proposal = viewModel.Proposals.FirstOrDefault(p => p.Field == field);
        if (proposal != null && !string.IsNullOrWhiteSpace(proposal.EffectiveProposedValue))
        {
            return proposal.EffectiveProposedValue.Trim();
        }

        return fallback;
    }
}
