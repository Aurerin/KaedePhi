using Avalonia.Controls;
using Avalonia.Interactivity;
using KaedePhi.Tool.Gui.ViewModels;

namespace KaedePhi.Tool.Gui.Views;

public partial class ProcessingPage : UserControl
{
    public ProcessingPage()
    {
        InitializeComponent();
    }

    private void OnReturnToToolsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ProcessingViewModel vm)
            vm.OnReturnToToolsClicked();
    }

    private void OnReturnToImportClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ProcessingViewModel vm)
            vm.OnReturnToImportClicked();
    }

    private void OnGoToExportClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ProcessingViewModel vm)
            vm.OnGoToExportClicked();
    }
}
