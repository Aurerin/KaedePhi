using Avalonia.Controls;
using Avalonia.Interactivity;
using KaedePhi.Tool.Gui.ViewModels;

namespace KaedePhi.Tool.Gui.Views;

public partial class ToolPage : UserControl
{
    public ToolPage()
    {
        InitializeComponent();
    }

    private void OnRunClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ToolViewModel vm)
            vm.OnRunClicked();
    }

    private void OnExportClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ToolViewModel vm)
            vm.OnExportClicked();
    }

    private void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ToolViewModel vm)
            vm.OnSettingsClicked();
    }

    private void OnReturnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ToolViewModel vm)
            vm.OnReturnToImportClicked();
    }
}
