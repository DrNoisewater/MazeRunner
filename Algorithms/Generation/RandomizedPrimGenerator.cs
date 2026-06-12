using MazeRunner.Models;

namespace MazeRunner.Algorithms.Generation;

public sealed class RandomizedPrimGenerator : IMazeGenerator
{
    public AlgorithmMetadata Metadata { get; } = new(
        "Randomized Prim",
        "Expands a randomized frontier outward from a growing maze region.",
        "Produces branching mazes and a satisfying outward-growth animation.",
        "Uses a larger active frontier than Recursive Backtracker.",
        CarvesWalls: true);

    public IEnumerable<AlgorithmStep> Generate(Maze maze, Random random)
    {
        yield return new AlgorithmStep(AlgorithmStepType.ResetWalls, maze.Start);
        MazePosition start = new(random.Next(maze.Width), random.Next(maze.Height));
        HashSet<MazePosition> visited = new() { start };
        List<(MazePosition From, MazePosition To)> frontier = new();
        yield return new AlgorithmStep(AlgorithmStepType.VisitCell, start);
        AddFrontier(start);

        while (frontier.Count > 0)
        {
            int index = random.Next(frontier.Count);
            (MazePosition from, MazePosition to) = frontier[index];
            frontier.RemoveAt(index);
            yield return new AlgorithmStep(AlgorithmStepType.PopFrontier, to);

            if (!visited.Add(to))
            {
                continue;
            }

            yield return new AlgorithmStep(AlgorithmStepType.MarkCurrent, to);
            yield return new AlgorithmStep(AlgorithmStepType.CarvePassage, from, to);
            yield return new AlgorithmStep(AlgorithmStepType.VisitCell, to);
            AddFrontier(to);
        }

        yield return new AlgorithmStep(AlgorithmStepType.Complete, maze.Goal, Message: "Generation complete");
        yield break;

        void AddFrontier(MazePosition position)
        {
            foreach (MazeNeighbor neighbor in maze.GetNeighbors(position))
            {
                if (!visited.Contains(neighbor.Position))
                {
                    frontier.Add((position, neighbor.Position));
                }
            }
        }
    }
}
