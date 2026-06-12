namespace MazeRunner.Models;

public sealed class MazeCell
{
    public MazeCell(MazePosition position)
    {
        Position = position;
    }

    public MazePosition Position { get; }
    public MazeWall Walls { get; set; } = MazeWall.All;
    public int TerrainCost { get; set; } = 1;
    public bool HasWall(MazeWall wall) => (Walls & wall) == wall;
}
