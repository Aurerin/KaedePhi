namespace KaedePhi.Tool.Common;

/// <summary>
/// 工具层共享常量。
/// </summary>
public static class Constants
{
    /// <summary>浮点比较容差</summary>
    public const float FloatEpsilon = 1e-6f;

    /// <summary>Phigros 速度值转换比例</summary>
    public const double SpeedValueRatio = 4.5d;

    /// <summary>Note X 位置比例系数</summary>
    public const float NotePositionXRatio = 0.1125f;

    /// <summary>默认采样精度</summary>
    public const int DefaultPrecision = 64;

    /// <summary>默认容差百分比</summary>
    public const double DefaultTolerancePercent = 0.1d;
}
