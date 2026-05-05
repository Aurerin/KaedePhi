using Avalonia.Controls;
using Avalonia.Interactivity;
using KaedePhi.Tool.Gui.ViewModels;

namespace KaedePhi.Tool.Gui.Views;

public partial class ExportPage : UserControl
{
    public ExportPage()
    {
        InitializeComponent();
    }

    private void OnExportClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ExportViewModel vm)
            vm.OnExportClicked();
    }

    private void OnReturnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ExportViewModel vm)
            vm.OnReturnClicked();
    }
}
