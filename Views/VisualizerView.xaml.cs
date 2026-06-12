using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MazeRunner.Algorithms.Generation;
using MazeRunner.Algorithms.Solving;
using MazeRunner.Controls;
using MazeRunner.Models;
using MazeRunner.Services;
using Microsoft.Win32;

namespace MazeRunner.Views;

public partial class VisualizerView : UserControl
{
    private readonly AnimationService _animation = new();
    private readonly MazeVisualization _visualization = new();
    private readonly Stopwatch _stopwatch = new();
    private CancellationTokenSource? _cancellation;
    private Maze? _maze;
    private MazeRunState _state = MazeRunState.Idle;
    private int _toastVersion;
    private int _generationVisitedCount;
    private bool _loaded;

    public VisualizerView()
    {
        InitializeComponent();
        Loaded += VisualizerView_Loaded;
        Unloaded += VisualizerView_Unloaded;
    }

    public bool HandleShortcut(KeyEventArgs e)
    {
        ModifierKeys modifiers = Keyboard.Modifiers;
        if (modifiers == ModifierKeys.Control && e.Key == Key.G)
        {
            _ = GenerateMazeAsync(false);
            return true;
        }
        if (modifiers == ModifierKeys.Control && e.Key == Key.Enter)
        {
            _ = SolveMazeAsync();
            return true;
        }
        if (modifiers == ModifierKeys.Control && e.Key == Key.R)
        {
            ResetSearch();
            return true;
        }
        if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.R)
        {
            RandomizeSeed();
            _ = GenerateMazeAsync(true);
            return true;
        }
        if (modifiers == ModifierKeys.Control && e.Key == Key.E)
        {
            ExportPng();
            return true;
        }
        if (modifiers == ModifierKeys.None && e.Key == Key.Space)
        {
            TogglePause();
            return true;
        }
        if (modifiers == ModifierKeys.None && e.Key == Key.Right)
        {
            StepOnce();
            return true;
        }
        if (modifiers == ModifierKeys.None && e.Key == Key.F)
        {
            MazeCanvas.FitToView();
            return true;
        }
        if (modifiers == ModifierKeys.None && (e.Key == Key.Add || e.Key == Key.OemPlus))
        {
            MazeCanvas.ZoomIn();
            return true;
        }
        if (modifiers == ModifierKeys.None && (e.Key == Key.Subtract || e.Key == Key.OemMinus))
        {
            MazeCanvas.ZoomOut();
            return true;
        }
        return false;
    }

    private async void VisualizerView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loaded) return;
        _loaded = true;
        GeneratorComboBox.ItemsSource = GeneratorRegistry.All.Select(item => item.Metadata.Name).ToList();
        SolverComboBox.ItemsSource = SolverRegistry.All.Select(item => item.Metadata.Name).ToList();
        GeneratorComboBox.SelectedIndex = 0;
        SolverComboBox.SelectedIndex = 0;
        WidthTextBox.Text = AppState.Settings.DefaultWidth.ToString();
        HeightTextBox.Text = AppState.Settings.DefaultHeight.ToString();
        SeedTextBox.Text = Random.Shared.Next(100000, 999999).ToString();
        SpeedSlider.Value = AppState.Settings.AnimationSpeed;
        MazeCanvas.ShowGrid = AppState.Settings.ShowGrid;
        MazeCanvas.ShowDirectionalMarkers = AppState.Settings.ShowDirectionalMarkers;
        MazeCanvas.Visualization = _visualization;
        _animation.ReduceMotion = AppState.Settings.ReduceMotion;
        UpdateGridLabel();

        if (AppState.CurrentMaze is not null)
        {
            LoadMaze(AppState.CurrentMaze.Clone(), "Restored current maze");
        }
        else
        {
            await GenerateMazeAsync(false, forceInstant: true);
        }
    }

    private void VisualizerView_Unloaded(object sender, RoutedEventArgs e)
    {
        CancelActiveRun();
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e) => await GenerateMazeAsync(false);
    private async void SolveButton_Click(object sender, RoutedEventArgs e) => await SolveMazeAsync();
    private void PauseButton_Click(object sender, RoutedEventArgs e) => TogglePause();
    private void StepButton_Click(object sender, RoutedEventArgs e) => StepOnce();
    private void ResetSearchButton_Click(object sender, RoutedEventArgs e) => ResetSearch();
    private void ResetMazeButton_Click(object sender, RoutedEventArgs e) => ClearMaze();
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => MazeCanvas.ZoomOut();
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => MazeCanvas.ZoomIn();
    private void FitButton_Click(object sender, RoutedEventArgs e) => MazeCanvas.FitToView();

    private void MazeCanvas_ZoomLevelChanged(object? sender, EventArgs e)
    {
        ZoomLabel.Text = $"{MazeCanvas.ZoomLevel:P0}";
    }

    private void GridToggleButton_Click(object sender, RoutedEventArgs e)
    {
        MazeCanvas.ShowGrid = !MazeCanvas.ShowGrid;
        AppState.Settings.ShowGrid = MazeCanvas.ShowGrid;
        AppState.NotifySettingsChanged();
        UpdateGridLabel();
    }

    private void UpdateGridLabel()
    {
        GridToggleButton.Content = MazeCanvas.ShowGrid ? "Grid on" : "Grid off";
    }

    private void SmallPresetButton_Click(object sender, RoutedEventArgs e) => SetPreset(15, 11);
    private void MediumPresetButton_Click(object sender, RoutedEventArgs e) => SetPreset(31, 21);
    private void LargePresetButton_Click(object sender, RoutedEventArgs e) => SetPreset(61, 41);
    private void HugePresetButton_Click(object sender, RoutedEventArgs e) => SetPreset(101, 71);

    private void SetPreset(int width, int height)
    {
        WidthTextBox.Text = width.ToString();
        HeightTextBox.Text = height.ToString();
    }

    private void RandomSeedButton_Click(object sender, RoutedEventArgs e) => RandomizeSeed();

    private void RandomizeSeed()
    {
        SeedTextBox.Text = Random.Shared.Next(1, int.MaxValue).ToString();
        ShowToast("New random seed ready");
    }

    private void CopySeedButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(SeedTextBox.Text);
            ShowToast("Seed copied to clipboard");
        }
        catch (Exception exception)
        {
            ShowToast($"Clipboard unavailable: {exception.Message}");
        }
    }

    private void LoopsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (LoopsValueLabel is not null) LoopsValueLabel.Text = $"{(int)e.NewValue}%";
    }

    private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        int speed = (int)e.NewValue;
        _animation.Speed = speed;
        if (SpeedValueLabel is not null) SpeedValueLabel.Text = $"{speed}%";
        if (_loaded)
        {
            AppState.Settings.AnimationSpeed = speed;
            AppState.NotifySettingsChanged();
        }
    }

    private void InstantCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        _animation.InstantComplete = InstantCheckBox.IsChecked == true;
    }

    private void WeightedTerrainCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loaded || _maze is null || IsActive) return;
        TerrainService.Apply(_maze, new Random(_maze.Seed ^ 0x2A94F), WeightedTerrainCheckBox.IsChecked == true);
        ResetSearch();
        MazeCanvas.Refresh();
        ShowToast(WeightedTerrainCheckBox.IsChecked == true ? "Weighted terrain applied" : "Weighted terrain cleared");
    }

    private void GeneratorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GeneratorComboBox.SelectedItem is string name) ShowMetadata(GeneratorRegistry.Find(name).Metadata);
    }

    private void SolverComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SolverComboBox.SelectedItem is string name)
        {
            SolverValueLabel.Text = name;
            ShowMetadata(SolverRegistry.Find(name).Metadata);
        }
    }

    private void ShowMetadata(AlgorithmMetadata metadata)
    {
        if (AlgorithmNameLabel is null) return;
        AlgorithmNameLabel.Text = metadata.Name;
        AlgorithmDescriptionLabel.Text = metadata.Description;
        AlgorithmNotesLabel.Text = $"Strengths: {metadata.Strengths}\n\nTradeoffs: {metadata.Weaknesses}";
    }

    private async Task GenerateMazeAsync(bool randomizeSeed, bool forceInstant = false)
    {
        if (IsActive) return;
        if (!TryReadParameters(out int width, out int height, out int seed)) return;
        if (randomizeSeed)
        {
            seed = Random.Shared.Next(1, int.MaxValue);
            SeedTextBox.Text = seed.ToString();
        }

        CancelActiveRun();
        _cancellation = new CancellationTokenSource();
        _visualization.ClearAll();
        _generationVisitedCount = 0;
        _maze = new Maze(width, height, seed);
        AppState.CurrentMaze = _maze;
        MazeCanvas.Maze = _maze;
        MazeCanvas.Visualization = _visualization;
        IMazeGenerator generator = SelectedGenerator;
        _maze.GeneratorName = generator.Metadata.Name;
        int loops = (int)LoopsSlider.Value;
        _animation.InstantComplete = forceInstant || InstantCheckBox.IsChecked == true;
        _animation.Resume();
        SetState(MazeRunState.Generating);
        _stopwatch.Restart();

        try
        {
            Random generatorRandom = new(seed);
            IEnumerable<AlgorithmStep> steps = generator.Generate(_maze, generatorRandom)
                .Concat(MazeBraider.CreateBraidingSteps(_maze, new Random(seed ^ 0x5A17), loops));
            await _animation.RunAsync(steps, ApplyGenerationStep, _cancellation.Token);
            TerrainService.Apply(_maze, new Random(seed ^ 0x2A94F), WeightedTerrainCheckBox.IsChecked == true);
            _stopwatch.Stop();

            // Generation activity is useful while carving, but it should not tint the
            // finished maze or resemble an active pathfinding search.
            _visualization.ClearGeneration();
            MazeCanvas.Refresh();
            SetState(MazeRunState.ReadyToSolve);
            string result = _maze.ValidateWallSymmetry(out _) && _maze.IsFullyConnected()
                ? "Maze ready · press Solve maze to start pathfinding"
                : "Maze generation validation failed";
            ShowToast(result);
        }
        catch (OperationCanceledException)
        {
            ShowToast("Generation cancelled");
        }
        catch (Exception exception)
        {
            SetState(MazeRunState.Idle);
            MessageBox.Show(exception.Message, "Maze generation failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _animation.InstantComplete = InstantCheckBox.IsChecked == true;
            UpdateStatistics();
            UpdateControls();
        }
    }

    private async Task SolveMazeAsync()
    {
        if (_maze is null || IsActive) return;
        CancelActiveRun();
        _cancellation = new CancellationTokenSource();
        _visualization.ClearGeneration();
        _visualization.ClearSearch();
        MazeCanvas.Refresh();
        IMazeSolver solver = SelectedSolver;
        _animation.Resume();
        _animation.InstantComplete = InstantCheckBox.IsChecked == true;
        SetState(MazeRunState.Solving);
        _stopwatch.Restart();

        try
        {
            await _animation.RunAsync(solver.Solve(_maze), ApplySolverStep, _cancellation.Token);
            _stopwatch.Stop();

            // Keep the selected solver's exploration metrics, but present one clean
            // canonical route after completion. Unweighted mazes show the shortest
            // route by steps; weighted mazes show the lowest-cost valid route.
            List<MazePosition> optimalPath = OptimalPathFinder.Find(_maze);
            _visualization.SolutionPath.Clear();
            _visualization.SolutionPath.AddRange(optimalPath);
            _visualization.HideSearchActivity();
            MazeCanvas.Refresh();
            SetState(MazeRunState.Solved);
            AddHistoryEntry(solver.Metadata.Name);
            ShowToast(_visualization.SolutionPath.Count > 0 ? "Maze solved · shortest route highlighted" : "No route found");
        }
        catch (OperationCanceledException)
        {
            ShowToast("Solve cancelled");
        }
        catch (Exception exception)
        {
            SetState(MazeRunState.ReadyToSolve);
            MessageBox.Show(exception.Message, "Solve failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            UpdateStatistics();
            UpdateControls();
        }
    }

    private void ApplyGenerationStep(AlgorithmStep step)
    {
        if (_maze is null) return;
        switch (step.Type)
        {
            case AlgorithmStepType.ResetWalls:
                _maze.ResetAllWalls();
                _visualization.ClearAll();
                break;
            case AlgorithmStepType.ClearInteriorWalls:
                _maze.ClearInteriorWalls();
                _visualization.ClearAll();
                break;
            case AlgorithmStepType.CarvePassage when step.OtherPosition is MazePosition other:
                _maze.RemoveWallBetween(step.Position, other);
                break;
            case AlgorithmStepType.AddWall when step.OtherPosition is MazePosition other:
                _maze.AddWallBetween(step.Position, other);
                break;
            case AlgorithmStepType.VisitCell:
                _visualization.GeneratorVisited.Add(step.Position);
                _generationVisitedCount = Math.Max(_generationVisitedCount, _visualization.GeneratorVisited.Count);
                break;
            case AlgorithmStepType.MarkCurrent:
                _visualization.CurrentCell = step.Position;
                break;
            case AlgorithmStepType.Backtrack:
                _visualization.Backtracked.Add(step.Position);
                break;
            case AlgorithmStepType.PushFrontier:
                _visualization.Frontier.Add(step.Position);
                break;
            case AlgorithmStepType.PopFrontier:
                _visualization.Frontier.Remove(step.Position);
                break;
            case AlgorithmStepType.Complete:
                _visualization.CurrentCell = null;
                break;
        }
        MazeCanvas.Refresh();
        UpdateStatistics();
    }

    private void ApplySolverStep(AlgorithmStep step)
    {
        switch (step.Type)
        {
            case AlgorithmStepType.PushFrontier:
                _visualization.Frontier.Add(step.Position);
                break;
            case AlgorithmStepType.PopFrontier:
                _visualization.Frontier.Remove(step.Position);
                break;
            case AlgorithmStepType.MarkCurrent:
                _visualization.CurrentCell = step.Position;
                break;
            case AlgorithmStepType.MarkExplored:
                _visualization.Explored.Add(step.Position);
                break;
            case AlgorithmStepType.Backtrack:
                _visualization.Backtracked.Add(step.Position);
                break;
            case AlgorithmStepType.HighlightSolution:
                _visualization.SolutionPath.Add(step.Position);
                break;
            case AlgorithmStepType.Complete:
                _visualization.CurrentCell = null;
                break;
        }
        MazeCanvas.Refresh();
        UpdateStatistics();
    }

    private void TogglePause()
    {
        if (!IsActive) return;
        if (_animation.IsPaused)
        {
            _animation.Resume();
            SetState(_state == MazeRunState.GenerationPaused ? MazeRunState.Generating : MazeRunState.Solving);
        }
        else
        {
            _animation.Pause();
            SetState(_state == MazeRunState.Generating ? MazeRunState.GenerationPaused : MazeRunState.SolvingPaused);
        }
    }

    private void StepOnce()
    {
        if (_state is MazeRunState.GenerationPaused or MazeRunState.SolvingPaused) _animation.Step();
    }

    private void ResetSearch()
    {
        if (_maze is null || IsActive) return;
        _visualization.ClearSearch();
        MazeCanvas.Refresh();
        SetState(MazeRunState.ReadyToSolve);
        ShowToast("Search visualization reset");
    }

    private void ClearMaze()
    {
        CancelActiveRun();
        _maze = null;
        AppState.CurrentMaze = null;
        _visualization.ClearAll();
        _generationVisitedCount = 0;
        MazeCanvas.Maze = null;
        SetState(MazeRunState.Idle);
        ShowToast("Maze cleared");
    }

    private void SelectStartButton_Click(object sender, RoutedEventArgs e) => SetSelectionMode(EndpointSelectionMode.Start);
    private void SelectGoalButton_Click(object sender, RoutedEventArgs e) => SetSelectionMode(EndpointSelectionMode.Goal);

    private void SetSelectionMode(EndpointSelectionMode mode)
    {
        if (_maze is null || IsActive) return;
        MazeCanvas.SelectionMode = mode;
        CanvasStatusLabel.Text = mode == EndpointSelectionMode.Start ? "Click a new start cell" : "Click a new goal cell";
        ShowToast(CanvasStatusLabel.Text);
    }

    private void MazeCanvas_CellClicked(object? sender, MazeCellClickedEventArgs e)
    {
        if (_maze is null || MazeCanvas.SelectionMode == EndpointSelectionMode.None) return;
        try
        {
            MazePosition start = MazeCanvas.SelectionMode == EndpointSelectionMode.Start ? e.Position : _maze.Start;
            MazePosition goal = MazeCanvas.SelectionMode == EndpointSelectionMode.Goal ? e.Position : _maze.Goal;
            _maze.SetEndpoints(start, goal);
            MazeCanvas.SelectionMode = EndpointSelectionMode.None;
            ResetSearch();
            CanvasStatusLabel.Text = "Endpoints updated";
            ShowToast("Endpoint updated");
        }
        catch (ArgumentException exception)
        {
            ShowToast(exception.Message);
        }
    }

    private void RandomizeEndpointsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_maze is null || IsActive) return;
        Random random = new(_maze.Seed ^ Environment.TickCount);
        MazePosition start = new(random.Next(_maze.Width), random.Next(_maze.Height));
        MazePosition goal;
        do goal = new MazePosition(random.Next(_maze.Width), random.Next(_maze.Height)); while (goal == start);
        _maze.SetEndpoints(start, goal);
        ResetSearch();
        MazeCanvas.Refresh();
        ShowToast("Endpoints randomized");
    }

    private void ImportJsonButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsActive) return;
        OpenFileDialog dialog = new() { Filter = "MazeRunner maze (*.json)|*.json|JSON files (*.json)|*.json" };
        if (dialog.ShowDialog() != true) return;
        try
        {
            LoadMaze(MazeDefinitionService.Import(dialog.FileName), "Maze imported");
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportJsonButton_Click(object sender, RoutedEventArgs e)
    {
        if (_maze is null) return;
        SaveFileDialog dialog = new() { Filter = "MazeRunner maze (*.json)|*.json", FileName = $"maze-{_maze.Seed}.json" };
        if (dialog.ShowDialog() != true) return;
        try
        {
            MazeDefinitionService.Export(dialog.FileName, _maze, (int)LoopsSlider.Value);
            ShowToast("Maze JSON exported");
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Export failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportPngButton_Click(object sender, RoutedEventArgs e) => ExportPng();

    private void ExportPng()
    {
        if (_maze is null) return;
        SaveFileDialog dialog = new() { Filter = "PNG image (*.png)|*.png", FileName = $"maze-{_maze.Seed}.png" };
        if (dialog.ShowDialog() != true) return;
        try
        {
            MazeImageExportService.ExportPng(dialog.FileName, MazeCanvas);
            ShowToast("Maze image exported");
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Image export failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadMaze(Maze maze, string toast)
    {
        CancelActiveRun();
        _maze = maze;
        AppState.CurrentMaze = maze;
        _visualization.ClearAll();
        _generationVisitedCount = maze.CellCount;
        MazeCanvas.Maze = maze;
        MazeCanvas.Visualization = _visualization;
        WidthTextBox.Text = maze.Width.ToString();
        HeightTextBox.Text = maze.Height.ToString();
        SeedTextBox.Text = maze.Seed.ToString();
        SetState(MazeRunState.ReadyToSolve);
        ShowToast(toast);
    }

    private bool TryReadParameters(out int width, out int height, out int seed)
    {
        width = height = seed = 0;
        if (!int.TryParse(WidthTextBox.Text, out width) || width is < 2 or > 151)
        {
            ShowToast("Width must be between 2 and 151");
            return false;
        }
        if (!int.TryParse(HeightTextBox.Text, out height) || height is < 2 or > 101)
        {
            ShowToast("Height must be between 2 and 101");
            return false;
        }
        if (!int.TryParse(SeedTextBox.Text, out seed))
        {
            ShowToast("Seed must be a valid 32-bit integer");
            return false;
        }
        return true;
    }

    private void AddHistoryEntry(string solverName)
    {
        if (_maze is null) return;
        int pathCost = _visualization.SolutionPath.Skip(1).Sum(position => _maze[position].TerrainCost);
        AppState.AddHistory(new RunHistoryEntry
        {
            Timestamp = DateTime.Now,
            Width = _maze.Width,
            Height = _maze.Height,
            Seed = _maze.Seed,
            Generator = _maze.GeneratorName,
            Solver = solverName,
            ExploredCells = _visualization.Explored.Count,
            SolutionLength = _visualization.SolutionPath.Count,
            WeightedPathCost = pathCost,
            ElapsedMilliseconds = _stopwatch.Elapsed.TotalMilliseconds,
            EfficiencyPercent = _visualization.Explored.Count == 0 ? 0 : Math.Min(100.0, _visualization.SolutionPath.Count / (double)_visualization.Explored.Count * 100.0)
        });
    }

    private void SetState(MazeRunState state)
    {
        _state = state;
        UpdateStatistics();
        UpdateControls();
    }

    private void UpdateStatistics()
    {
        StatusValueLabel.Text = _state.ToString();
        DimensionsValueLabel.Text = _maze is null ? "—" : $"{_maze.Width} × {_maze.Height}";
        CellsValueLabel.Text = _maze?.CellCount.ToString() ?? "—";
        GeneratorValueLabel.Text = _maze?.GeneratorName ?? "—";
        SolverValueLabel.Text = SolverComboBox.SelectedItem as string ?? "—";
        SeedValueLabel.Text = _maze?.Seed.ToString() ?? "—";
        VisitedValueLabel.Text = _generationVisitedCount.ToString();
        ExploredValueLabel.Text = _visualization.Explored.Count.ToString();
        FrontierValueLabel.Text = _visualization.Frontier.Count.ToString();
        PathValueLabel.Text = _visualization.SolutionPath.Count == 0 ? "—" : _visualization.SolutionPath.Count.ToString();
        CostValueLabel.Text = _maze is null || _visualization.SolutionPath.Count == 0 ? "—" : _visualization.SolutionPath.Skip(1).Sum(position => _maze[position].TerrainCost).ToString();
        ElapsedValueLabel.Text = _stopwatch.Elapsed == TimeSpan.Zero ? "—" : $"{_stopwatch.Elapsed.TotalMilliseconds:0} ms";
        CanvasStatusLabel.Text = _maze is null
            ? "No maze generated"
            : _state switch
            {
                MazeRunState.ReadyToSolve => $"Maze ready · seed {_maze.Seed} · press Solve maze to begin",
                MazeRunState.Solved => $"Solved · seed {_maze.Seed}",
                MazeRunState.Generating => $"Generating maze · seed {_maze.Seed}",
                MazeRunState.GenerationPaused => $"Generation paused · seed {_maze.Seed}",
                MazeRunState.Solving => $"Solving maze · seed {_maze.Seed}",
                MazeRunState.SolvingPaused => $"Solve paused · seed {_maze.Seed}",
                _ => $"{_state} · seed {_maze.Seed}"
            };
    }

    private void UpdateControls()
    {
        bool active = IsActive;
        bool paused = _state is MazeRunState.GenerationPaused or MazeRunState.SolvingPaused;
        GenerateButton.IsEnabled = !active;
        SolveButton.IsEnabled = _maze is not null && !active && (_state is MazeRunState.ReadyToSolve or MazeRunState.Solved);
        PauseButton.IsEnabled = active;
        PauseButton.Content = paused ? "Resume" : "Pause";
        StepButton.IsEnabled = paused;
        ResetSearchButton.IsEnabled = _maze is not null && !active;
        ResetMazeButton.IsEnabled = _maze is not null;
        GeneratorComboBox.IsEnabled = !active;
        SolverComboBox.IsEnabled = !active;
        WidthTextBox.IsEnabled = !active;
        HeightTextBox.IsEnabled = !active;
        SeedTextBox.IsEnabled = !active;
        LoopsSlider.IsEnabled = !active;
        WeightedTerrainCheckBox.IsEnabled = _maze is not null && !active;
    }

    private bool IsActive => _state is MazeRunState.Generating or MazeRunState.GenerationPaused or MazeRunState.Solving or MazeRunState.SolvingPaused;
    private IMazeGenerator SelectedGenerator => GeneratorRegistry.Find(GeneratorComboBox.SelectedItem as string ?? GeneratorRegistry.All[0].Metadata.Name);
    private IMazeSolver SelectedSolver => SolverRegistry.Find(SolverComboBox.SelectedItem as string ?? SolverRegistry.All[0].Metadata.Name);

    private void CancelActiveRun()
    {
        _cancellation?.Cancel();
        _cancellation?.Dispose();
        _cancellation = null;
        _animation.Resume();
    }

    private async void ShowToast(string message)
    {
        int version = ++_toastVersion;
        ToastTextBlock.Text = message;
        ToastBorder.Visibility = Visibility.Visible;
        await Task.Delay(2300);
        if (version == _toastVersion) ToastBorder.Visibility = Visibility.Collapsed;
    }
}
