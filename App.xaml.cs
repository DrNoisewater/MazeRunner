using System.Windows;
using MazeRunner.Services;

namespace MazeRunner;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppState.Initialize();
        ThemeService.Apply(AppState.Settings);
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppState.Save();
        base.OnExit(e);
    }
}
