namespace MazeRunner.Models;

public sealed record AlgorithmMetadata(
    string Name,
    string Description,
    string Strengths,
    string Weaknesses,
    bool GuaranteesShortestPath = false,
    bool SupportsWeightedTerrain = false,
    bool CarvesWalls = false,
    bool AddsWalls = false);
