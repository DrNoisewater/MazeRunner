using System.Collections.ObjectModel;
using MazeRunner.Infrastructure;
using MazeRunner.Models;

namespace MazeRunner.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private AppSection _currentSection;

    public MainViewModel()
    {
        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new(AppSection.Visualizer, "VI", "Visualizer", "Generate and solve mazes interactively."),
            new(AppSection.Compare, "CO", "Compare", "Benchmark solvers against the same maze."),
            new(AppSection.History, "HI", "History", "Review completed visualization runs."),
            new(AppSection.Settings, "SE", "Settings", "Tune appearance and playback defaults.")
        };

        NavigateCommand = new RelayCommand<AppSection>(Navigate);
        Navigate(AppSection.Visualizer);
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; }
    public RelayCommand<AppSection> NavigateCommand { get; }

    public AppSection CurrentSection
    {
        get => _currentSection;
        private set
        {
            if (!SetProperty(ref _currentSection, value))
            {
                return;
            }

            OnPropertyChanged(nameof(CurrentSectionTitle));
            OnPropertyChanged(nameof(CurrentSectionDescription));
        }
    }

    public string CurrentSectionTitle => NavigationItems.First(item => item.Section == CurrentSection).Title;
    public string CurrentSectionDescription => NavigationItems.First(item => item.Section == CurrentSection).Description;

    private void Navigate(AppSection section)
    {
        CurrentSection = section;
        foreach (NavigationItem item in NavigationItems)
        {
            item.IsSelected = item.Section == section;
        }
    }
}
