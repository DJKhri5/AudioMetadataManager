using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioMetadataManager.Models;

public sealed class AudioItem : INotifyPropertyChanged
{
    private bool _isSelected = true;
    public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
    public string FullPath { get; set; } = "";
    public string RelativePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string ProposedFileName { get; set; } = "";
    public string Extension { get; set; } = "";
    public long SizeBytes { get; set; }
    public string SizeDisplay => SizeBytes < 1024 * 1024 ? $"{SizeBytes / 1024d:0.0} KB" : $"{SizeBytes / 1024d / 1024d:0.0} MB";
    public string Artist { get; set; } = "";
    public string Title { get; set; } = "";
    public string Version { get; set; } = "";
    public string Album { get; set; } = "";
    public string Genre { get; set; } = "";
    public uint Year { get; set; }
    public TimeSpan Duration { get; set; }
    public int AudioBitrateKbps { get; set; }
    public int SampleRateHz { get; set; }
    public int BitsPerSample { get; set; }
    public int Channels { get; set; }
    public string Codec { get; set; } = "";
    public bool HasArtwork { get; set; }
    public string SourceUsed { get; set; } = "Nombre";
    public string Status { get; set; } = "Analizado";
    public string Warnings { get; set; } = "";
    public string DuplicateGroup { get; set; } = "";
    public string TechnicalSummary => $"{Codec} | {Duration:mm\\:ss} | {AudioBitrateKbps} kbps | {SampleRateHz / 1000d:0.#} kHz | {BitsPerSample} bit | {Channels} ch";
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
