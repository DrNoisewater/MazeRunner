namespace MazeRunner.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Dark";
    public string Accent { get; set; } = "Amber";
    public int DefaultWidth { get; set; } = 31;
    public int DefaultHeight { get; set; } = 21;
    public int AnimationSpeed { get; set; } = 62;
    public bool ReduceMotion { get; set; }
    public bool ShowGrid { get; set; } = true;
    public bool ShowDirectionalMarkers { get; set; }
    public bool SaveHistory { get; set; } = true;
    public bool ConfirmBeforeClearingHistory { get; set; } = true;
    public double WindowWidth { get; set; } = 1520;
    public double WindowHeight { get; set; } = 930;
}
