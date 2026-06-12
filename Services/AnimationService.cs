using MazeRunner.Models;

namespace MazeRunner.Services;

public sealed class AnimationService
{
    private bool _stepRequested;

    public bool IsPaused { get; private set; }
    public bool InstantComplete { get; set; }
    public bool ReduceMotion { get; set; }
    public int Speed { get; set; } = 62;

    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;

    public void Step()
    {
        if (IsPaused)
        {
            _stepRequested = true;
        }
    }

    public async Task RunAsync(
        IEnumerable<AlgorithmStep> steps,
        Action<AlgorithmStep> applyStep,
        CancellationToken cancellationToken)
    {
        foreach (AlgorithmStep step in steps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            while (IsPaused && !_stepRequested)
            {
                await Task.Delay(22, cancellationToken);
            }

            _stepRequested = false;
            applyStep(step);

            if (!InstantComplete)
            {
                int delay = ReduceMotion ? 1 : Math.Max(1, 132 - Math.Clamp(Speed, 1, 100));
                await Task.Delay(delay, cancellationToken);
            }
            else if (step.Type == AlgorithmStepType.Complete)
            {
                await Task.Yield();
            }
        }
    }
}
