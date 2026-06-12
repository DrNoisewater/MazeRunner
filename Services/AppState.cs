using System.IO;
using System.Text.Json;
using MazeRunner.Models;

namespace MazeRunner.Services;

public static class AppState
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MazeRunner");
    private static readonly string SettingsFile = Path.Combine(DataDirectory, "settings.json");
    private static readonly string HistoryFile = Path.Combine(DataDirectory, "history.json");

    public static AppSettings Settings { get; private set; } = new();
    public static List<RunHistoryEntry> History { get; private set; } = new();
    public static Maze? CurrentMaze { get; set; }

    public static event EventHandler? HistoryChanged;
    public static event EventHandler? SettingsChanged;

    public static void Initialize()
    {
        Directory.CreateDirectory(DataDirectory);
        Settings = LoadOrDefault(SettingsFile, new AppSettings());
        History = LoadOrDefault(HistoryFile, new List<RunHistoryEntry>());
    }

    public static void AddHistory(RunHistoryEntry entry)
    {
        if (!Settings.SaveHistory) return;
        History.Insert(0, entry);
        if (History.Count > 500) History.RemoveRange(500, History.Count - 500);
        SaveHistory();
        HistoryChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void ClearHistory()
    {
        History.Clear();
        SaveHistory();
        HistoryChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void NotifySettingsChanged()
    {
        SaveSettings();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void Save()
    {
        SaveSettings();
        SaveHistory();
    }

    private static T LoadOrDefault<T>(string file, T fallback)
    {
        try
        {
            return File.Exists(file)
                ? JsonSerializer.Deserialize<T>(File.ReadAllText(file), JsonOptions) ?? fallback
                : fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static void SaveSettings()
    {
        Directory.CreateDirectory(DataDirectory);
        File.WriteAllText(SettingsFile, JsonSerializer.Serialize(Settings, JsonOptions));
    }

    private static void SaveHistory()
    {
        Directory.CreateDirectory(DataDirectory);
        File.WriteAllText(HistoryFile, JsonSerializer.Serialize(History, JsonOptions));
    }
}
