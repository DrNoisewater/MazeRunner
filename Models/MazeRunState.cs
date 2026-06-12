namespace MazeRunner.Models;

public enum MazeRunState
{
    Idle,
    Generating,
    GenerationPaused,
    ReadyToSolve,
    Solving,
    SolvingPaused,
    Solved
}
