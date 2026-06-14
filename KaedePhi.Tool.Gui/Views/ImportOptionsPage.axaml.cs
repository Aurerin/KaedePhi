using Avalonia.Controls;
using Avalonia.Interactivity;
using KaedePhi.Tool.Gui.ViewModels;

namespace KaedePhi.Tool.Gui.Views;

public partial class ImportOptionsPage : UserControl
{
    public ImportOptionsPage()
    {
        InitializeComponent();
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ImportOptionsViewModel vm)
            vm.OnConfirmClicked();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ImportOptionsViewModel vm)
            vm.OnCancelClicked();
    }

    private void OnCancelImportClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ImportOptionsViewModel vm)
            vm.OnCancelImportClicked();
    }
}
