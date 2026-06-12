namespace MazeRunner.Models;

public sealed class RunHistoryEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int Width { get; set; }
    public int Height { get; set; }
    public int Seed { get; set; }
    public string Generator { get; set; } = string.Empty;
    public string Solver { get; set; } = string.Empty;
    public int ExploredCells { get; set; }
    public int SolutionLength { get; set; }
    public int WeightedPathCost { get; set; }
    public double ElapsedMilliseconds { get; set; }
    public double EfficiencyPercent { get; set; }
}
