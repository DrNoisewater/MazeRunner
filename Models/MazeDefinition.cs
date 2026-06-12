namespace MazeRunner.Models;

public sealed class MazeDefinition
{
    public int Version { get; set; } = 1;
    public int Width { get; set; }
    public int Height { get; set; }
    public int Seed { get; set; }
    public string Generator { get; set; } = string.Empty;
    public int LoopsPercent { get; set; }
    public MazePosition Start { get; set; }
    public MazePosition Goal { get; set; }
    public List<CellDefinition> Cells { get; set; } = new();
}

public sealed class CellDefinition
{
    public int X { get; set; }
    public int Y { get; set; }
    public MazeWall Walls { get; set; }
    public int TerrainCost { get; set; } = 1;
}
