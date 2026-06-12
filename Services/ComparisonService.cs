using System.Diagnostics;
using MazeRunner.Algorithms.Solving;
using MazeRunner.Models;

namespace MazeRunner.Services;

public sealed record SolverComparisonResult(
    string Solver,
    int Explored,
    int PathLength,
    int PathCost,
    double ElapsedMilliseconds,
    double Efficiency);

public static class ComparisonService
{
    public static SolverComparisonResult Evaluate(Maze maze, IMazeSolver solver)
    {
        HashSet<MazePosition> explored = new();
        List<MazePosition> path = new();
        Stopwatch stopwatch = Stopwatch.StartNew();
        foreach (AlgorithmStep step in solver.Solve(maze))
        {
            if (step.Type == AlgorithmStepType.MarkExplored) explored.Add(step.Position);
            if (step.Type == AlgorithmStepType.HighlightSolution) path.Add(step.Position);
        }
        stopwatch.Stop();
        int cost = path.Skip(1).Sum(position => maze[position].TerrainCost);
        double efficiency = explored.Count == 0 ? 0 : Math.Min(100.0, path.Count / (double)explored.Count * 100.0);
        return new SolverComparisonResult(solver.Metadata.Name, explored.Count, path.Count, cost, stopwatch.Elapsed.TotalMilliseconds, efficiency);
    }
}
