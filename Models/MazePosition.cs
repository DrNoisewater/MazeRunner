namespace MazeRunner.Models;

public readonly record struct MazePosition(int X, int Y)
{
    public MazePosition Offset(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY);
    public int ManhattanDistanceTo(MazePosition other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    public override string ToString() => $"({X}, {Y})";
}
