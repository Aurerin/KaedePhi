using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Gui.Models;
using static KaedePhi.Tool.Gui.Models.FontAwesome;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.Gui.ViewModels;

public sealed class ToolViewModel : INotifyPropertyChanged
{
    public string CurrentFileName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string DetectedFormat
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// 导入文件的原始格式（ChartType 枚举值）
    /// </summary>
    public ChartType SourceChartType
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public List<ToolOption> Tools { get; } =
    [
        new()
        {
            Name = tool_unbind_name,
            Description = tool_unbind_desc,
            IconGlyph = Unbind,
            ToolId = "unbind",
            HasPrecision = true,
            HasTolerance = true,
            HasClassicMode = true,
            HasDisableCompress = true
        },

        new()
        {
            Name = tool_layermerge_name,
            Description = tool_layermerge_desc,
            IconGlyph = Merge,
            ToolId = "layermerge",
            HasPrecision = true,
            HasTolerance = true,
            HasClassicMode = true,
            HasDisableCompress = true
        },

        new()
        {
            Name = tool_cut_name,
            Description = tool_cut_desc,
            IconGlyph = Cut,
            ToolId = "cut",
            HasPrecision = true,
            HasTolerance = true,
            HasDisableCompress = true
        },

        new()
        {
            Name = tool_fit_name,
            Description = tool_fit_desc,
            IconGlyph = Fix,
            ToolId = "fit",
            HasTolerance = true,
            HasFitOptions = true,
            DefaultTolerance = 0.5
        },

        new()
        {
            Name = tool_render_name,
            Description = tool_render_desc,
            IconGlyph = Image,
            ToolId = "render",
            HasRenderOptions = true
        }
    ];

    public ToolOption? SelectedTool
    {
        get;
        set
        {
            field = value;
            if (value != null)
            {
                Precision = value.DefaultPrecision;
                Tolerance = value.DefaultTolerance;
                ClassicMode = false;
                DisableCompress = false;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowPrecision));
            OnPropertyChanged(nameof(ShowTolerance));
            OnPropertyChanged(nameof(ShowClassicMode));
            OnPropertyChanged(nameof(ShowDisableCompress));
            OnPropertyChanged(nameof(ShowRenderOptions));
            OnPropertyChanged(nameof(ShowFitOptions));
            OnPropertyChanged(nameof(CanRun));
        }
    }

    public double Precision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64;

    public double Tolerance
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.1;

    public bool ClassicMode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool DisableCompress
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int PixelsPerBeat
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 100;

    public int ChannelWidth
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 150;

    public int SamplesPerEvent
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64;

    public int BeatSubdivisions
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 2;

    public double SegmentPenalty
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 1.0;

    public double KeepOriginalPenalty
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 1.02;

    public int FullSearchRunLengthThreshold
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 160;

    public int LongRunSearchWindow
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 160;

    public double PhaseDetectionEpsilon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0.015;

    public string StatusText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public bool IsProcessing
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRun));
        }
    }

    public bool ShowPrecision => SelectedTool?.HasPrecision == true;
    public bool ShowTolerance => SelectedTool?.HasTolerance == true;
    public bool ShowClassicMode => SelectedTool?.HasClassicMode == true;
    public bool ShowDisableCompress => SelectedTool?.HasDisableCompress == true;
    public bool ShowRenderOptions => SelectedTool?.HasRenderOptions == true;
    public bool ShowFitOptions => SelectedTool?.HasFitOptions == true;

    public bool CanRun => SelectedTool != null && !IsProcessing;

    public event Action? RequestRun;
    public event Action? RequestExport;
    public event Action? RequestSettings;
    public event Action? RequestReturnToImport;

    public void OnRunClicked() => RequestRun?.Invoke();
    public void OnExportClicked() => RequestExport?.Invoke();
    public void OnSettingsClicked() => RequestSettings?.Invoke();
    public void OnReturnToImportClicked() => RequestReturnToImport?.Invoke();

    public void ApplyConfigDefaults(GuiAppConfig config)
    {
        if (SelectedTool == null) return;

        switch (SelectedTool.ToolId)
        {
            case "unbind":
                Precision = config.Unbind.Precision;
                Tolerance = config.Unbind.Tolerance;
                ClassicMode = config.Unbind.ClassicMode;
                DisableCompress = config.Unbind.DisableCompress;
                break;
            case "layermerge":
                Precision = config.LayerMerge.Precision;
                Tolerance = config.LayerMerge.Tolerance;
                ClassicMode = config.LayerMerge.ClassicMode;
                DisableCompress = config.LayerMerge.DisableCompress;
                break;
            case "cut":
                Precision = config.Cut.Precision;
                Tolerance = config.Cut.Tolerance;
                DisableCompress = config.Cut.DisableCompress;
                break;
            case "fit":
                Tolerance = config.Fit.Tolerance;
                SegmentPenalty = config.Fit.SegmentPenalty;
                KeepOriginalPenalty = config.Fit.KeepOriginalPenalty;
                FullSearchRunLengthThreshold = config.Fit.FullSearchRunLengthThreshold;
                LongRunSearchWindow = config.Fit.LongRunSearchWindow;
                PhaseDetectionEpsilon = config.Fit.PhaseDetectionEpsilon;
                break;
            case "render":
                PixelsPerBeat = config.Render.PixelsPerBeat;
                ChannelWidth = config.Render.ChannelWidth;
                SamplesPerEvent = config.Render.SamplesPerEvent;
                BeatSubdivisions = config.Render.BeatSubdivisions;
                break;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
