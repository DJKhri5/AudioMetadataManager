namespace AudioMetadataManager.Models;
public sealed class SimulationProject
{
    public string ProjectName { get; set; } = "Nueva simulación";
    public string RootFolder { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public string Version { get; set; } = "0.2.0";
    public List<AudioItem> Items { get; set; } = [];
}
