using PhiChainEasing = KaedePhi.Core.PhiChain.v6.Easing;
using PhiChainEasingKind = KaedePhi.Core.PhiChain.v6.EasingKind;

namespace KaedePhi.Tool.Converter.PhiChain.Utils;

/// <summary>
/// PhiChain 与 KPC 缓动类型之间的映射与转换工具。
/// </summary>
public static class EasingConverter
{
    /// <summary>
    /// KPC 缓动在 PhiChain 中无对应项时抛出，用于触发切段拟合。
    /// </summary>
    public sealed class EasingNotSupportedException(PhiChainEasingKind easingKind)
        : Exception(
            $"PhiChain easing {easingKind} is unsupported in KPC and requires linear slicing"
        )
    {
        public PhiChainEasingKind EasingKind { get; } = easingKind;
    }

    /// <summary>
    /// 将 PhiChain 缓动转换为 KPC 缓动编号。
    /// </summary>
    /// <param name="src">PhiChain 缓动实例</param>
    /// <returns>KPC 缓动编号，不支持的类型抛出异常</returns>
    public static int ConvertToKpcEasingNumber(PhiChainEasing src)
    {
        return src.EasingType switch
        {
            PhiChainEasingKind.Linear => 1,
            PhiChainEasingKind.EaseInSine => 2,
            PhiChainEasingKind.EaseOutSine => 3,
            PhiChainEasingKind.EaseInOutSine => 4,
            PhiChainEasingKind.EaseInQuad => 5,
            PhiChainEasingKind.EaseOutQuad => 6,
            PhiChainEasingKind.EaseInOutQuad => 7,
            PhiChainEasingKind.EaseInCubic => 8,
            PhiChainEasingKind.EaseOutCubic => 9,
            PhiChainEasingKind.EaseInOutCubic => 10,
            PhiChainEasingKind.EaseInQuart => 11,
            PhiChainEasingKind.EaseOutQuart => 12,
            PhiChainEasingKind.EaseInOutQuart => 13,
            PhiChainEasingKind.EaseInQuint => 14,
            PhiChainEasingKind.EaseOutQuint => 15,
            PhiChainEasingKind.EaseInOutQuint => 16,
            PhiChainEasingKind.EaseInExpo => 17,
            PhiChainEasingKind.EaseOutExpo => 18,
            PhiChainEasingKind.EaseInOutExpo => 19,
            PhiChainEasingKind.EaseInCirc => 20,
            PhiChainEasingKind.EaseOutCirc => 21,
            PhiChainEasingKind.EaseInOutCirc => 22,
            PhiChainEasingKind.EaseInBack => 23,
            PhiChainEasingKind.EaseOutBack => 24,
            PhiChainEasingKind.EaseInOutBack => 25,
            PhiChainEasingKind.EaseInElastic => 26,
            PhiChainEasingKind.EaseOutElastic => 27,
            PhiChainEasingKind.EaseInOutElastic => 28,
            PhiChainEasingKind.EaseInBounce => 29,
            PhiChainEasingKind.EaseOutBounce => 30,
            PhiChainEasingKind.EaseInOutBounce => 31,
            // Steps 和自定义 Elastic 不支持，需要切段处理
            PhiChainEasingKind.Steps => throw new EasingNotSupportedException(
                PhiChainEasingKind.Steps
            ),
            PhiChainEasingKind.Elastic => throw new EasingNotSupportedException(
                PhiChainEasingKind.Elastic
            ),
            // Custom 贝塞尔曲线由调用方处理
            PhiChainEasingKind.Custom => 1,
            _ => 1,
        };
    }

    /// <summary>
    /// 将 KPC 缓动编号转换为 PhiChain 缓动。
    /// </summary>
    /// <param name="kpcEasingNumber">KPC 缓动编号</param>
    /// <returns>PhiChain 缓动实例</returns>
    public static PhiChainEasing ConvertFromKpcEasingNumber(int kpcEasingNumber)
    {
        var kind = kpcEasingNumber switch
        {
            1 => PhiChainEasingKind.Linear,
            2 => PhiChainEasingKind.EaseInSine,
            3 => PhiChainEasingKind.EaseOutSine,
            4 => PhiChainEasingKind.EaseInOutSine,
            5 => PhiChainEasingKind.EaseInQuad,
            6 => PhiChainEasingKind.EaseOutQuad,
            7 => PhiChainEasingKind.EaseInOutQuad,
            8 => PhiChainEasingKind.EaseInCubic,
            9 => PhiChainEasingKind.EaseOutCubic,
            10 => PhiChainEasingKind.EaseInOutCubic,
            11 => PhiChainEasingKind.EaseInQuart,
            12 => PhiChainEasingKind.EaseOutQuart,
            13 => PhiChainEasingKind.EaseInOutQuart,
            14 => PhiChainEasingKind.EaseInQuint,
            15 => PhiChainEasingKind.EaseOutQuint,
            16 => PhiChainEasingKind.EaseInOutQuint,
            17 => PhiChainEasingKind.EaseInExpo,
            18 => PhiChainEasingKind.EaseOutExpo,
            19 => PhiChainEasingKind.EaseInOutExpo,
            20 => PhiChainEasingKind.EaseInCirc,
            21 => PhiChainEasingKind.EaseOutCirc,
            22 => PhiChainEasingKind.EaseInOutCirc,
            23 => PhiChainEasingKind.EaseInBack,
            24 => PhiChainEasingKind.EaseOutBack,
            25 => PhiChainEasingKind.EaseInOutBack,
            26 => PhiChainEasingKind.EaseInElastic,
            27 => PhiChainEasingKind.EaseOutElastic,
            28 => PhiChainEasingKind.EaseInOutElastic,
            29 => PhiChainEasingKind.EaseInBounce,
            30 => PhiChainEasingKind.EaseOutBounce,
            31 => PhiChainEasingKind.EaseInOutBounce,
            _ => PhiChainEasingKind.Linear,
        };
        return new PhiChainEasing { EasingType = kind };
    }

    /// <summary>
    /// 将 KPC 缓动转换为 PhiChain 缓动；贝塞尔事件转为线性。
    /// </summary>
    /// <param name="src">KPC 缓动实例</param>
    /// <param name="isBezier">是否为贝塞尔事件</param>
    /// <returns>PhiChain 缓动实例</returns>
    public static PhiChainEasing ConvertEasing(Kpc.Easing src, bool isBezier)
    {
        return isBezier
            ? new PhiChainEasing { EasingType = PhiChainEasingKind.Linear }
            : ConvertFromKpcEasingNumber((int)src);
    }

    /// <summary>
    /// 将 PhiChain 缓动转换为 KPC 缓动。
    /// </summary>
    /// <param name="src">PhiChain 缓动实例</param>
    /// <returns>KPC 缓动实例</returns>
    public static Kpc.Easing ConvertEasing(PhiChainEasing src)
    {
        return new Kpc.Easing(ConvertToKpcEasingNumber(src));
    }

    /// <summary>
    /// 检查 PhiChain 缓动是否需要切段处理。
    /// </summary>
    /// <param name="src">PhiChain 缓动实例</param>
    /// <returns>如果需要切段返回 true</returns>
    public static bool NeedsLinearSlicing(PhiChainEasing src)
    {
        return src.EasingType is PhiChainEasingKind.Steps or PhiChainEasingKind.Elastic;
    }
}
