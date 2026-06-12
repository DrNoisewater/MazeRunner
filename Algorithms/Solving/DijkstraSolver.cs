using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

public sealed class DijkstraSolver : IMazeSolver
{
    public AlgorithmMetadata Metadata { get; } = new(
        "Dijkstra's Algorithm",
        "Explores the lowest accumulated travel-cost route first.",
        "Guarantees the cheapest route when terrain costs vary.",
        "Often examines more cells than A* because it has no goal heuristic.",
        GuaranteesShortestPath: true,
        SupportsWeightedTerrain: true);

    public IEnumerable<AlgorithmStep> Solve(Maze maze)
    {
        PriorityQueue<MazePosition, int> queue = new();
        Dictionary<MazePosition, int> distances = new() { [maze.Start] = 0 };
        Dictionary<MazePosition, MazePosition> parents = new();
        HashSet<MazePosition> explored = new();
        queue.Enqueue(maze.Start, 0);
        yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, maze.Start);

        while (queue.Count > 0)
        {
            MazePosition current = queue.Dequeue();
            if (!explored.Add(current)) continue;
            yield return new AlgorithmStep(AlgorithmStepType.PopFrontier, current);
            yield return new AlgorithmStep(AlgorithmStepType.MarkCurrent, current);
            yield return new AlgorithmStep(AlgorithmStepType.MarkExplored, current);
            if (current == maze.Goal) break;

            foreach (MazeNeighbor neighbor in maze.GetOpenNeighbors(current))
            {
                int candidate = distances[current] + maze[neighbor.Position].TerrainCost;
                if (distances.TryGetValue(neighbor.Position, out int existing) && existing <= candidate) continue;
                distances[neighbor.Position] = candidate;
                parents[neighbor.Position] = current;
                queue.Enqueue(neighbor.Position, candidate);
                yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, neighbor.Position);
            }
        }

        foreach (AlgorithmStep step in SolverPath.Highlight(maze.Start, maze.Goal, parents)) yield return step;
    }
}
