namespace MazeRunner.Models;

public enum AlgorithmStepType
{
    ResetWalls,
    ClearInteriorWalls,
    VisitCell,
    CarvePassage,
    AddWall,
    PushFrontier,
    PopFrontier,
    MarkExplored,
    MarkCurrent,
    Backtrack,
    HighlightSolution,
    Complete
}

public sealed record AlgorithmStep(
    AlgorithmStepType Type,
    MazePosition Position,
    MazePosition? OtherPosition = null,
    string? Message = null);
