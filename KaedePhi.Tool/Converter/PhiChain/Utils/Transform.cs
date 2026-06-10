using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter.PhiChain.Utils;

/// <summary>
/// PhiChain 坐标系与 KPC 坐标系之间的坐标变换工具。
/// </summary>
public static class Transform
{
    private static readonly CoordinateProfile PhichainProfile = CoordinateProfile.PhichainProfile;

    /// <summary>
    /// 将 PhiChain X 坐标转换为 KPC X 坐标。
    /// </summary>
    public static double TransformToKpcX(float x) => CoordinateGeometry.ToKpcX(x, PhichainProfile);

    /// <summary>
    /// 将 PhiChain Y 坐标转换为 KPC Y 坐标。
    /// </summary>
    public static double TransformToKpcY(float y) => CoordinateGeometry.ToKpcY(y, PhichainProfile);

    /// <summary>
    /// 将 PhiChain 角度转换为 KPC 角度。
    /// </summary>
    public static double TransformToKpcAngle(float angle) => CoordinateGeometry.ToKpcAngle(angle, PhichainProfile);

    /// <summary>
    /// 将 KPC X 坐标转换为 PhiChain X 坐标。
    /// </summary>
    public static float TransformToPhichainX(double x) => CoordinateGeometry.ToTargetXf(x, PhichainProfile);

    /// <summary>
    /// 将 KPC Y 坐标转换为 PhiChain Y 坐标。
    /// </summary>
    public static float TransformToPhichainY(double y) => CoordinateGeometry.ToTargetYf(y, PhichainProfile);

    /// <summary>
    /// 将 KPC 角度转换为 PhiChain 角度。
    /// </summary>
    public static double TransformToPhichainAngle(double angle) => CoordinateGeometry.ToTargetAngle(angle, PhichainProfile);
}
