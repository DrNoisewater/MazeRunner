using MazeRunner.Models;

namespace MazeRunner.Algorithms.Generation;

public sealed class RecursiveBacktrackerGenerator : IMazeGenerator
{
    public AlgorithmMetadata Metadata { get; } = new(
        "Recursive Backtracker",
        "Depth-first carving with long corridors and visible retreat behavior.",
        "Fast, deterministic, and visually easy to follow.",
        "Often creates fewer short branches than Prim's algorithm.",
        CarvesWalls: true);

    public IEnumerable<AlgorithmStep> Generate(Maze maze, Random random)
    {
        yield return new AlgorithmStep(AlgorithmStepType.ResetWalls, maze.Start);
        HashSet<MazePosition> visited = new() { maze.Start };
        Stack<MazePosition> stack = new();
        stack.Push(maze.Start);
        yield return new AlgorithmStep(AlgorithmStepType.VisitCell, maze.Start);

        while (stack.Count > 0)
        {
            MazePosition current = stack.Peek();
            yield return new AlgorithmStep(AlgorithmStepType.MarkCurrent, current);

            List<MazeNeighbor> candidates = maze.GetNeighbors(current)
                .Where(neighbor => !visited.Contains(neighbor.Position))
                .ToList();

            if (candidates.Count == 0)
            {
                stack.Pop();
                yield return new AlgorithmStep(AlgorithmStepType.Backtrack, current);
                continue;
            }

            MazeNeighbor chosen = candidates[random.Next(candidates.Count)];
            visited.Add(chosen.Position);
            stack.Push(chosen.Position);
            yield return new AlgorithmStep(AlgorithmStepType.CarvePassage, current, chosen.Position);
            yield return new AlgorithmStep(AlgorithmStepType.VisitCell, chosen.Position);
        }

        yield return new AlgorithmStep(AlgorithmStepType.Complete, maze.Goal, Message: "Generation complete");
    }
}
