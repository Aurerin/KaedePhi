using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Gui.ViewModels;

public sealed class ImportOptionsViewModel : INotifyPropertyChanged
{
    private ChartType _detectedFormat;
    private string _fileName = string.Empty;
    private bool _isLoading;

    /// <summary>
    /// 检测到的源文件格式
    /// </summary>
    public ChartType DetectedFormat
    {
        get => _detectedFormat;
        set
        {
            _detectedFormat = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowPeOptions));
            OnPropertyChanged(nameof(ShowPhiChainOptions));
            OnPropertyChanged(nameof(FormatName));
        }
    }

    /// <summary>
    /// 源文件名
    /// </summary>
    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否正在加载
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 格式名称（用于显示）
    /// </summary>
    public string FormatName => _detectedFormat.ToString();

    /// <summary>
    /// 是否显示 PhiEdit 导入选项
    /// </summary>
    public bool ShowPeOptions => _detectedFormat == ChartType.PhiEdit;

    /// <summary>
    /// 是否显示 PhiChain 导入选项
    /// </summary>
    public bool ShowPhiChainOptions => _detectedFormat == ChartType.PhiChain;

    #region PhiEdit 导入选项

    /// <summary>
    /// PE 帧转事件后持续拍长度
    /// </summary>
    public double PeFrameDurationBeat
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 1d / 64d;

    /// <summary>
    /// PE 速度帧值到 KPC 速度事件值的转换比率
    /// </summary>
    public double PeSpeedConversionRatio
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 14d / 9d;

    /// <summary>
    /// PE 尾部拍填充量
    /// </summary>
    public double PeTrailingBeatPadding
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 1d / 64d;

    #endregion

    #region PhiChain 导入选项

    /// <summary>
    /// PhiChain 不支持的缓动切段精度
    /// </summary>
    public int PhiChainUnsupportedEasingPrecision
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 64;

    #endregion

    public event Action? RequestConfirm;
    public event Action? RequestCancel;

    public void OnConfirmClicked() => RequestConfirm?.Invoke();
    public void OnCancelClicked() => RequestCancel?.Invoke();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
