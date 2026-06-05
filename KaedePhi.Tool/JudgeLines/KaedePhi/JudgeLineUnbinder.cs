using KaedePhi.Tool.Common;
using KaedePhi.Tool.JudgeLines.KaedePhi.Utils;
using JudgeLine = KaedePhi.Core.KaedePhi.JudgeLine;

namespace KaedePhi.Tool.JudgeLines.KaedePhi;

/// <summary>
/// KPC 谱面判定线父子解绑器。
/// <para>等间隔采样：<see cref="FatherUnbindProcessor"/>；自适应采样：<see cref="FatherUnbindPlusProcessor"/>。</para>
/// </summary>
public class JudgeLineUnbinder : LoggableBase, IJudgeLineUnbinder<JudgeLine>
{
    /// <inheritdoc/>
    public (double X, double Y) GetLinePos(
        double fatherLineX,
        double fatherLineY,
        double angleDegrees,
        double lineX,
        double lineY
    ) => FatherUnbindHelpers.GetLinePos(fatherLineX, fatherLineY, angleDegrees, lineX, lineY);

    /// <inheritdoc/>
    public (double X, double Y) GetLinePos(
        double fatherLineX,
        double fatherLineY,
        double angleDegrees,
        double lineX,
        double lineY,
        CoordinateProfile renderProfile
    )
    {
        using var _ = FatherUnbindHelpers.UseRenderProfile(renderProfile);
        return FatherUnbindHelpers.GetLinePos(fatherLineX, fatherLineY, angleDegrees, lineX, lineY);
    }

    private FatherUnbindProcessor CreateProcessor(List<JudgeLine> allJudgeLines) =>
        new(
            FatherUnbindHelpers.JudgeLineCacheTable.GetOrCreateValue(allJudgeLines),
            LogInfo,
            LogWarning,
            LogError,
            LogDebug
        );

    private FatherUnbindPlusProcessor CreatePlusProcessor(
        List<JudgeLine> allJudgeLines,
        double tolerance
    ) =>
        new(
            FatherUnbindHelpers.JudgeLineCacheTable.GetOrCreateValue(allJudgeLines),
            tolerance,
            LogInfo,
            LogWarning,
            LogError,
            LogDebug
        );

    /// <inheritdoc/>
    public JudgeLine FatherUnbind(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        double precision,
        IProgress<ToolProgress>? progress = null
    ) =>
        CreateProcessor(allJudgeLines)
            .FatherUnbind(targetJudgeLineIndex, allJudgeLines, precision, progress);

    /// <inheritdoc/>
    public JudgeLine FatherUnbind(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        CoordinateProfile renderProfile,
        double precision,
        IProgress<ToolProgress>? progress = null
    )
    {
        using var _ = FatherUnbindHelpers.UseRenderProfile(renderProfile);
        return FatherUnbind(targetJudgeLineIndex, allJudgeLines, precision, progress);
    }

    /// <summary>
    /// 将判定线与父判定线解绑（自适应采样）。
    /// 以事件边界为强制切割点，仅在误差超过容差时插入新采样段，相较等间隔版可减少冗余段数。
    /// </summary>
    public JudgeLine FatherUnbind(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        double precision,
        double tolerance,
        IProgress<ToolProgress>? progress = null
    ) =>
        CreatePlusProcessor(allJudgeLines, tolerance)
            .FatherUnbind(targetJudgeLineIndex, allJudgeLines, precision, progress);

    /// <summary>
    /// 将判定线与父判定线解绑（自适应采样，指定渲染坐标系）。
    /// </summary>
    public JudgeLine FatherUnbind(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        CoordinateProfile renderProfile,
        double precision,
        double tolerance,
        IProgress<ToolProgress>? progress = null
    )
    {
        using var _ = FatherUnbindHelpers.UseRenderProfile(renderProfile);
        return FatherUnbind(targetJudgeLineIndex, allJudgeLines, precision, tolerance, progress);
    }
}
