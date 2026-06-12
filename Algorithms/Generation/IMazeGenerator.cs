using MazeRunner.Models;

namespace MazeRunner.Algorithms.Generation;

public interface IMazeGenerator
{
    AlgorithmMetadata Metadata { get; }
    IEnumerable<AlgorithmStep> Generate(Maze maze, Random random);
}
