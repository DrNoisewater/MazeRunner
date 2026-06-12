namespace MazeRunner.Algorithms.Solving;

public static class SolverRegistry
{
    public static IReadOnlyList<IMazeSolver> All { get; } = new IMazeSolver[]
    {
        new BreadthFirstSolver(),
        new DepthFirstSolver(),
        new DijkstraSolver(),
        new AStarSolver(),
        new GreedyBestFirstSolver(),
        new BidirectionalBreadthFirstSolver(),
        new WallFollowerSolver()
    };

    public static IMazeSolver Find(string name) => All.First(item => item.Metadata.Name == name);
}
