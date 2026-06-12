using System.Windows;
using System.Windows.Controls;
using MazeRunner.Services;

namespace MazeRunner.Views;

public partial class SettingsView : UserControl
{
    private bool _loading;

    public SettingsView()
    {
        InitializeComponent();
        Loaded += SettingsView_Loaded;
    }

    private void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        _loading = true;
        SelectComboItem(ThemeComboBox, AppState.Settings.Theme);
        SelectComboItem(AccentComboBox, AppState.Settings.Accent);
        ShowGridCheckBox.IsChecked = AppState.Settings.ShowGrid;
        ShowMarkersCheckBox.IsChecked = AppState.Settings.ShowDirectionalMarkers;
        ReduceMotionCheckBox.IsChecked = AppState.Settings.ReduceMotion;
        SaveHistoryCheckBox.IsChecked = AppState.Settings.SaveHistory;
        ConfirmClearCheckBox.IsChecked = AppState.Settings.ConfirmBeforeClearingHistory;
        DefaultWidthTextBox.Text = AppState.Settings.DefaultWidth.ToString();
        DefaultHeightTextBox.Text = AppState.Settings.DefaultHeight.ToString();
        SpeedSlider.Value = AppState.Settings.AnimationSpeed;
        SpeedLabel.Text = $"{AppState.Settings.AnimationSpeed}%";
        _loading = false;
    }

    private void SettingChanged(object sender, RoutedEventArgs e) => Save();
    private void SettingTextChanged(object sender, TextChangedEventArgs e) => Save();

    private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SpeedLabel is not null) SpeedLabel.Text = $"{(int)e.NewValue}%";
        Save();
    }

    private void Save()
    {
        if (_loading || ThemeComboBox is null || AccentComboBox is null) return;
        AppState.Settings.Theme = GetComboText(ThemeComboBox);
        AppState.Settings.Accent = GetComboText(AccentComboBox);
        AppState.Settings.ShowGrid = ShowGridCheckBox.IsChecked == true;
        AppState.Settings.ShowDirectionalMarkers = ShowMarkersCheckBox.IsChecked == true;
        AppState.Settings.ReduceMotion = ReduceMotionCheckBox.IsChecked == true;
        AppState.Settings.SaveHistory = SaveHistoryCheckBox.IsChecked == true;
        AppState.Settings.ConfirmBeforeClearingHistory = ConfirmClearCheckBox.IsChecked == true;
        if (int.TryParse(DefaultWidthTextBox.Text, out int width) && width is >= 2 and <= 151) AppState.Settings.DefaultWidth = width;
        if (int.TryParse(DefaultHeightTextBox.Text, out int height) && height is >= 2 and <= 101) AppState.Settings.DefaultHeight = height;
        AppState.Settings.AnimationSpeed = (int)SpeedSlider.Value;
        ThemeService.Apply(AppState.Settings);
        AppState.NotifySettingsChanged();
    }

    private static string GetComboText(ComboBox comboBox) => (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;

    private static void SelectComboItem(ComboBox comboBox, string content)
    {
        foreach (ComboBoxItem item in comboBox.Items)
        {
            if (string.Equals(item.Content?.ToString(), content, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = item;
                return;
            }
        }
        comboBox.SelectedIndex = 0;
    }
}
