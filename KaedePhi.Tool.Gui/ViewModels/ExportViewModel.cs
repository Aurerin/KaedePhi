using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Gui.ViewModels;

public sealed class ExportViewModel : INotifyPropertyChanged
{
    private ChartType _selectedFormat;
    private bool _useStream;
    private bool _isExporting;
    private string _statusText = string.Empty;

    public List<ChartType> AvailableFormats { get; } = new()
    {
        ChartType.RePhiEdit,
        ChartType.PhiEdit
    };

    public ChartType SelectedFormat
    {
        get => _selectedFormat;
        set { _selectedFormat = value; OnPropertyChanged(); }
    }

    public bool UseStream
    {
        get => _useStream;
        set { _useStream = value; OnPropertyChanged(); }
    }

    public bool IsExporting
    {
        get => _isExporting;
        set { _isExporting = value; OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public event Action? RequestExport;
    public event Action? RequestReturnToImport;

    public void OnExportClicked() => RequestExport?.Invoke();
    public void OnReturnClicked() => RequestReturnToImport?.Invoke();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
