using MazeRunner.Models;

namespace MazeRunner.Algorithms.Generation;

public static class MazeBraider
{
    public static IEnumerable<AlgorithmStep> CreateBraidingSteps(Maze maze, Random random, int percent)
    {
        if (percent <= 0) yield break;

        List<MazePosition> deadEnds = maze.Cells
            .Where(cell => maze.GetOpenNeighbors(cell.Position).Count() == 1)
            .Select(cell => cell.Position)
            .OrderBy(_ => random.Next())
            .ToList();

        foreach (MazePosition deadEnd in deadEnds)
        {
            if (random.Next(100) >= percent) continue;
            List<MazeNeighbor> closedNeighbors = maze.GetNeighbors(deadEnd)
                .Where(neighbor => maze[deadEnd].HasWall(neighbor.Direction))
                .ToList();
            if (closedNeighbors.Count == 0) continue;
            MazeNeighbor chosen = closedNeighbors[random.Next(closedNeighbors.Count)];
            yield return new AlgorithmStep(AlgorithmStepType.CarvePassage, deadEnd, chosen.Position, "Braiding passage");
        }
    }
}
