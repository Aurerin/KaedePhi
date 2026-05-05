using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using KaedePhi.Tool.Gui.ViewModels;

namespace KaedePhi.Tool.Gui.Views;

public partial class ImportPage : UserControl
{
    private bool _isPicking;

    public ImportPage()
    {
        InitializeComponent();
    }

    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        if (_isPicking) return;
        _isPicking = true;

        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Chart File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } },
                    new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
                }
            });

            if (files.Count > 0 && DataContext is ImportViewModel vm)
            {
                var path = files[0].TryGetLocalPath();
                if (!string.IsNullOrEmpty(path))
                {
                    vm.OnFileSelected(path);
                }
            }
        }
        finally
        {
            _isPicking = false;
        }
    }
}
