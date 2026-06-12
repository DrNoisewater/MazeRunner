using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

/// <summary>
/// Calculates the canonical route displayed after a solve finishes.
/// With ordinary terrain this is the shortest route by number of steps.
/// When weighted terrain is enabled it becomes the lowest-cost valid route.
/// </summary>
internal static class OptimalPathFinder
{
    public static List<MazePosition> Find(Maze maze)
    {
        PriorityQueue<MazePosition, int> queue = new();
        Dictionary<MazePosition, int> distances = new() { [maze.Start] = 0 };
        Dictionary<MazePosition, MazePosition> parents = new();
        HashSet<MazePosition> settled = new();

        queue.Enqueue(maze.Start, 0);

        while (queue.Count > 0)
        {
            MazePosition current = queue.Dequeue();
            if (!settled.Add(current)) continue;
            if (current == maze.Goal) break;

            foreach (MazeNeighbor neighbor in maze.GetOpenNeighbors(current))
            {
                int candidate = distances[current] + maze[neighbor.Position].TerrainCost;
                if (distances.TryGetValue(neighbor.Position, out int existing) && existing <= candidate)
                {
                    continue;
                }

                distances[neighbor.Position] = candidate;
                parents[neighbor.Position] = current;
                queue.Enqueue(neighbor.Position, candidate);
            }
        }

        return SolverPath.Reconstruct(maze.Start, maze.Goal, parents);
    }
}
