using MazeRunner.Models;

namespace MazeRunner.Algorithms.Generation;

public sealed class RandomizedKruskalGenerator : IMazeGenerator
{
    public AlgorithmMetadata Metadata { get; } = new(
        "Randomized Kruskal",
        "Shuffles interior walls and merges disconnected regions with union-find.",
        "Creates an evenly distributed carving pattern with a distinctive animation.",
        "Less spatially continuous than frontier-based generators.",
        CarvesWalls: true);

    public IEnumerable<AlgorithmStep> Generate(Maze maze, Random random)
    {
        yield return new AlgorithmStep(AlgorithmStepType.ResetWalls, maze.Start);
        List<(MazePosition First, MazePosition Second)> edges = new();
        for (int y = 0; y < maze.Height; y++)
        {
            for (int x = 0; x < maze.Width; x++)
            {
                MazePosition current = new(x, y);
                if (x + 1 < maze.Width) edges.Add((current, new MazePosition(x + 1, y)));
                if (y + 1 < maze.Height) edges.Add((current, new MazePosition(x, y + 1)));
            }
        }

        Shuffle(edges, random);
        DisjointSet sets = new(maze.CellCount);
        foreach ((MazePosition first, MazePosition second) in edges)
        {
            int firstIndex = first.Y * maze.Width + first.X;
            int secondIndex = second.Y * maze.Width + second.X;
            if (!sets.Union(firstIndex, secondIndex))
            {
                continue;
            }

            yield return new AlgorithmStep(AlgorithmStepType.MarkCurrent, first);
            yield return new AlgorithmStep(AlgorithmStepType.CarvePassage, first, second);
            yield return new AlgorithmStep(AlgorithmStepType.VisitCell, first);
            yield return new AlgorithmStep(AlgorithmStepType.VisitCell, second);
        }

        yield return new AlgorithmStep(AlgorithmStepType.Complete, maze.Goal, Message: "Generation complete");
    }

    private static void Shuffle<T>(IList<T> items, Random random)
    {
        for (int index = items.Count - 1; index > 0; index--)
        {
            int swap = random.Next(index + 1);
            (items[index], items[swap]) = (items[swap], items[index]);
        }
    }

    private sealed class DisjointSet
    {
        private readonly int[] _parents;
        private readonly byte[] _ranks;

        public DisjointSet(int count)
        {
            _parents = Enumerable.Range(0, count).ToArray();
            _ranks = new byte[count];
        }

        public int Find(int value)
        {
            if (_parents[value] != value)
            {
                _parents[value] = Find(_parents[value]);
            }

            return _parents[value];
        }

        public bool Union(int first, int second)
        {
            int firstRoot = Find(first);
            int secondRoot = Find(second);
            if (firstRoot == secondRoot) return false;

            if (_ranks[firstRoot] < _ranks[secondRoot])
            {
                _parents[firstRoot] = secondRoot;
            }
            else if (_ranks[firstRoot] > _ranks[secondRoot])
            {
                _parents[secondRoot] = firstRoot;
            }
            else
            {
                _parents[secondRoot] = firstRoot;
                _ranks[firstRoot]++;
            }

            return true;
        }
    }
}
