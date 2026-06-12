namespace MazeRunner.Models;

[Flags]
public enum MazeWall
{
    None = 0,
    North = 1,
    East = 2,
    South = 4,
    West = 8,
    All = North | East | South | West
}

public static class MazeWallExtensions
{
    public static MazeWall Opposite(this MazeWall wall) => wall switch
    {
        MazeWall.North => MazeWall.South,
        MazeWall.East => MazeWall.West,
        MazeWall.South => MazeWall.North,
        MazeWall.West => MazeWall.East,
        _ => throw new ArgumentOutOfRangeException(nameof(wall), wall, "A single cardinal wall is required.")
    };

    public static (int DeltaX, int DeltaY) ToOffset(this MazeWall wall) => wall switch
    {
        MazeWall.North => (0, -1),
        MazeWall.East => (1, 0),
        MazeWall.South => (0, 1),
        MazeWall.West => (-1, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(wall), wall, "A single cardinal wall is required.")
    };
}
