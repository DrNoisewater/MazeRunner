using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

public interface IMazeSolver
{
    AlgorithmMetadata Metadata { get; }
    IEnumerable<AlgorithmStep> Solve(Maze maze);
}
