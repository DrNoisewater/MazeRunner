namespace MazeRunner.Models;

public sealed class Maze
{
    private static readonly MazeWall[] CardinalWalls =
    {
        MazeWall.North,
        MazeWall.East,
        MazeWall.South,
        MazeWall.West
    };

    private readonly MazeCell[,] _cells;

    public Maze(int width, int height, int seed)
    {
        if (width < 2 || width > 151)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Maze width must be between 2 and 151 cells.");
        }

        if (height < 2 || height > 101)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Maze height must be between 2 and 101 cells.");
        }

        Width = width;
        Height = height;
        Seed = seed;
        Start = new MazePosition(0, 0);
        Goal = new MazePosition(width - 1, height - 1);
        _cells = new MazeCell[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _cells[x, y] = new MazeCell(new MazePosition(x, y));
            }
        }
    }

    public int Width { get; }
    public int Height { get; }
    public int Seed { get; }
    public string GeneratorName { get; set; } = string.Empty;
    public int CellCount => Width * Height;
    public MazePosition Start { get; private set; }
    public MazePosition Goal { get; private set; }

    public IEnumerable<MazeCell> Cells
    {
        get
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return _cells[x, y];
                }
            }
        }
    }

    public MazeCell this[int x, int y] => GetCell(new MazePosition(x, y));
    public MazeCell this[MazePosition position] => GetCell(position);

    public bool Contains(MazePosition position) =>
        position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;

    public MazeCell GetCell(MazePosition position)
    {
        EnsureContained(position);
        return _cells[position.X, position.Y];
    }

    public IEnumerable<MazeNeighbor> GetNeighbors(MazePosition position)
    {
        EnsureContained(position);
        foreach (MazeWall direction in CardinalWalls)
        {
            (int deltaX, int deltaY) = direction.ToOffset();
            MazePosition candidate = position.Offset(deltaX, deltaY);
            if (Contains(candidate))
            {
                yield return new MazeNeighbor(candidate, direction);
            }
        }
    }

    public IEnumerable<MazeNeighbor> GetOpenNeighbors(MazePosition position)
    {
        MazeCell cell = GetCell(position);
        foreach (MazeNeighbor neighbor in GetNeighbors(position))
        {
            if (!cell.HasWall(neighbor.Direction))
            {
                yield return neighbor;
            }
        }
    }

    public void SetEndpoints(MazePosition start, MazePosition goal)
    {
        EnsureContained(start);
        EnsureContained(goal);
        if (start == goal)
        {
            throw new ArgumentException("The start and goal positions must differ.");
        }

        Start = start;
        Goal = goal;
    }

    public void ResetAllWalls()
    {
        foreach (MazeCell cell in Cells)
        {
            cell.Walls = MazeWall.All;
        }
    }

    public void ClearInteriorWalls()
    {
        foreach (MazeCell cell in Cells)
        {
            cell.Walls = MazeWall.None;
            if (cell.Position.Y == 0) cell.Walls |= MazeWall.North;
            if (cell.Position.X == Width - 1) cell.Walls |= MazeWall.East;
            if (cell.Position.Y == Height - 1) cell.Walls |= MazeWall.South;
            if (cell.Position.X == 0) cell.Walls |= MazeWall.West;
        }
    }

    public void RemoveWallBetween(MazePosition first, MazePosition second)
    {
        MazeWall direction = GetDirection(first, second);
        GetCell(first).Walls &= ~direction;
        GetCell(second).Walls &= ~direction.Opposite();
    }

    public void AddWallBetween(MazePosition first, MazePosition second)
    {
        MazeWall direction = GetDirection(first, second);
        GetCell(first).Walls |= direction;
        GetCell(second).Walls |= direction.Opposite();
    }

    public bool ValidateWallSymmetry(out string? errorMessage)
    {
        foreach (MazeCell cell in Cells)
        {
            foreach (MazeNeighbor neighbor in GetNeighbors(cell.Position))
            {
                bool first = cell.HasWall(neighbor.Direction);
                bool second = GetCell(neighbor.Position).HasWall(neighbor.Direction.Opposite());
                if (first != second)
                {
                    errorMessage = $"Wall mismatch between {cell.Position} and {neighbor.Position}.";
                    return false;
                }
            }
        }

        errorMessage = null;
        return true;
    }

    public bool IsFullyConnected()
    {
        HashSet<MazePosition> visited = new() { Start };
        Queue<MazePosition> queue = new();
        queue.Enqueue(Start);

        while (queue.Count > 0)
        {
            MazePosition current = queue.Dequeue();
            foreach (MazeNeighbor neighbor in GetOpenNeighbors(current))
            {
                if (visited.Add(neighbor.Position))
                {
                    queue.Enqueue(neighbor.Position);
                }
            }
        }

        return visited.Count == CellCount;
    }

    public Maze Clone()
    {
        Maze clone = new(Width, Height, Seed)
        {
            GeneratorName = GeneratorName
        };
        clone.SetEndpoints(Start, Goal);
        foreach (MazeCell cell in Cells)
        {
            MazeCell target = clone[cell.Position];
            target.Walls = cell.Walls;
            target.TerrainCost = cell.TerrainCost;
        }

        return clone;
    }

    private MazeWall GetDirection(MazePosition first, MazePosition second)
    {
        EnsureContained(first);
        EnsureContained(second);
        return (second.X - first.X, second.Y - first.Y) switch
        {
            (0, -1) => MazeWall.North,
            (1, 0) => MazeWall.East,
            (0, 1) => MazeWall.South,
            (-1, 0) => MazeWall.West,
            _ => throw new ArgumentException($"Cells {first} and {second} are not cardinally adjacent.")
        };
    }

    private void EnsureContained(MazePosition position)
    {
        if (!Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "The position is outside the maze.");
        }
    }
}
