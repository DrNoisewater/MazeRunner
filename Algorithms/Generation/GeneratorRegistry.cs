namespace MazeRunner.Algorithms.Generation;

public static class GeneratorRegistry
{
    public static IReadOnlyList<IMazeGenerator> All { get; } = new IMazeGenerator[]
    {
        new RecursiveBacktrackerGenerator(),
        new RandomizedPrimGenerator(),
        new RandomizedKruskalGenerator(),
        new RecursiveDivisionGenerator()
    };

    public static IMazeGenerator Find(string name) => All.First(item => item.Metadata.Name == name);
}
