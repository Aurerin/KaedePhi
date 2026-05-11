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
    private bool _indentedOutput;
    private bool _isExporting;
    private string _statusText = string.Empty;

    public List<ChartType> AvailableFormats { get; } = new()
    {
        ChartType.RePhiEdit,
        ChartType.PhiEdit,
        ChartType.PhigrosV3
    };

    public ChartType SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            _selectedFormat = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsJsonFormat));
            // 切换到非 JSON 格式时，自动清除格式化选项
            if (!IsJsonFormat)
                IndentedOutput = false;
        }
    }

    /// <summary>
    /// 当前选择的导出格式是否为 JSON 格式（true 时才显示格式化输出选项）
    /// </summary>
    public bool IsJsonFormat => _selectedFormat switch
    {
        ChartType.PhiEdit => false,
        _ => true
    };

    public bool UseStream
    {
        get => _useStream;
        set { _useStream = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 是否格式化（缩进）JSON 输出，仅对 JSON 格式生效
    /// </summary>
    public bool IndentedOutput
    {
        get => _indentedOutput;
        set { _indentedOutput = value; OnPropertyChanged(); }
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
