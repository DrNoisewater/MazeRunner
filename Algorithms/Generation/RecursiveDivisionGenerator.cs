using MazeRunner.Models;

namespace MazeRunner.Algorithms.Generation;

public sealed class RecursiveDivisionGenerator : IMazeGenerator
{
    public AlgorithmMetadata Metadata { get; } = new(
        "Recursive Division",
        "Starts with open space, then adds dividing walls with a single opening per division.",
        "Creates structured chambers and a visually different wall-building animation.",
        "Its rectilinear structure is less organic than carved mazes.",
        AddsWalls: true);

    public IEnumerable<AlgorithmStep> Generate(Maze maze, Random random)
    {
        yield return new AlgorithmStep(AlgorithmStepType.ClearInteriorWalls, maze.Start);
        foreach (AlgorithmStep step in Divide(maze, random, 0, 0, maze.Width, maze.Height))
        {
            yield return step;
        }

        foreach (MazeCell cell in maze.Cells)
        {
            yield return new AlgorithmStep(AlgorithmStepType.VisitCell, cell.Position);
        }

        yield return new AlgorithmStep(AlgorithmStepType.Complete, maze.Goal, Message: "Generation complete");
    }

    private static IEnumerable<AlgorithmStep> Divide(Maze maze, Random random, int x, int y, int width, int height)
    {
        if (width <= 1 || height <= 1)
        {
            yield break;
        }

        bool horizontal = height > width || (height == width && random.Next(2) == 0);
        if (horizontal)
        {
            int divideY = y + random.Next(height - 1);
            int passageX = x + random.Next(width);
            for (int wallX = x; wallX < x + width; wallX++)
            {
                if (wallX == passageX) continue;
                MazePosition first = new(wallX, divideY);
                MazePosition second = new(wallX, divideY + 1);
                yield return new AlgorithmStep(AlgorithmStepType.AddWall, first, second);
            }

            int topHeight = divideY - y + 1;
            foreach (AlgorithmStep step in Divide(maze, random, x, y, width, topHeight)) yield return step;
            foreach (AlgorithmStep step in Divide(maze, random, x, divideY + 1, width, height - topHeight)) yield return step;
        }
        else
        {
            int divideX = x + random.Next(width - 1);
            int passageY = y + random.Next(height);
            for (int wallY = y; wallY < y + height; wallY++)
            {
                if (wallY == passageY) continue;
                MazePosition first = new(divideX, wallY);
                MazePosition second = new(divideX + 1, wallY);
                yield return new AlgorithmStep(AlgorithmStepType.AddWall, first, second);
            }

            int leftWidth = divideX - x + 1;
            foreach (AlgorithmStep step in Divide(maze, random, x, y, leftWidth, height)) yield return step;
            foreach (AlgorithmStep step in Divide(maze, random, divideX + 1, y, width - leftWidth, height)) yield return step;
        }
    }
}
