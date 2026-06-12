using System.Windows;
using System.Windows.Controls;
using MazeRunner.Services;

namespace MazeRunner.Views;

public partial class HistoryView : UserControl
{
    public HistoryView()
    {
        InitializeComponent();
        Loaded += HistoryView_Loaded;
        Unloaded += HistoryView_Unloaded;
    }

    private void HistoryView_Loaded(object sender, RoutedEventArgs e)
    {
        AppState.HistoryChanged += AppState_HistoryChanged;
        Refresh();
    }

    private void HistoryView_Unloaded(object sender, RoutedEventArgs e)
    {
        AppState.HistoryChanged -= AppState_HistoryChanged;
    }

    private void AppState_HistoryChanged(object? sender, EventArgs e) => Refresh();

    private void Refresh()
    {
        HistoryDataGrid.ItemsSource = AppState.History.Select(entry => new HistoryRow
        {
            Timestamp = entry.Timestamp,
            Dimensions = $"{entry.Width} × {entry.Height}",
            Seed = entry.Seed,
            Generator = entry.Generator,
            Solver = entry.Solver,
            ExploredCells = entry.ExploredCells,
            SolutionLength = entry.SolutionLength,
            WeightedPathCost = entry.WeightedPathCost,
            Elapsed = entry.ElapsedMilliseconds,
            Efficiency = entry.EfficiencyPercent
        }).ToList();
    }

    private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (AppState.Settings.ConfirmBeforeClearingHistory)
        {
            MessageBoxResult result = MessageBox.Show("Clear all stored MazeRunner history?", "Clear history", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
        }
        AppState.ClearHistory();
    }

    private sealed class HistoryRow
    {
        public DateTime Timestamp { get; set; }
        public string Dimensions { get; set; } = string.Empty;
        public int Seed { get; set; }
        public string Generator { get; set; } = string.Empty;
        public string Solver { get; set; } = string.Empty;
        public int ExploredCells { get; set; }
        public int SolutionLength { get; set; }
        public int WeightedPathCost { get; set; }
        public double Elapsed { get; set; }
        public double Efficiency { get; set; }
    }
}
