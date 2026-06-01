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
    
    /// <summary>
    /// 获取谱面格式的级别（数值越大级别越高）
    /// RePhiEdit > PhiChain > PhiEdit > PhiFans > PhigrosV3 > PhigrosV1
    /// </summary>
    private static int GetFormatLevel(ChartType format) => format switch
    {
        ChartType.RePhiEdit => 6,
        ChartType.PhiChain => 5,
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
            OnPropertyChanged(nameof(ShowPeOptions));
            OnPropertyChanged(nameof(ShowPhigrosOptions));
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
            OnPropertyChanged(nameof(ShowPeOptions));
            OnPropertyChanged(nameof(ShowPhigrosOptions));
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
    /// 是否显示 PhiEdit 专属转换选项（降级且目标为 PhiEdit）
    /// </summary>
    public bool ShowPeOptions => ShowConversionOptions && _selectedFormat == ChartType.PhiEdit;

    /// <summary>
    /// 是否显示 PhigrosV3 专属转换选项（降级且目标为 PhigrosV3）
    /// </summary>
    public bool ShowPhigrosOptions => ShowConversionOptions && _selectedFormat == ChartType.PhigrosV3;

    public bool UseStream
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否格式化（缩进）JSON 输出，仅对 JSON 格式生效
    /// </summary>
    public bool IndentedOutput
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsExporting
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    // ---- PE 转换选项 ----

    public double PeSpeedConversionRatio
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 14d / 9d;

    public double PeTrailingBeatPadding
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 1d / 64d;

    public double PeUnsupportedEasingPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PeMisalignedXyEventPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PeAlphaCutPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PeAlphaCutTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1d;

    public double PeSpeedCutPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PeSpeedCutTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1d;

    // ---- PhigrosV3 转换选项 ----

    public float PhigrosDefaultBpm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 120f;

    public double PhigrosEasingPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PhigrosMisalignedXyEventPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PhigrosAlphaCutPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double PhigrosAlphaCutTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1d;

    public double PhigrosSpeedCutPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    // ---- 通用选项 ----

    public double UnbindPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double UnbindTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1d;

    public bool UnbindClassicMode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double MultiLayerMergePrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64d;

    public double MultiLayerMergeTolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1d;

    public bool MultiLayerMergeClassicMode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool UnbindCompress
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool MultiLayerMergeCompress
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool RemoveAttachUiLine
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool RemoveTextureLine
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool FilterFakeNotes
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool NegativeAlphaElevation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double NegativeAlphaStep
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 4.0d;

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
        FilterFakeNotes = config.PhigrosFilterFakeNotes;
        NegativeAlphaElevation = config.PhigrosNegativeAlphaElevation;
        NegativeAlphaStep = config.PhigrosNegativeAlphaStep;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
