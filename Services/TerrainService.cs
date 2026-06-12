using MazeRunner.Models;

namespace MazeRunner.Services;

public static class TerrainService
{
    public static void Apply(Maze maze, Random random, bool enabled)
    {
        foreach (MazeCell cell in maze.Cells)
        {
            cell.TerrainCost = 1;
        }

        if (!enabled) return;
        foreach (MazeCell cell in maze.Cells)
        {
            if (cell.Position == maze.Start || cell.Position == maze.Goal) continue;
            int roll = random.Next(100);
            cell.TerrainCost = roll switch
            {
                < 7 => 5,
                < 18 => 3,
                < 34 => 2,
                _ => 1
            };
        }
    }
}
