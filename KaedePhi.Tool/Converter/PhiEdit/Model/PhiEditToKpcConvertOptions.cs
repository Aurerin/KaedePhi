namespace KaedePhi.Tool.Converter.PhiEdit.Model;

public class PhiEditToKpcConvertOptions
{
    /// <summary>
    /// PhiEdit帧转事件后持续拍长度
    /// </summary>
    public double FrameDurationBeat { get; set; } = 1 / 64d;

    /// <summary>
    /// PE 速度帧值到 KPC 速度事件值的转换比率。
    /// KPC 速度值 = PE 速度值 / SpeedConversionRatio。
    /// </summary>
    public double SpeedConversionRatio { get; set; } = 14d / 9d;

    /// <summary>
    /// 尾部拍填充量（拍），用于确保事件覆盖到判定线时间范围末端。
    /// </summary>
    public double TrailingBeatPadding { get; set; } = 1d / 64d;
}