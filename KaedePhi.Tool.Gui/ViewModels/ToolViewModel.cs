using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.Gui.Models;
using static KaedePhi.Tool.Gui.Models.FontAwesome;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.Gui.ViewModels;

public sealed class ToolViewModel : INotifyPropertyChanged
{
    private string _currentFileName = string.Empty;
    private string _detectedFormat = string.Empty;
    private ToolOption? _selectedTool;
    private double _precision = 64;
    private double _tolerance = 0.1;
    private bool _classicMode;
    private bool _disableCompress;
    private int _pixelsPerBeat = 100;
    private int _channelWidth = 150;
    private int _samplesPerEvent = 64;
    private int _beatSubdivisions = 2;
    private string _statusText = string.Empty;
    private bool _isProcessing;

    public string CurrentFileName
    {
        get => _currentFileName;
        set { _currentFileName = value; OnPropertyChanged(); }
    }

    public string DetectedFormat
    {
        get => _detectedFormat;
        set { _detectedFormat = value; OnPropertyChanged(); }
    }

    public List<ToolOption> Tools { get; } = new()
    {
        new ToolOption
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
        new ToolOption
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
        new ToolOption
        {
            Name = tool_cut_name,
            Description = tool_cut_desc,
            IconGlyph = Cut,
            ToolId = "cut",
            HasPrecision = true,
            HasTolerance = true,
            HasDisableCompress = true
        },
        new ToolOption
        {
            Name = tool_fit_name,
            Description = tool_fit_desc,
            IconGlyph = Fix,
            ToolId = "fit",
            HasTolerance = true,
            DefaultTolerance = 0.5
        },
        new ToolOption
        {
            Name = tool_render_name,
            Description = tool_render_desc,
            IconGlyph = Image,
            ToolId = "render",
            HasRenderOptions = true
        }
    };

    public ToolOption? SelectedTool
    {
        get => _selectedTool;
        set
        {
            _selectedTool = value;
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
        }
    }

    public double Precision
    {
        get => _precision;
        set { _precision = value; OnPropertyChanged(); }
    }

    public double Tolerance
    {
        get => _tolerance;
        set { _tolerance = value; OnPropertyChanged(); }
    }

    public bool ClassicMode
    {
        get => _classicMode;
        set { _classicMode = value; OnPropertyChanged(); }
    }

    public bool DisableCompress
    {
        get => _disableCompress;
        set { _disableCompress = value; OnPropertyChanged(); }
    }

    public int PixelsPerBeat
    {
        get => _pixelsPerBeat;
        set { _pixelsPerBeat = value; OnPropertyChanged(); }
    }

    public int ChannelWidth
    {
        get => _channelWidth;
        set { _channelWidth = value; OnPropertyChanged(); }
    }

    public int SamplesPerEvent
    {
        get => _samplesPerEvent;
        set { _samplesPerEvent = value; OnPropertyChanged(); }
    }

    public int BeatSubdivisions
    {
        get => _beatSubdivisions;
        set { _beatSubdivisions = value; OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set { _isProcessing = value; OnPropertyChanged(); }
    }

    public bool ShowPrecision => SelectedTool?.HasPrecision == true;
    public bool ShowTolerance => SelectedTool?.HasTolerance == true;
    public bool ShowClassicMode => SelectedTool?.HasClassicMode == true;
    public bool ShowDisableCompress => SelectedTool?.HasDisableCompress == true;
    public bool ShowRenderOptions => SelectedTool?.HasRenderOptions == true;

    public event Action? RequestRun;
    public event Action? RequestExport;
    public event Action? RequestSettings;

    public void OnRunClicked() => RequestRun?.Invoke();
    public void OnExportClicked() => RequestExport?.Invoke();
    public void OnSettingsClicked() => RequestSettings?.Invoke();

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
