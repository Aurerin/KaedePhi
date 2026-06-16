namespace KaedePhi.Tool.Cli.Model;

/// <summary>
/// 事件拟合命令默认配置
/// </summary>
public class FitConfig
{
    /// <summary>
    /// 容差百分比，取值范围 [0, 100]。越小越精确，越大越宽松，默认值 0.1（即 0.1%）。
    /// </summary>
    public double Tolerance { get; set; } = 0.1d;

    /// <summary>
    /// 是否为干运行（仅计算不写入）
    /// </summary>
    public bool DryRun { get; set; } = false;
}
