using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using KaedePhi.Tool.Gui.ViewModels;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.Gui.Views;

public partial class ImportPage : UserControl
{
    private bool _isPicking;
    private Border? _dropOverlay;

    public ImportPage()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _dropOverlay = this.FindControl<Border>("DropOverlay");
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (DataContext is ImportViewModel vm && vm.IsLoading)
            return;
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
            if (_dropOverlay != null)
                _dropOverlay.IsVisible = true;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        if (_dropOverlay != null)
            _dropOverlay.IsVisible = false;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (_dropOverlay != null)
            _dropOverlay.IsVisible = false;

        if (DataContext is not ImportViewModel vm || vm.IsLoading)
            return;

        foreach (var item in e.DataTransfer.Items)
        {
            if (item.TryGetRaw(DataFormat.File) is IStorageItem storageItem)
            {
                var path = storageItem.TryGetLocalPath();
                if (!string.IsNullOrEmpty(path))
                {
                    vm.OnFileSelected(path);
                    return;
                }
            }
        }
    }

    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        if (_isPicking)
            return;
        _isPicking = true;

        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
                return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = import_file_picker_title,
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType(file_type_json) { Patterns = new[] { "*.json" } },
                        new FilePickerFileType(file_type_pe_chart) { Patterns = new[] { "*.pec" } },
                        new FilePickerFileType(file_type_all) { Patterns = new[] { "*.*" } },
                    },
                }
            );

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
