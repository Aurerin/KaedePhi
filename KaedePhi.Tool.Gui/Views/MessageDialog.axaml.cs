using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace KaedePhi.Tool.Gui.Views;

public partial class MessageDialog : Window
{
    public enum DialogType
    {
        Info,
        Success,
        Warning,
        Error,
    }

    public MessageDialog()
    {
        InitializeComponent();
    }

    public static MessageDialog ShowInfo(Window owner, string title, string message) =>
        Show(owner, title, message, DialogType.Info);

    public static MessageDialog ShowSuccess(Window owner, string title, string message) =>
        Show(owner, title, message, DialogType.Success);

    public static MessageDialog ShowWarning(Window owner, string title, string message) =>
        Show(owner, title, message, DialogType.Warning);

    public static MessageDialog ShowError(Window owner, string title, string message) =>
        Show(owner, title, message, DialogType.Error);

    private static MessageDialog Show(Window owner, string title, string message, DialogType type)
    {
        var dialog = new MessageDialog
        {
            TitleText = { Text = title },
            MessageText = { Text = message },
        };

        dialog.IconText.Text = type switch
        {
            DialogType.Success => "\uf058",
            DialogType.Warning => "\uf071",
            DialogType.Error => "\uf057",
            _ => "\uf05a",
        };

        dialog.IconText.Foreground = type switch
        {
            DialogType.Success => new SolidColorBrush(Color.Parse("#4CAF50")),
            DialogType.Warning => new SolidColorBrush(Color.Parse("#FF9800")),
            DialogType.Error => new SolidColorBrush(Color.Parse("#F44336")),
            _ => new SolidColorBrush(Color.Parse("#2196F3")),
        };

        dialog.ShowDialog(owner);
        return dialog;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
