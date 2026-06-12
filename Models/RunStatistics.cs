using MazeRunner.Infrastructure;

namespace MazeRunner.Models;

public sealed class RunStatistics : ObservableObject
{
    private MazeRunState _state = MazeRunState.Idle;
    private int _visitedCells;
    private int _exploredCells;
    private int _frontierCount;
    private int _solutionLength;
    private int _weightedPathCost;
    private TimeSpan _generationElapsed;
    private TimeSpan _solveElapsed;

    public MazeRunState State { get => _state; set => SetProperty(ref _state, value); }
    public int VisitedCells { get => _visitedCells; set => SetProperty(ref _visitedCells, value); }
    public int ExploredCells { get => _exploredCells; set => SetProperty(ref _exploredCells, value); }
    public int FrontierCount { get => _frontierCount; set => SetProperty(ref _frontierCount, value); }
    public int SolutionLength { get => _solutionLength; set => SetProperty(ref _solutionLength, value); }
    public int WeightedPathCost { get => _weightedPathCost; set => SetProperty(ref _weightedPathCost, value); }
    public TimeSpan GenerationElapsed { get => _generationElapsed; set => SetProperty(ref _generationElapsed, value); }
    public TimeSpan SolveElapsed { get => _solveElapsed; set => SetProperty(ref _solveElapsed, value); }

    public double SearchEfficiency(int totalCells) => totalCells <= 0 || ExploredCells <= 0
        ? 0
        : Math.Min(100.0, SolutionLength / (double)ExploredCells * 100.0);

    public void ResetGeneration()
    {
        State = MazeRunState.Idle;
        VisitedCells = 0;
        ExploredCells = 0;
        FrontierCount = 0;
        SolutionLength = 0;
        WeightedPathCost = 0;
        GenerationElapsed = TimeSpan.Zero;
        SolveElapsed = TimeSpan.Zero;
    }

    public void ResetSearch()
    {
        ExploredCells = 0;
        FrontierCount = 0;
        SolutionLength = 0;
        WeightedPathCost = 0;
        SolveElapsed = TimeSpan.Zero;
    }
}
