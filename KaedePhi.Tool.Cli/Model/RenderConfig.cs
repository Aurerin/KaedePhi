namespace KaedePhi.Tool.Cli.Model;

/// <summary>
/// 事件通道渲染命令默认配置
/// </summary>
public class RenderConfig
{
    /// <summary>每拍对应像素高度（默认 100）</summary>
    public float PixelsPerBeat { get; set; } = 100f;

    /// <summary>每个通道的宽度（像素，默认 150）</summary>
    public int ChannelWidth { get; set; } = 150;

    /// <summary>每个事件的曲线采样点数（默认 64）</summary>
    public int SamplesPerEvent { get; set; } = 64;

    /// <summary>默认输出目录（为空时使用输入文件所在目录）</summary>
    public string OutputDir { get; set; } = "";

    /// <summary>每拍格线细分数（1=只绘节拍线，4=绘四分音符线，默认 2）</summary>
    public int BeatSubdivisions { get; set; } = 2;

    #region 新增渲染参数

    /// <summary>采样范围填充比例（用于自动计算值域范围的上下边距，默认 0.10）</summary>
    public double RangePaddingRatio { get; set; } = 0.10;

    /// <summary>每个事件用于值域探测的采样点数（默认 16）</summary>
    public int RangeSamplesPerEvent { get; set; } = 16;

    /// <summary>段分组容差（相邻事件端点小于此值时视为连续，默认 1e-6）</summary>
    public double SegmentGroupTolerance { get; set; } = 1e-6;

    /// <summary>最小值域半宽（当值域接近零时的兜底值，默认 0.1）</summary>
    public double MinValueRangeHalf { get; set; } = 0.1;

    /// <summary>最小值域半宽比例（相对于中心值的比例，默认 0.15）</summary>
    public double MinValueRangeHalfRatio { get; set; } = 0.15;

    #endregion
}