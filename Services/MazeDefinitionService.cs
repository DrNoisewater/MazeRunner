using System.IO;
using System.Text.Json;
using MazeRunner.Models;

namespace MazeRunner.Services;

public static class MazeDefinitionService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static void Export(string path, Maze maze, int loopsPercent)
    {
        MazeDefinition definition = new()
        {
            Width = maze.Width,
            Height = maze.Height,
            Seed = maze.Seed,
            Generator = maze.GeneratorName,
            LoopsPercent = loopsPercent,
            Start = maze.Start,
            Goal = maze.Goal,
            Cells = maze.Cells.Select(cell => new CellDefinition
            {
                X = cell.Position.X,
                Y = cell.Position.Y,
                Walls = cell.Walls,
                TerrainCost = cell.TerrainCost
            }).ToList()
        };
        File.WriteAllText(path, JsonSerializer.Serialize(definition, Options));
    }

    public static Maze Import(string path)
    {
        MazeDefinition? definition = JsonSerializer.Deserialize<MazeDefinition>(File.ReadAllText(path), Options);
        if (definition is null || definition.Version != 1)
        {
            throw new InvalidDataException("The maze file is missing or uses an unsupported version.");
        }

        Maze maze = new(definition.Width, definition.Height, definition.Seed)
        {
            GeneratorName = string.IsNullOrWhiteSpace(definition.Generator) ? "Imported Maze" : definition.Generator
        };
        maze.SetEndpoints(definition.Start, definition.Goal);
        if (definition.Cells.Count != maze.CellCount)
        {
            throw new InvalidDataException("The maze file does not contain the expected number of cells.");
        }

        foreach (CellDefinition cell in definition.Cells)
        {
            MazePosition position = new(cell.X, cell.Y);
            if (!maze.Contains(position)) throw new InvalidDataException("The maze file contains an out-of-range cell.");
            maze[position].Walls = cell.Walls;
            maze[position].TerrainCost = Math.Clamp(cell.TerrainCost, 1, 9);
        }

        if (!maze.ValidateWallSymmetry(out string? error))
        {
            throw new InvalidDataException(error ?? "The imported wall layout is invalid.");
        }

        if (!maze.IsFullyConnected())
        {
            throw new InvalidDataException("The imported maze is not fully connected.");
        }

        return maze;
    }
}
