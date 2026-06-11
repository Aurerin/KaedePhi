using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter.PhiChain.Utils;

/// <summary>
/// PhiChain 坐标系与 KPC 坐标系之间的坐标变换工具。
/// </summary>
public static class Transform
{
    private static readonly CoordinateProfile PhiChainProfile = CoordinateProfile.PhiChainProfile;

    /// <summary>
    /// 将 PhiChain X 坐标转换为 KPC X 坐标。
    /// </summary>
    public static double TransformToKpcX(float x) => CoordinateGeometry.ToKpcX(x, PhiChainProfile);

    /// <summary>
    /// 将 PhiChain Y 坐标转换为 KPC Y 坐标。
    /// </summary>
    public static double TransformToKpcY(float y) => CoordinateGeometry.ToKpcY(y, PhiChainProfile);

    /// <summary>
    /// 将 PhiChain 角度转换为 KPC 角度。
    /// </summary>
    public static double TransformToKpcAngle(float angle) => CoordinateGeometry.ToKpcAngle(angle, PhiChainProfile);

    /// <summary>
    /// 将 KPC X 坐标转换为 PhiChain X 坐标。
    /// </summary>
    public static float TransformToPhiChainX(double x) => CoordinateGeometry.ToTargetXf(x, PhiChainProfile);

    /// <summary>
    /// 将 KPC Y 坐标转换为 PhiChain Y 坐标。
    /// </summary>
    public static float TransformToPhiChainY(double y) => CoordinateGeometry.ToTargetYf(y, PhiChainProfile);

    /// <summary>
    /// 将 KPC 角度转换为 PhiChain 角度。
    /// </summary>
    public static double TransformToPhiChainAngle(double angle) => CoordinateGeometry.ToTargetAngle(angle, PhiChainProfile);
}
