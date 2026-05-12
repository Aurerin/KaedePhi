namespace KaedePhi.Tool.Cli.Model;

/// <summary>
/// 事件拟合命令默认配置
/// </summary>
public class FitConfig
{
    /// <summary>
    /// 拟合容差
    /// </summary>
    public double Tolerance { get; set; } = 0.5d;

    /// <summary>
    /// 是否为干运行（仅计算不写入）
    /// </summary>
    public bool DryRun { get; set; } = false;

    // ---- EventFit 算法参数 ----

    /// <summary>
    /// DP 中每增加一个段的惩罚；越大越倾向于更少事件
    /// </summary>
    public double SegmentPenalty { get; set; } = 1.0d;

    /// <summary>
    /// 保留原事件的轻微偏置，避免在边界条件下过拟合复杂缓动
    /// </summary>
    public double KeepOriginalPenalty { get; set; } = 1.02d;

    /// <summary>
    /// 长段时限制回看窗口的长度阈值。超过此值时启用窗口限制
    /// </summary>
    public int FullSearchRunLengthThreshold { get; set; } = 160;

    /// <summary>
    /// 长段时的回看窗口大小
    /// </summary>
    public int LongRunSearchWindow { get; set; } = 160;

    /// <summary>
    /// 相位检测的 epsilon 阈值，用于判断缓动曲线的相位类型
    /// </summary>
    public double PhaseDetectionEpsilon { get; set; } = 0.015d;
}