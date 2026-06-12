using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

public sealed class DepthFirstSolver : IMazeSolver
{
    public AlgorithmMetadata Metadata { get; } = new(
        "Depth-First Search",
        "Dives down one route at a time and retreats from dead ends.",
        "Uses little memory and creates an expressive backtracking animation.",
        "Does not guarantee the shortest route.");

    public IEnumerable<AlgorithmStep> Solve(Maze maze)
    {
        Stack<MazePosition> stack = new();
        HashSet<MazePosition> visited = new() { maze.Start };
        Dictionary<MazePosition, MazePosition> parents = new();
        stack.Push(maze.Start);
        yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, maze.Start);

        while (stack.Count > 0)
        {
            MazePosition current = stack.Pop();
            yield return new AlgorithmStep(AlgorithmStepType.PopFrontier, current);
            yield return new AlgorithmStep(AlgorithmStepType.MarkCurrent, current);
            yield return new AlgorithmStep(AlgorithmStepType.MarkExplored, current);
            if (current == maze.Goal) break;

            List<MazeNeighbor> neighbors = maze.GetOpenNeighbors(current).Reverse().ToList();
            bool pushedAny = false;
            foreach (MazeNeighbor neighbor in neighbors)
            {
                if (!visited.Add(neighbor.Position)) continue;
                parents[neighbor.Position] = current;
                stack.Push(neighbor.Position);
                pushedAny = true;
                yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, neighbor.Position);
            }

            if (!pushedAny)
            {
                yield return new AlgorithmStep(AlgorithmStepType.Backtrack, current);
            }
        }

        foreach (AlgorithmStep step in SolverPath.Highlight(maze.Start, maze.Goal, parents)) yield return step;
    }
}
