using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

public sealed class BreadthFirstSolver : IMazeSolver
{
    public AlgorithmMetadata Metadata { get; } = new(
        "Breadth-First Search",
        "Expands evenly in layers until it reaches the goal.",
        "Guarantees the shortest route in an unweighted maze.",
        "Explores broadly and does not optimize weighted travel cost.",
        GuaranteesShortestPath: true);

    public IEnumerable<AlgorithmStep> Solve(Maze maze)
    {
        Queue<MazePosition> queue = new();
        HashSet<MazePosition> visited = new() { maze.Start };
        Dictionary<MazePosition, MazePosition> parents = new();
        queue.Enqueue(maze.Start);
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
                queue.Enqueue(neighbor.Position);
                yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, neighbor.Position);
            }
        }

        foreach (AlgorithmStep step in SolverPath.Highlight(maze.Start, maze.Goal, parents)) yield return step;
    }
}
