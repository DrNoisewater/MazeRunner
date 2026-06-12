using System.Windows;
using System.Windows.Controls;
using MazeRunner.Algorithms.Solving;
using MazeRunner.Services;

namespace MazeRunner.Views;

public partial class CompareView : UserControl
{
    public CompareView()
    {
        InitializeComponent();
        Loaded += CompareView_Loaded;
    }

    private void CompareView_Loaded(object sender, RoutedEventArgs e)
    {
        List<string> solvers = SolverRegistry.All.Select(item => item.Metadata.Name).ToList();
        SolverAComboBox.ItemsSource = solvers;
        SolverBComboBox.ItemsSource = solvers;
        SolverAComboBox.SelectedIndex = 0;
        SolverBComboBox.SelectedIndex = Math.Min(3, solvers.Count - 1);
        MazeStatusLabel.Text = AppState.CurrentMaze is null
            ? "Generate a maze in the Visualizer first."
            : $"Current maze: {AppState.CurrentMaze.Width} × {AppState.CurrentMaze.Height} · seed {AppState.CurrentMaze.Seed}";
    }

    private void RunComparisonButton_Click(object sender, RoutedEventArgs e)
    {
        if (AppState.CurrentMaze is null)
        {
            MessageBox.Show("Generate or import a maze in the Visualizer first.", "No maze available", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (SolverAComboBox.SelectedItem is not string first || SolverBComboBox.SelectedItem is not string second) return;
        SolverComparisonResult resultA = ComparisonService.Evaluate(AppState.CurrentMaze.Clone(), SolverRegistry.Find(first));
        SolverComparisonResult resultB = ComparisonService.Evaluate(AppState.CurrentMaze.Clone(), SolverRegistry.Find(second));
        SolverAResultLabel.Text = Format(resultA);
        SolverBResultLabel.Text = Format(resultB);
    }

    private static string Format(SolverComparisonResult result) =>
        $"{result.Solver}\n\nCells explored   {result.Explored:N0}\nPath length      {result.PathLength:N0}\nPath cost        {result.PathCost:N0}\nSolve time       {result.ElapsedMilliseconds:0.###} ms\nEfficiency       {result.Efficiency:0.0}%";
}
