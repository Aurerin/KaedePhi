using KaedePhi.Core.PhiChain.v6;
using PhichainEasing = KaedePhi.Core.PhiChain.v6.Easing;
using PhichainEasingKind = KaedePhi.Core.PhiChain.v6.EasingKind;

namespace KaedePhi.Tool.Converter.PhiChain.Utils;

/// <summary>
/// PhiChain 与 KPC 缓动类型之间的映射与转换工具。
/// </summary>
public static class EasingConverter
{
    /// <summary>
    /// KPC 缓动在 PhiChain 中无对应项时抛出，用于触发切段拟合。
    /// </summary>
    public sealed class EasingNotSupportedException(PhichainEasingKind easingKind)
        : Exception($"PhiChain easing {easingKind} is unsupported in KPC and requires linear slicing")
    {
        public PhichainEasingKind EasingKind { get; } = easingKind;
    }

    /// <summary>
    /// 将 PhiChain 缓动转换为 KPC 缓动编号。
    /// </summary>
    /// <param name="src">PhiChain 缓动实例</param>
    /// <returns>KPC 缓动编号，不支持的类型抛出异常</returns>
    public static int ConvertToKpcEasingNumber(PhichainEasing src)
    {
        return src.EasingType switch
        {
            PhichainEasingKind.Linear => 1,
            PhichainEasingKind.EaseInSine => 2,
            PhichainEasingKind.EaseOutSine => 3,
            PhichainEasingKind.EaseInOutSine => 4,
            PhichainEasingKind.EaseInQuad => 5,
            PhichainEasingKind.EaseOutQuad => 6,
            PhichainEasingKind.EaseInOutQuad => 7,
            PhichainEasingKind.EaseInCubic => 8,
            PhichainEasingKind.EaseOutCubic => 9,
            PhichainEasingKind.EaseInOutCubic => 10,
            PhichainEasingKind.EaseInQuart => 11,
            PhichainEasingKind.EaseOutQuart => 12,
            PhichainEasingKind.EaseInOutQuart => 13,
            PhichainEasingKind.EaseInQuint => 14,
            PhichainEasingKind.EaseOutQuint => 15,
            PhichainEasingKind.EaseInOutQuint => 16,
            PhichainEasingKind.EaseInExpo => 17,
            PhichainEasingKind.EaseOutExpo => 18,
            PhichainEasingKind.EaseInOutExpo => 19,
            PhichainEasingKind.EaseInCirc => 20,
            PhichainEasingKind.EaseOutCirc => 21,
            PhichainEasingKind.EaseInOutCirc => 22,
            PhichainEasingKind.EaseInBack => 23,
            PhichainEasingKind.EaseOutBack => 24,
            PhichainEasingKind.EaseInOutBack => 25,
            PhichainEasingKind.EaseInElastic => 26,
            PhichainEasingKind.EaseOutElastic => 27,
            PhichainEasingKind.EaseInOutElastic => 28,
            PhichainEasingKind.EaseInBounce => 29,
            PhichainEasingKind.EaseOutBounce => 30,
            PhichainEasingKind.EaseInOutBounce => 31,
            // Steps 和自定义 Elastic 不支持，需要切段处理
            PhichainEasingKind.Steps => throw new EasingNotSupportedException(PhichainEasingKind.Steps),
            PhichainEasingKind.Elastic => throw new EasingNotSupportedException(PhichainEasingKind.Elastic),
            // Custom 贝塞尔曲线由调用方处理
            PhichainEasingKind.Custom => 1,
            _ => 1,
        };
    }

    /// <summary>
    /// 将 KPC 缓动编号转换为 PhiChain 缓动。
    /// </summary>
    /// <param name="kpcEasingNumber">KPC 缓动编号</param>
    /// <returns>PhiChain 缓动实例</returns>
    public static PhichainEasing ConvertFromKpcEasingNumber(int kpcEasingNumber)
    {
        var kind = kpcEasingNumber switch
        {
            1 => PhichainEasingKind.Linear,
            2 => PhichainEasingKind.EaseInSine,
            3 => PhichainEasingKind.EaseOutSine,
            4 => PhichainEasingKind.EaseInOutSine,
            5 => PhichainEasingKind.EaseInQuad,
            6 => PhichainEasingKind.EaseOutQuad,
            7 => PhichainEasingKind.EaseInOutQuad,
            8 => PhichainEasingKind.EaseInCubic,
            9 => PhichainEasingKind.EaseOutCubic,
            10 => PhichainEasingKind.EaseInOutCubic,
            11 => PhichainEasingKind.EaseInQuart,
            12 => PhichainEasingKind.EaseOutQuart,
            13 => PhichainEasingKind.EaseInOutQuart,
            14 => PhichainEasingKind.EaseInQuint,
            15 => PhichainEasingKind.EaseOutQuint,
            16 => PhichainEasingKind.EaseInOutQuint,
            17 => PhichainEasingKind.EaseInExpo,
            18 => PhichainEasingKind.EaseOutExpo,
            19 => PhichainEasingKind.EaseInOutExpo,
            20 => PhichainEasingKind.EaseInCirc,
            21 => PhichainEasingKind.EaseOutCirc,
            22 => PhichainEasingKind.EaseInOutCirc,
            23 => PhichainEasingKind.EaseInBack,
            24 => PhichainEasingKind.EaseOutBack,
            25 => PhichainEasingKind.EaseInOutBack,
            26 => PhichainEasingKind.EaseInElastic,
            27 => PhichainEasingKind.EaseOutElastic,
            28 => PhichainEasingKind.EaseInOutElastic,
            29 => PhichainEasingKind.EaseInBounce,
            30 => PhichainEasingKind.EaseOutBounce,
            31 => PhichainEasingKind.EaseInOutBounce,
            _ => PhichainEasingKind.Linear,
        };
        return new PhichainEasing { EasingType = kind };
    }

    /// <summary>
    /// 将 KPC 缓动转换为 PhiChain 缓动；贝塞尔事件转为线性。
    /// </summary>
    /// <param name="src">KPC 缓动实例</param>
    /// <param name="isBezier">是否为贝塞尔事件</param>
    /// <returns>PhiChain 缓动实例</returns>
    public static PhichainEasing ConvertEasing(Kpc.Easing src, bool isBezier)
    {
        return isBezier
            ? new PhichainEasing { EasingType = PhichainEasingKind.Linear }
            : ConvertFromKpcEasingNumber((int)src);
    }

    /// <summary>
    /// 将 PhiChain 缓动转换为 KPC 缓动。
    /// </summary>
    /// <param name="src">PhiChain 缓动实例</param>
    /// <returns>KPC 缓动实例</returns>
    public static Kpc.Easing ConvertEasing(PhichainEasing src)
    {
        return new Kpc.Easing(ConvertToKpcEasingNumber(src));
    }

    /// <summary>
    /// 检查 PhiChain 缓动是否需要切段处理。
    /// </summary>
    /// <param name="src">PhiChain 缓动实例</param>
    /// <returns>如果需要切段返回 true</returns>
    public static bool NeedsLinearSlicing(PhichainEasing src)
    {
        return src.EasingType is PhichainEasingKind.Steps or PhichainEasingKind.Elastic;
    }
}
