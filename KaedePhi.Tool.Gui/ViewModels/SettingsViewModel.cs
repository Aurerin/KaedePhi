using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.Gui.Models;
using KaedePhi.Tool.Gui.Services;

namespace KaedePhi.Tool.Gui.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ConfigService _config;
    private string _statusText = string.Empty;

    public SettingsViewModel(ConfigService config)
    {
        _config = config;
        LoadFromConfig();
    }

    public int MaxLogFiles { get; set; } = 5;

    public double UnbindPrecision { get; set; } = 64;
    public double UnbindTolerance { get; set; } = 0.1;
    public bool UnbindClassicMode { get; set; }
    public bool UnbindDisableCompress { get; set; }

    public double LayerMergePrecision { get; set; } = 64;
    public double LayerMergeTolerance { get; set; } = 0.1;
    public bool LayerMergeClassicMode { get; set; }
    public bool LayerMergeDisableCompress { get; set; }

    public double CutPrecision { get; set; } = 64;
    public double CutTolerance { get; set; } = 0.1;
    public bool CutDisableCompress { get; set; }

    public double FitTolerance { get; set; } = 0.5;

    public int RenderPixelsPerBeat { get; set; } = 100;
    public int RenderChannelWidth { get; set; } = 150;
    public int RenderSamplesPerEvent { get; set; } = 64;
    public int RenderBeatSubdivisions { get; set; } = 2;

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public event Action? RequestReturnToTools;

    public void OnReturnClicked() => RequestReturnToTools?.Invoke();

    public void OnSaveClicked()
    {
        var c = _config.Config;
        c.MaxLogFiles = MaxLogFiles;

        c.Unbind.Precision = UnbindPrecision;
        c.Unbind.Tolerance = UnbindTolerance;
        c.Unbind.ClassicMode = UnbindClassicMode;
        c.Unbind.DisableCompress = UnbindDisableCompress;

        c.LayerMerge.Precision = LayerMergePrecision;
        c.LayerMerge.Tolerance = LayerMergeTolerance;
        c.LayerMerge.ClassicMode = LayerMergeClassicMode;
        c.LayerMerge.DisableCompress = LayerMergeDisableCompress;

        c.Cut.Precision = CutPrecision;
        c.Cut.Tolerance = CutTolerance;
        c.Cut.DisableCompress = CutDisableCompress;

        c.Fit.Tolerance = FitTolerance;

        c.Render.PixelsPerBeat = RenderPixelsPerBeat;
        c.Render.ChannelWidth = RenderChannelWidth;
        c.Render.SamplesPerEvent = RenderSamplesPerEvent;
        c.Render.BeatSubdivisions = RenderBeatSubdivisions;

        _config.Save();
        StatusText = "配置已保存。";
    }

    public void OnResetClicked()
    {
        var defaults = new GuiAppConfig();
        _config.Config = defaults;
        _config.Save();
        LoadFromConfig();
        OnPropertyChanged(string.Empty);
        StatusText = "已恢复默认设置。";
    }

    private void LoadFromConfig()
    {
        var c = _config.Config;
        MaxLogFiles = c.MaxLogFiles;

        UnbindPrecision = c.Unbind.Precision;
        UnbindTolerance = c.Unbind.Tolerance;
        UnbindClassicMode = c.Unbind.ClassicMode;
        UnbindDisableCompress = c.Unbind.DisableCompress;

        LayerMergePrecision = c.LayerMerge.Precision;
        LayerMergeTolerance = c.LayerMerge.Tolerance;
        LayerMergeClassicMode = c.LayerMerge.ClassicMode;
        LayerMergeDisableCompress = c.LayerMerge.DisableCompress;

        CutPrecision = c.Cut.Precision;
        CutTolerance = c.Cut.Tolerance;
        CutDisableCompress = c.Cut.DisableCompress;

        FitTolerance = c.Fit.Tolerance;

        RenderPixelsPerBeat = c.Render.PixelsPerBeat;
        RenderChannelWidth = c.Render.ChannelWidth;
        RenderSamplesPerEvent = c.Render.SamplesPerEvent;
        RenderBeatSubdivisions = c.Render.BeatSubdivisions;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
