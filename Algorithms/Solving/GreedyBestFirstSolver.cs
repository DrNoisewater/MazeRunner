using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

public sealed class GreedyBestFirstSolver : IMazeSolver
{
    public AlgorithmMetadata Metadata { get; } = new(
        "Greedy Best-First Search",
        "Follows the cell that appears closest to the goal by Manhattan distance.",
        "Can reach the goal quickly in favorable layouts.",
        "Does not guarantee the shortest or cheapest route.");

    public IEnumerable<AlgorithmStep> Solve(Maze maze)
    {
        PriorityQueue<MazePosition, int> queue = new();
        HashSet<MazePosition> visited = new() { maze.Start };
        Dictionary<MazePosition, MazePosition> parents = new();
        queue.Enqueue(maze.Start, 0);
        yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, maze.Start);

        while (queue.Count > 0)
        {
            MazePosition current = queue.Dequeue();
            yield return new AlgorithmStep(AlgorithmStepType.PopFrontier, current);
            yield return new AlgorithmStep(AlgorithmStepType.MarkCurrent, current);
            yield return new AlgorithmStep(AlgorithmStepType.MarkExplored, current);
            if (current == maze.Goal) break;

            foreach (MazeNeighbor neighbor in maze.GetOpenNeighbors(current))
            {
                if (!visited.Add(neighbor.Position)) continue;
                parents[neighbor.Position] = current;
                queue.Enqueue(neighbor.Position, neighbor.Position.ManhattanDistanceTo(maze.Goal));
                yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, neighbor.Position);
            }
        }

        foreach (AlgorithmStep step in SolverPath.Highlight(maze.Start, maze.Goal, parents)) yield return step;
    }
}
