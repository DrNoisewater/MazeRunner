using System.Windows;
using System.Windows.Media;
using MazeRunner.Models;

namespace MazeRunner.Services;

public static class ThemeService
{
    public static void Apply(AppSettings settings)
    {
        ResourceDictionary resources = Application.Current.Resources;
        bool light = string.Equals(settings.Theme, "Light", StringComparison.OrdinalIgnoreCase);
        SetBrush(resources, "AppBackgroundBrush", light ? "#EEF2F6" : "#0B0F14");
        SetBrush(resources, "SurfaceBrush", light ? "#FFFFFF" : "#111821");
        SetBrush(resources, "SurfaceAltBrush", light ? "#F5F7FA" : "#151E29");
        SetBrush(resources, "SurfaceRaisedBrush", light ? "#E9EEF4" : "#1A2531");
        SetBrush(resources, "BorderBrush", light ? "#D5DDE6" : "#263442");
        SetBrush(resources, "BorderStrongBrush", light ? "#B8C4D0" : "#344657");
        SetBrush(resources, "TextPrimaryBrush", light ? "#1B2633" : "#F1F5F9");
        SetBrush(resources, "TextSecondaryBrush", light ? "#506070" : "#AAB7C5");
        SetBrush(resources, "TextMutedBrush", light ? "#748292" : "#748295");
        SetBrush(resources, "MazeCanvasBackgroundBrush", light ? "#E8EEF4" : "#0D131B");
        SetBrush(resources, "MazeCellBrush", light ? "#F8FBFD" : "#101923");
        SetBrush(resources, "MazeCellAlternateBrush", light ? "#F0F5F9" : "#111B26");
        SetBrush(resources, "MazeCellGridBrush", light ? "#DAE3EC" : "#1B2A38");
        SetBrush(resources, "MazeWallBrush", light ? "#526B84" : "#587089");

        string accent = settings.Accent switch
        {
            "Blue" => "#76A9F4",
            "Green" => "#65C18C",
            "Purple" => "#B58ADB",
            _ => "#D4A55D"
        };
        SetBrush(resources, "AccentBrush", accent);
        SetBrush(resources, "AccentHoverBrush", Lighten(accent));
        SetBrush(resources, "AccentMutedBrush", accent, 0.62);
        SetBrush(resources, "AccentSoftBrush", accent, 0.20);
        SetBrush(resources, "MazeSolutionBrush", light ? "#D7F4E2" : "#1D4B38");
        SetBrush(resources, "MazeSolutionRouteBrush", light ? "#23834C" : "#71E39D");
    }

    private static void SetBrush(ResourceDictionary resources, string key, string color, double opacity = 1.0)
    {
        SolidColorBrush brush = new((Color)ColorConverter.ConvertFromString(color)) { Opacity = opacity };
        brush.Freeze();
        resources[key] = brush;
    }

    private static string Lighten(string color)
    {
        Color source = (Color)ColorConverter.ConvertFromString(color);
        byte Mix(byte value) => (byte)Math.Min(255, value + 18);
        return $"#{Mix(source.R):X2}{Mix(source.G):X2}{Mix(source.B):X2}";
    }
}
