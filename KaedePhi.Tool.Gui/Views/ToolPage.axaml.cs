using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KaedePhi.Tool.Gui.Models;
using KaedePhi.Tool.Gui.ViewModels;

namespace KaedePhi.Tool.Gui.Views;

public partial class ToolPage : UserControl
{
    public ToolPage()
    {
        InitializeComponent();
    }

    private void OnToolCardPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Tag is string toolId && DataContext is ToolViewModel vm)
        {
            vm.SelectedTool = vm.Tools.Find(t => t.ToolId == toolId);
        }
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
}
