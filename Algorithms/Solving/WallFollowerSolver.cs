using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

public sealed class WallFollowerSolver : IMazeSolver
{
    private static readonly MazeWall[] Clockwise =
    {
        MazeWall.North,
        MazeWall.East,
        MazeWall.South,
        MazeWall.West
    };

    public AlgorithmMetadata Metadata { get; } = new(
        "Wall Follower",
        "Simulates a person keeping their right hand against a wall.",
        "Produces an intuitive physical traversal animation.",
        "Heuristic only: it is not generally optimal and may revisit cells.");

    public IEnumerable<AlgorithmStep> Solve(Maze maze)
    {
        MazePosition current = maze.Start;
        MazeWall direction = MazeWall.East;
        List<MazePosition> route = new() { current };
        int guard = Math.Max(maze.CellCount * 12, 100);

        for (int iteration = 0; iteration < guard && current != maze.Goal; iteration++)
        {
            yield return new AlgorithmStep(AlgorithmStepType.MarkCurrent, current);
            yield return new AlgorithmStep(AlgorithmStepType.MarkExplored, current);
            MazeWall right = TurnRight(direction);
            MazeWall chosen;
            if (CanMove(current, right)) chosen = right;
            else if (CanMove(current, direction)) chosen = direction;
            else if (CanMove(current, TurnLeft(direction))) chosen = TurnLeft(direction);
            else chosen = direction.Opposite();

            (int dx, int dy) = chosen.ToOffset();
            current = current.Offset(dx, dy);
            direction = chosen;
            route.Add(current);
            yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, current);
        }

        if (current != maze.Goal)
        {
            yield return new AlgorithmStep(AlgorithmStepType.Complete, maze.Goal, Message: "Wall follower stopped without reaching the goal");
            yield break;
        }

        foreach (MazePosition position in route)
        {
            yield return new AlgorithmStep(AlgorithmStepType.HighlightSolution, position);
        }
        yield return new AlgorithmStep(AlgorithmStepType.Complete, maze.Goal, Message: "Solved heuristically");

        bool CanMove(MazePosition position, MazeWall wall) => !maze[position].HasWall(wall);
    }

    private static MazeWall TurnRight(MazeWall wall)
    {
        int index = Array.IndexOf(Clockwise, wall);
        return Clockwise[(index + 1) % Clockwise.Length];
    }

    private static MazeWall TurnLeft(MazeWall wall)
    {
        int index = Array.IndexOf(Clockwise, wall);
        return Clockwise[(index + Clockwise.Length - 1) % Clockwise.Length];
    }
}
