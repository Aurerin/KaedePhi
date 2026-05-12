using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.Gui.Models;
using KaedePhi.Tool.Gui.Services;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

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

    // ---- Unbind ----
    public double UnbindPrecision { get; set; } = 64;
    public double UnbindTolerance { get; set; } = 0.1;
    public bool UnbindClassicMode { get; set; }
    public bool UnbindDisableCompress { get; set; }

    // ---- LayerMerge ----
    public double LayerMergePrecision { get; set; } = 64;
    public double LayerMergeTolerance { get; set; } = 0.1;
    public bool LayerMergeClassicMode { get; set; }
    public bool LayerMergeDisableCompress { get; set; }

    // ---- Cut ----
    public double CutPrecision { get; set; } = 64;
    public double CutTolerance { get; set; } = 0.1;
    public bool CutDisableCompress { get; set; }

    // ---- Fit ----
    public double FitTolerance { get; set; } = 0.5;
    public double FitSegmentPenalty { get; set; } = 1.0;
    public double FitKeepOriginalPenalty { get; set; } = 1.02;
    public int FitFullSearchRunLengthThreshold { get; set; } = 160;
    public int FitLongRunSearchWindow { get; set; } = 160;
    public double FitPhaseDetectionEpsilon { get; set; } = 0.015;

    // ---- Render ----
    public int RenderPixelsPerBeat { get; set; } = 100;
    public int RenderChannelWidth { get; set; } = 150;
    public int RenderSamplesPerEvent { get; set; } = 64;
    public int RenderBeatSubdivisions { get; set; } = 2;
    public double RenderRangePaddingRatio { get; set; } = 0.10;
    public int RenderRangeSamplesPerEvent { get; set; } = 16;
    public double RenderSegmentGroupTolerance { get; set; } = 1e-6;
    public double RenderMinValueRangeHalf { get; set; } = 0.1;
    public double RenderMinValueRangeHalfRatio { get; set; } = 0.15;

    // ---- Convert (PE) ----
    public double ConvertPeSpeedConversionRatio { get; set; } = 14d / 9d;
    public double ConvertPeTrailingBeatPadding { get; set; } = 1d / 64d;
    public double ConvertPeUnsupportedEasingPrecision { get; set; } = 64d;
    public double ConvertPeMisalignedXyEventPrecision { get; set; } = 64d;
    public double ConvertPeAlphaCutPrecision { get; set; } = 64d;
    public double ConvertPeAlphaCutTolerance { get; set; } = 0.1d;
    public double ConvertPeSpeedCutPrecision { get; set; } = 64d;
    public double ConvertPeSpeedCutTolerance { get; set; } = 0.1d;

    // ---- Convert (PhigrosV3) ----
    public float ConvertPhigrosDefaultBpm { get; set; } = 120f;
    public double ConvertPhigrosEasingPrecision { get; set; } = 64d;
    public double ConvertPhigrosMisalignedXyEventPrecision { get; set; } = 64d;
    public double ConvertPhigrosAlphaCutPrecision { get; set; } = 64d;
    public double ConvertPhigrosAlphaCutTolerance { get; set; } = 0.1d;
    public double ConvertPhigrosSpeedCutPrecision { get; set; } = 64d;

    // ---- Convert (Common) ----
    public double ConvertUnbindPrecision { get; set; } = 64d;
    public double ConvertUnbindTolerance { get; set; } = 0.1d;
    public bool ConvertUnbindClassicMode { get; set; }
    public double ConvertMultiLayerMergePrecision { get; set; } = 64d;
    public double ConvertMultiLayerMergeTolerance { get; set; } = 0.1d;
    public bool ConvertMultiLayerMergeClassicMode { get; set; }

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
        c.Fit.SegmentPenalty = FitSegmentPenalty;
        c.Fit.KeepOriginalPenalty = FitKeepOriginalPenalty;
        c.Fit.FullSearchRunLengthThreshold = FitFullSearchRunLengthThreshold;
        c.Fit.LongRunSearchWindow = FitLongRunSearchWindow;
        c.Fit.PhaseDetectionEpsilon = FitPhaseDetectionEpsilon;

        c.Render.PixelsPerBeat = RenderPixelsPerBeat;
        c.Render.ChannelWidth = RenderChannelWidth;
        c.Render.SamplesPerEvent = RenderSamplesPerEvent;
        c.Render.BeatSubdivisions = RenderBeatSubdivisions;
        c.Render.RangePaddingRatio = RenderRangePaddingRatio;
        c.Render.RangeSamplesPerEvent = RenderRangeSamplesPerEvent;
        c.Render.SegmentGroupTolerance = RenderSegmentGroupTolerance;
        c.Render.MinValueRangeHalf = RenderMinValueRangeHalf;
        c.Render.MinValueRangeHalfRatio = RenderMinValueRangeHalfRatio;

        c.Convert.PeSpeedConversionRatio = ConvertPeSpeedConversionRatio;
        c.Convert.PeTrailingBeatPadding = ConvertPeTrailingBeatPadding;
        c.Convert.PeUnsupportedEasingPrecision = ConvertPeUnsupportedEasingPrecision;
        c.Convert.PeMisalignedXyEventPrecision = ConvertPeMisalignedXyEventPrecision;
        c.Convert.PeAlphaCutPrecision = ConvertPeAlphaCutPrecision;
        c.Convert.PeAlphaCutTolerance = ConvertPeAlphaCutTolerance;
        c.Convert.PeSpeedCutPrecision = ConvertPeSpeedCutPrecision;
        c.Convert.PeSpeedCutTolerance = ConvertPeSpeedCutTolerance;
        c.Convert.PhigrosDefaultBpm = ConvertPhigrosDefaultBpm;
        c.Convert.PhigrosEasingPrecision = ConvertPhigrosEasingPrecision;
        c.Convert.PhigrosMisalignedXyEventPrecision = ConvertPhigrosMisalignedXyEventPrecision;
        c.Convert.PhigrosAlphaCutPrecision = ConvertPhigrosAlphaCutPrecision;
        c.Convert.PhigrosAlphaCutTolerance = ConvertPhigrosAlphaCutTolerance;
        c.Convert.PhigrosSpeedCutPrecision = ConvertPhigrosSpeedCutPrecision;
        c.Convert.UnbindPrecision = ConvertUnbindPrecision;
        c.Convert.UnbindTolerance = ConvertUnbindTolerance;
        c.Convert.UnbindClassicMode = ConvertUnbindClassicMode;
        c.Convert.MultiLayerMergePrecision = ConvertMultiLayerMergePrecision;
        c.Convert.MultiLayerMergeTolerance = ConvertMultiLayerMergeTolerance;
        c.Convert.MultiLayerMergeClassicMode = ConvertMultiLayerMergeClassicMode;

        _config.Save();
        StatusText = settings_saved;
    }

    public void OnResetClicked()
    {
        var defaults = new GuiAppConfig();
        _config.Config = defaults;
        _config.Save();
        LoadFromConfig();
        OnPropertyChanged(string.Empty);
        StatusText = settings_reset;
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
        FitSegmentPenalty = c.Fit.SegmentPenalty;
        FitKeepOriginalPenalty = c.Fit.KeepOriginalPenalty;
        FitFullSearchRunLengthThreshold = c.Fit.FullSearchRunLengthThreshold;
        FitLongRunSearchWindow = c.Fit.LongRunSearchWindow;
        FitPhaseDetectionEpsilon = c.Fit.PhaseDetectionEpsilon;

        RenderPixelsPerBeat = c.Render.PixelsPerBeat;
        RenderChannelWidth = c.Render.ChannelWidth;
        RenderSamplesPerEvent = c.Render.SamplesPerEvent;
        RenderBeatSubdivisions = c.Render.BeatSubdivisions;
        RenderRangePaddingRatio = c.Render.RangePaddingRatio;
        RenderRangeSamplesPerEvent = c.Render.RangeSamplesPerEvent;
        RenderSegmentGroupTolerance = c.Render.SegmentGroupTolerance;
        RenderMinValueRangeHalf = c.Render.MinValueRangeHalf;
        RenderMinValueRangeHalfRatio = c.Render.MinValueRangeHalfRatio;

        ConvertPeSpeedConversionRatio = c.Convert.PeSpeedConversionRatio;
        ConvertPeTrailingBeatPadding = c.Convert.PeTrailingBeatPadding;
        ConvertPeUnsupportedEasingPrecision = c.Convert.PeUnsupportedEasingPrecision;
        ConvertPeMisalignedXyEventPrecision = c.Convert.PeMisalignedXyEventPrecision;
        ConvertPeAlphaCutPrecision = c.Convert.PeAlphaCutPrecision;
        ConvertPeAlphaCutTolerance = c.Convert.PeAlphaCutTolerance;
        ConvertPeSpeedCutPrecision = c.Convert.PeSpeedCutPrecision;
        ConvertPeSpeedCutTolerance = c.Convert.PeSpeedCutTolerance;
        ConvertPhigrosDefaultBpm = c.Convert.PhigrosDefaultBpm;
        ConvertPhigrosEasingPrecision = c.Convert.PhigrosEasingPrecision;
        ConvertPhigrosMisalignedXyEventPrecision = c.Convert.PhigrosMisalignedXyEventPrecision;
        ConvertPhigrosAlphaCutPrecision = c.Convert.PhigrosAlphaCutPrecision;
        ConvertPhigrosAlphaCutTolerance = c.Convert.PhigrosAlphaCutTolerance;
        ConvertPhigrosSpeedCutPrecision = c.Convert.PhigrosSpeedCutPrecision;
        ConvertUnbindPrecision = c.Convert.UnbindPrecision;
        ConvertUnbindTolerance = c.Convert.UnbindTolerance;
        ConvertUnbindClassicMode = c.Convert.UnbindClassicMode;
        ConvertMultiLayerMergePrecision = c.Convert.MultiLayerMergePrecision;
        ConvertMultiLayerMergeTolerance = c.Convert.MultiLayerMergeTolerance;
        ConvertMultiLayerMergeClassicMode = c.Convert.MultiLayerMergeClassicMode;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
