using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Gui.Models;

namespace KaedePhi.Tool.Gui.ViewModels;

public sealed class ExportViewModel : INotifyPropertyChanged
{
    private ChartType _selectedFormat;
    private ChartType _sourceFormat;
    private bool _useStream;
    private bool _indentedOutput;
    private bool _isExporting;
    private string _statusText = string.Empty;

    // 转换选项字段
    private double _peSpeedConversionRatio = 14d / 9d;
    private double _peTrailingBeatPadding = 1d / 64d;
    private double _peUnsupportedEasingPrecision = 64d;
    private double _peMisalignedXyEventPrecision = 64d;
    private double _peAlphaCutPrecision = 64d;
    private double _peAlphaCutTolerance = 0.1d;
    private double _peSpeedCutPrecision = 64d;
    private double _peSpeedCutTolerance = 0.1d;
    private float _phigrosDefaultBpm = 120f;
    private double _phigrosEasingPrecision = 64d;
    private double _phigrosMisalignedXyEventPrecision = 64d;
    private double _phigrosAlphaCutPrecision = 64d;
    private double _phigrosAlphaCutTolerance = 0.1d;
    private double _phigrosSpeedCutPrecision = 64d;
    private double _unbindPrecision = 64d;
    private double _unbindTolerance = 0.1d;
    private bool _unbindClassicMode;
    private bool _unbindCompress = true;
    private double _multiLayerMergePrecision = 64d;
    private double _multiLayerMergeTolerance = 0.1d;
    private bool _multiLayerMergeClassicMode;
    private bool _multiLayerMergeCompress = true;
    private bool _removeAttachUiLine;
    private bool _removeTextureLine;

    /// <summary>
    /// 获取谱面格式的级别（数值越大级别越高）
    /// PhiChain > RePhiEdit > PhiEdit > PhiFans > PhigrosV3 > PhigrosV1
    /// </summary>
    private static int GetFormatLevel(ChartType format) => format switch
    {
        ChartType.PhiChain => 6,
        ChartType.RePhiEdit => 5,
        ChartType.PhiEdit => 4,
        ChartType.PhiFans => 3,
        ChartType.PhigrosV3 => 2,
        ChartType.PhigrosV1 => 1,
        _ => 0
    };

    /// <summary>
    /// 判断是否是降级转换（从高级格式转换到低级格式）
    /// </summary>
    private bool IsDowngrade => GetFormatLevel(_sourceFormat) > GetFormatLevel(_selectedFormat);

    public List<ChartType> AvailableFormats { get; } = new()
    {
        ChartType.RePhiEdit,
        ChartType.PhiEdit,
        ChartType.PhigrosV3
    };

    public ChartType SourceFormat
    {
        get => _sourceFormat;
        set
        {
            _sourceFormat = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowConversionOptions));
            OnPropertyChanged(nameof(IsTargetPe));
            OnPropertyChanged(nameof(IsTargetPhigros));
            OnPropertyChanged(nameof(IsSourcePe));
            OnPropertyChanged(nameof(IsSourcePhigros));
        }
    }

    public ChartType SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            _selectedFormat = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsJsonFormat));
            OnPropertyChanged(nameof(ShowConversionOptions));
            OnPropertyChanged(nameof(IsTargetPe));
            OnPropertyChanged(nameof(IsTargetPhigros));
            OnPropertyChanged(nameof(IsSourcePe));
            OnPropertyChanged(nameof(IsSourcePhigros));
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

    /// <summary>
    /// 是否需要显示转换选项（仅在降级转换时显示）
    /// </summary>
    public bool ShowConversionOptions => IsDowngrade;

    /// <summary>
    /// 目标格式是否为 PhiEdit（显示"转换到 PhiEdit"的选项）
    /// </summary>
    public bool IsTargetPe => ShowConversionOptions && _selectedFormat == ChartType.PhiEdit;

    /// <summary>
    /// 目标格式是否为 PhigrosV3（显示"转换到 PhigrosV3"的选项）
    /// </summary>
    public bool IsTargetPhigros => ShowConversionOptions && _selectedFormat == ChartType.PhigrosV3;

    /// <summary>
    /// 源格式是否为 PhiEdit（显示"从 PhiEdit 转换"的选项）
    /// </summary>
    public bool IsSourcePe => ShowConversionOptions && _sourceFormat == ChartType.PhiEdit;

    /// <summary>
    /// 源格式是否为 PhigrosV3（显示"从 PhigrosV3 转换"的选项）
    /// </summary>
    public bool IsSourcePhigros => ShowConversionOptions && _sourceFormat == ChartType.PhigrosV3;

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

    // ---- PE 转换选项 ----

    public double PeSpeedConversionRatio
    {
        get => _peSpeedConversionRatio;
        set { _peSpeedConversionRatio = value; OnPropertyChanged(); }
    }

    public double PeTrailingBeatPadding
    {
        get => _peTrailingBeatPadding;
        set { _peTrailingBeatPadding = value; OnPropertyChanged(); }
    }

    public double PeUnsupportedEasingPrecision
    {
        get => _peUnsupportedEasingPrecision;
        set { _peUnsupportedEasingPrecision = value; OnPropertyChanged(); }
    }

    public double PeMisalignedXyEventPrecision
    {
        get => _peMisalignedXyEventPrecision;
        set { _peMisalignedXyEventPrecision = value; OnPropertyChanged(); }
    }

    public double PeAlphaCutPrecision
    {
        get => _peAlphaCutPrecision;
        set { _peAlphaCutPrecision = value; OnPropertyChanged(); }
    }

    public double PeAlphaCutTolerance
    {
        get => _peAlphaCutTolerance;
        set { _peAlphaCutTolerance = value; OnPropertyChanged(); }
    }

    public double PeSpeedCutPrecision
    {
        get => _peSpeedCutPrecision;
        set { _peSpeedCutPrecision = value; OnPropertyChanged(); }
    }

    public double PeSpeedCutTolerance
    {
        get => _peSpeedCutTolerance;
        set { _peSpeedCutTolerance = value; OnPropertyChanged(); }
    }

    // ---- PhigrosV3 转换选项 ----

    public float PhigrosDefaultBpm
    {
        get => _phigrosDefaultBpm;
        set { _phigrosDefaultBpm = value; OnPropertyChanged(); }
    }

    public double PhigrosEasingPrecision
    {
        get => _phigrosEasingPrecision;
        set { _phigrosEasingPrecision = value; OnPropertyChanged(); }
    }

    public double PhigrosMisalignedXyEventPrecision
    {
        get => _phigrosMisalignedXyEventPrecision;
        set { _phigrosMisalignedXyEventPrecision = value; OnPropertyChanged(); }
    }

    public double PhigrosAlphaCutPrecision
    {
        get => _phigrosAlphaCutPrecision;
        set { _phigrosAlphaCutPrecision = value; OnPropertyChanged(); }
    }

    public double PhigrosAlphaCutTolerance
    {
        get => _phigrosAlphaCutTolerance;
        set { _phigrosAlphaCutTolerance = value; OnPropertyChanged(); }
    }

    public double PhigrosSpeedCutPrecision
    {
        get => _phigrosSpeedCutPrecision;
        set { _phigrosSpeedCutPrecision = value; OnPropertyChanged(); }
    }

    // ---- 通用选项 ----

    public double UnbindPrecision
    {
        get => _unbindPrecision;
        set { _unbindPrecision = value; OnPropertyChanged(); }
    }

    public double UnbindTolerance
    {
        get => _unbindTolerance;
        set { _unbindTolerance = value; OnPropertyChanged(); }
    }

    public bool UnbindClassicMode
    {
        get => _unbindClassicMode;
        set { _unbindClassicMode = value; OnPropertyChanged(); }
    }

    public double MultiLayerMergePrecision
    {
        get => _multiLayerMergePrecision;
        set { _multiLayerMergePrecision = value; OnPropertyChanged(); }
    }

    public double MultiLayerMergeTolerance
    {
        get => _multiLayerMergeTolerance;
        set { _multiLayerMergeTolerance = value; OnPropertyChanged(); }
    }

    public bool MultiLayerMergeClassicMode
    {
        get => _multiLayerMergeClassicMode;
        set { _multiLayerMergeClassicMode = value; OnPropertyChanged(); }
    }

    public bool UnbindCompress
    {
        get => _unbindCompress;
        set { _unbindCompress = value; OnPropertyChanged(); }
    }

    public bool MultiLayerMergeCompress
    {
        get => _multiLayerMergeCompress;
        set { _multiLayerMergeCompress = value; OnPropertyChanged(); }
    }

    public bool RemoveAttachUiLine
    {
        get => _removeAttachUiLine;
        set { _removeAttachUiLine = value; OnPropertyChanged(); }
    }

    public bool RemoveTextureLine
    {
        get => _removeTextureLine;
        set { _removeTextureLine = value; OnPropertyChanged(); }
    }

    public event Action? RequestExport;
    public event Action? RequestReturnToImport;

    public void OnExportClicked() => RequestExport?.Invoke();
    public void OnReturnClicked() => RequestReturnToImport?.Invoke();

    /// <summary>
    /// 从配置加载转换选项默认值
    /// </summary>
    public void ApplyConversionDefaults(ConvertDefaultsConfig config)
    {
        PeSpeedConversionRatio = config.PeSpeedConversionRatio;
        PeTrailingBeatPadding = config.PeTrailingBeatPadding;
        PeUnsupportedEasingPrecision = config.PeUnsupportedEasingPrecision;
        PeMisalignedXyEventPrecision = config.PeMisalignedXyEventPrecision;
        PeAlphaCutPrecision = config.PeAlphaCutPrecision;
        PeAlphaCutTolerance = config.PeAlphaCutTolerance;
        PeSpeedCutPrecision = config.PeSpeedCutPrecision;
        PeSpeedCutTolerance = config.PeSpeedCutTolerance;
        PhigrosDefaultBpm = config.PhigrosDefaultBpm;
        PhigrosEasingPrecision = config.PhigrosEasingPrecision;
        PhigrosMisalignedXyEventPrecision = config.PhigrosMisalignedXyEventPrecision;
        PhigrosAlphaCutPrecision = config.PhigrosAlphaCutPrecision;
        PhigrosAlphaCutTolerance = config.PhigrosAlphaCutTolerance;
        PhigrosSpeedCutPrecision = config.PhigrosSpeedCutPrecision;
        UnbindPrecision = config.UnbindPrecision;
        UnbindTolerance = config.UnbindTolerance;
        UnbindClassicMode = config.UnbindClassicMode;
        MultiLayerMergePrecision = config.MultiLayerMergePrecision;
        MultiLayerMergeTolerance = config.MultiLayerMergeTolerance;
        MultiLayerMergeClassicMode = config.MultiLayerMergeClassicMode;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
