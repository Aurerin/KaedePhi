using Avalonia.Controls;
using Avalonia.Interactivity;
using KaedePhi.Tool.Gui.ViewModels;

namespace KaedePhi.Tool.Gui.Views;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void OnReturnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.OnReturnClicked();
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.OnSaveClicked();
    }

    private void OnResetClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.OnResetClicked();
    }
}
