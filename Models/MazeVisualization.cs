namespace MazeRunner.Models;

public sealed class MazeVisualization
{
    public HashSet<MazePosition> GeneratorVisited { get; } = new();
    public HashSet<MazePosition> Frontier { get; } = new();
    public HashSet<MazePosition> Explored { get; } = new();
    public HashSet<MazePosition> Backtracked { get; } = new();
    public List<MazePosition> SolutionPath { get; } = new();
    public MazePosition? CurrentCell { get; set; }
    public bool ShowSearchActivity { get; set; } = true;

    public void ClearGeneration()
    {
        GeneratorVisited.Clear();
        Frontier.Clear();
        Backtracked.Clear();
        CurrentCell = null;
    }

    public void ClearSearch()
    {
        Frontier.Clear();
        Explored.Clear();
        Backtracked.Clear();
        SolutionPath.Clear();
        CurrentCell = null;
        ShowSearchActivity = true;
    }

    public void HideSearchActivity()
    {
        ShowSearchActivity = false;
        Frontier.Clear();
        Backtracked.Clear();
        CurrentCell = null;
    }

    public void ClearAll()
    {
        GeneratorVisited.Clear();
        ClearSearch();
    }
}
