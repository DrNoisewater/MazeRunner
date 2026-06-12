using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

internal static class SolverPath
{
    public static IEnumerable<AlgorithmStep> Highlight(
        MazePosition start,
        MazePosition goal,
        IReadOnlyDictionary<MazePosition, MazePosition> parents)
    {
        List<MazePosition> path = Reconstruct(start, goal, parents);
        foreach (MazePosition position in path)
        {
            yield return new AlgorithmStep(AlgorithmStepType.HighlightSolution, position);
        }

        yield return new AlgorithmStep(AlgorithmStepType.Complete, goal, Message: path.Count > 0 ? "Solved" : "No route found");
    }

    public static List<MazePosition> Reconstruct(
        MazePosition start,
        MazePosition goal,
        IReadOnlyDictionary<MazePosition, MazePosition> parents)
    {
        if (start == goal) return new List<MazePosition> { start };
        if (!parents.ContainsKey(goal)) return new List<MazePosition>();

        List<MazePosition> path = new() { goal };
        MazePosition current = goal;
        while (current != start)
        {
            current = parents[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
