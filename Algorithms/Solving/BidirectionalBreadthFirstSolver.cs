using MazeRunner.Models;

namespace MazeRunner.Algorithms.Solving;

public sealed class BidirectionalBreadthFirstSolver : IMazeSolver
{
    public AlgorithmMetadata Metadata { get; } = new(
        "Bidirectional BFS",
        "Searches outward from both endpoints until the two frontiers meet.",
        "Guarantees a shortest unweighted route while often exploring fewer cells than BFS.",
        "Requires a known target and has a more complex frontier pattern.",
        GuaranteesShortestPath: true);

    public IEnumerable<AlgorithmStep> Solve(Maze maze)
    {
        Queue<MazePosition> fromStart = new();
        Queue<MazePosition> fromGoal = new();
        Dictionary<MazePosition, MazePosition> startParents = new();
        Dictionary<MazePosition, MazePosition> goalParents = new();
        HashSet<MazePosition> startVisited = new() { maze.Start };
        HashSet<MazePosition> goalVisited = new() { maze.Goal };
        fromStart.Enqueue(maze.Start);
        fromGoal.Enqueue(maze.Goal);
        yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, maze.Start);
        yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, maze.Goal);
        MazePosition? meeting = null;

        while (fromStart.Count > 0 && fromGoal.Count > 0 && meeting is null)
        {
            foreach (AlgorithmStep step in Expand(fromStart, startVisited, goalVisited, startParents, value => meeting = value))
            {
                yield return step;
            }
            if (meeting is not null) break;
            foreach (AlgorithmStep step in Expand(fromGoal, goalVisited, startVisited, goalParents, value => meeting = value))
            {
                yield return step;
            }
        }

        if (meeting is null)
        {
            yield return new AlgorithmStep(AlgorithmStepType.Complete, maze.Goal, Message: "No route found");
            yield break;
        }

        List<MazePosition> firstHalf = SolverPath.Reconstruct(maze.Start, meeting.Value, startParents);
        List<MazePosition> secondHalf = new();
        MazePosition current = meeting.Value;
        while (current != maze.Goal)
        {
            current = goalParents[current];
            secondHalf.Add(current);
        }

        foreach (MazePosition position in firstHalf.Concat(secondHalf))
        {
            yield return new AlgorithmStep(AlgorithmStepType.HighlightSolution, position);
        }
        yield return new AlgorithmStep(AlgorithmStepType.Complete, maze.Goal, Message: "Solved");
        yield break;

        IEnumerable<AlgorithmStep> Expand(
            Queue<MazePosition> queue,
            HashSet<MazePosition> ownVisited,
            HashSet<MazePosition> otherVisited,
            Dictionary<MazePosition, MazePosition> ownParents,
            Action<MazePosition> foundMeeting)
        {
            int count = queue.Count;
            for (int index = 0; index < count; index++)
            {
                MazePosition node = queue.Dequeue();
                yield return new AlgorithmStep(AlgorithmStepType.PopFrontier, node);
                yield return new AlgorithmStep(AlgorithmStepType.MarkCurrent, node);
                yield return new AlgorithmStep(AlgorithmStepType.MarkExplored, node);
                foreach (MazeNeighbor neighbor in maze.GetOpenNeighbors(node))
                {
                    if (!ownVisited.Add(neighbor.Position)) continue;
                    ownParents[neighbor.Position] = node;
                    queue.Enqueue(neighbor.Position);
                    yield return new AlgorithmStep(AlgorithmStepType.PushFrontier, neighbor.Position);
                    if (otherVisited.Contains(neighbor.Position))
                    {
                        foundMeeting(neighbor.Position);
                        yield break;
                    }
                }
            }
        }
    }
}
