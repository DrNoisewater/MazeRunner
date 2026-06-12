using MazeRunner.Infrastructure;

namespace MazeRunner.Models;

public sealed class NavigationItem : ObservableObject
{
    private bool _isSelected;

    public NavigationItem(AppSection section, string shortLabel, string title, string description)
    {
        Section = section;
        ShortLabel = shortLabel;
        Title = title;
        Description = description;
    }

    public AppSection Section { get; }
    public string ShortLabel { get; }
    public string Title { get; }
    public string Description { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
