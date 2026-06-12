using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

public sealed class AStarSolver : IMazeSolver
{
    public AlgorithmMetadata Metadata { get; } = new(
        "A* Search",
        "Combines accumulated path cost with a Manhattan-distance estimate to the goal.",
        "Usually explores fewer cells than Dijkstra while preserving optimal weighted routes.",
        "Its behavior depends on a suitable heuristic.",
        GuaranteesShortestPath: true,
        SupportsWeightedTerrain: true);

    public IEnumerable<AlgorithmStep> Solve(Maze maze)
    {
        PriorityQueue<MazePosition, int> queue = new();
        Dictionary<MazePosition, int> costs = new() { [maze.Start] = 0 };
        Dictionary<MazePosition, MazePosition> parents = new();
        HashSet<MazePosition> explored = new();
        queue.Enqueue(maze.Start, maze.Start.ManhattanDistanceTo(maze.Goal));
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
                int candidate = costs[current] + maze[neighbor.Position].TerrainCost;
                if (costs.TryGetValue(neighbor.Position, out int existing) && existing <= candidate) continue;
                costs[neighbor.Position] = candidate;
                parents[neighbor.Position] = current;
                int priority = candidate + neighbor.Position.ManhattanDistanceTo(maze.Goal);
                queue.Enqueue(neighbor.Position, priority);
                yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, neighbor.Position);
            }
        }

        foreach (AlgorithmStep step in SolverPath.Highlight(maze.Start, maze.Goal, parents)) yield return step;
    }
}
