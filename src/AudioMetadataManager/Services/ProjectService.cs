using System.Text.Json;
using AudioMetadataManager.Models;
namespace AudioMetadataManager.Services;
public static class ProjectService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public static async Task SaveAsync(string path, SimulationProject project)
    {
        project.UpdatedAt = DateTime.Now;
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, project, Options);
    }
    public static async Task<SimulationProject> LoadAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<SimulationProject>(stream, Options) ?? throw new InvalidDataException("Proyecto no válido.");
    }
}
