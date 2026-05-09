using System.Collections.Concurrent;
using KaedePhi.Core.Common;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Event.KaedePhi;
using JudgeLine = KaedePhi.Core.KaedePhi.JudgeLine;

namespace KaedePhi.Tool.JudgeLines.KaedePhi.Utils;

/// <summary>
/// NRC 判定线父子解绑同步处理器。
/// 所有采样算法均委托给 <see cref="FatherUnbindHelpers"/> 中的共享实现，
/// 本类只负责缓存检查、父线递归解绑、通道合并及日志记录。
/// </summary>
public static class FatherUnbindProcessor
{
    private static readonly EventMerger<double> Merger = new();
    private static readonly EventCutter<double> Cutter = new();

    private readonly record struct PrepareResult(
        JudgeLine JudgeLine,
        JudgeLine? FatherLine,
        bool ShouldReturn);

    private static PrepareResult PrepareUnbindContext(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        ConcurrentDictionary<int, JudgeLine> cache,
        string logTag,
        string startAction,
        Func<int, List<JudgeLine>, JudgeLine> recursiveUnbind,
        Action<string>? logInfo,
        Action<string>? logWarning,
        Action<string>? logError,
        Action<string>? logDebug)
    {
        if (FatherUnbindHelpers.TryGetCachedClone(targetJudgeLineIndex, cache, logTag, out var cached, logDebug))
        {
            return new PrepareResult(cached, null, true);
        }

        var judgeLineCopy = allJudgeLines[targetJudgeLineIndex].Clone();
        var allJudgeLinesCopy = allJudgeLines.Select(jl => jl.Clone()).ToList();

        if (FatherUnbindHelpers.TryReturnWhenNoFather(targetJudgeLineIndex, judgeLineCopy, cache, logTag, logWarning))
        {
            return new PrepareResult(judgeLineCopy, null, true);
        }

        logInfo?.Invoke($"{logTag}[{targetJudgeLineIndex}]: {startAction}，父线索引={judgeLineCopy.Father}");

        var fatherLineCopy = allJudgeLinesCopy[judgeLineCopy.Father].Clone();
        if (fatherLineCopy.Father >= 0)
        {
            logDebug?.Invoke($"{logTag}[{targetJudgeLineIndex}]: 父线 {judgeLineCopy.Father} 仍有父线，递归解绑");
            fatherLineCopy = recursiveUnbind(judgeLineCopy.Father, allJudgeLinesCopy);
        }

        FatherUnbindHelpers.CleanupRedundantLayers(judgeLineCopy, fatherLineCopy);

        return new PrepareResult(judgeLineCopy, fatherLineCopy, false);
    }

    /// <summary>
    /// 等间隔采样解绑（同步版）：将判定线与父线解绑，以等间隔拍步长采样保持原始行为。
    /// </summary>
    public static JudgeLine FatherUnbind(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        double precision,
        ConcurrentDictionary<int, JudgeLine> cache,
        Action<string>? logInfo = null,
        Action<string>? logWarning = null,
        Action<string>? logError = null,
        Action<string>? logDebug = null,
        IProgress<ToolProgress>? progress = null)
    {
        JudgeLine judgeLineCopy;
        try
        {
            progress?.Report(new ToolProgress(0.1, "准备解绑上下文"));

            var (judgeLine, fatherLine, shouldReturn) = PrepareUnbindContext(
                targetJudgeLineIndex,
                allJudgeLines,
                cache,
                logTag: "FatherUnbind",
                startAction: "开始解绑",
                recursiveUnbind: (idx, lines) => FatherUnbind(idx, lines, precision, cache, logInfo, logWarning, logError, logDebug, progress),
                logInfo, logWarning, logError, logDebug);

            judgeLineCopy = judgeLine;
            if (shouldReturn || fatherLine is null)
            {
                progress?.Report(new ToolProgress(1.0));
                return judgeLineCopy;
            }

            progress?.Report(new ToolProgress(0.2, "合并通道"));
            var mergedChannels =
                FatherUnbindHelpers.MergeChannels(judgeLineCopy.EventLayers, fatherLine.EventLayers, Merge);

            progress?.Report(new ToolProgress(0.4, "切割事件"));
            var cutLength = new Beat(1d / precision);
            var (txMin, txMax) = FatherUnbindHelpers.GetEventRange(mergedChannels.Tx);
            var (tyMin, tyMax) = FatherUnbindHelpers.GetEventRange(mergedChannels.Ty);
            var (fxMin, fxMax) = FatherUnbindHelpers.GetEventRange(mergedChannels.Fx);
            var (fyMin, fyMax) = FatherUnbindHelpers.GetEventRange(mergedChannels.Fy);
            var (frMin, frMax) = FatherUnbindHelpers.GetEventRange(mergedChannels.Fr);

            var cutTasks = new[]
            {
                Task.Run(() => Cutter.CutEventsInRange(mergedChannels.Tx, txMin, txMax, cutLength)),
                Task.Run(() => Cutter.CutEventsInRange(mergedChannels.Ty, tyMin, tyMax, cutLength)),
                Task.Run(() => Cutter.CutEventsInRange(mergedChannels.Fx, fxMin, fxMax, cutLength)),
                Task.Run(() => Cutter.CutEventsInRange(mergedChannels.Fy, fyMin, fyMax, cutLength)),
                Task.Run(() => Cutter.CutEventsInRange(mergedChannels.Fr, frMin, frMax, cutLength))
            };
            Task.WaitAll(cutTasks);

            var cutChannels = new FatherUnbindHelpers.EventChannels(
                Fx: cutTasks[2].Result, Fy: cutTasks[3].Result, Fr: cutTasks[4].Result,
                Tx: cutTasks[0].Result, Ty: cutTasks[1].Result);

            var overallMin = new Beat(Math.Min(Math.Min(Math.Min(txMin, tyMin), Math.Min(fxMin, fyMin)), frMin));
            var overallMax = new Beat(Math.Max(Math.Max(Math.Max(txMax, tyMax), Math.Max(fxMax, fyMax)), frMax));
            var step = new Beat(1d / precision);
            var beats = FatherUnbindHelpers.BuildBeatList(overallMin, overallMax, step);

            progress?.Report(new ToolProgress(0.6, "等间隔采样"));
            logDebug?.Invoke($"FatherUnbind[{targetJudgeLineIndex}]: 等间隔采样 {beats.Count} 段，精度={precision}");
            var (sortedX, sortedY) = FatherUnbindHelpers.EqualSpacingSampling(beats, overallMax, step, cutChannels);

            progress?.Report(new ToolProgress(0.9, "写回结果"));
            logDebug?.Invoke($"FatherUnbind[{targetJudgeLineIndex}]: 采样完成，写回");
            FatherUnbindHelpers.WriteResultToLine(judgeLineCopy, sortedX, sortedY, cutChannels.Fr, Merge);

            cache.TryAdd(targetJudgeLineIndex, judgeLineCopy);
            logInfo?.Invoke($"FatherUnbind[{targetJudgeLineIndex}]: 解绑完成");
            progress?.Report(new ToolProgress(1.0));
            return judgeLineCopy;

            List<Kpc.Event<double>> Merge(List<Kpc.Event<double>> a, List<Kpc.Event<double>> b)
                => Merger.EventListMerge(a, b, precision);
        }
        catch (Exception ex)
        {
            logError?.Invoke($"FatherUnbind[{targetJudgeLineIndex}]: 解绑失败，返回原始数据: " + ex.Message);
            return allJudgeLines[targetJudgeLineIndex].Clone();
        }
    }

    /// <summary>
    /// 自适应采样解绑（同步版）：以事件边界为强制切割点，仅在误差超过容差时插入新采样段，
    /// 相较等间隔版可减少冗余段数。
    /// </summary>
    public static JudgeLine FatherUnbindPlus(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        double precision, double tolerance,
        ConcurrentDictionary<int, JudgeLine> cache,
        Action<string>? logInfo = null,
        Action<string>? logWarning = null,
        Action<string>? logError = null,
        Action<string>? logDebug = null,
        IProgress<ToolProgress>? progress = null)
    {
        JudgeLine judgeLineCopy;
        try
        {
            progress?.Report(new ToolProgress(0.1, "准备解绑上下文"));

            var (judgeLine, fatherLine, shouldReturn) = PrepareUnbindContext(
                targetJudgeLineIndex,
                allJudgeLines,
                cache,
                logTag: "FatherUnbindPlus",
                startAction: "开始解绑（自适应采样）",
                recursiveUnbind: (idx, lines) => FatherUnbindPlus(idx, lines, precision, tolerance, cache, logInfo, logWarning, logError, logDebug, progress),
                logInfo, logWarning, logError, logDebug);

            judgeLineCopy = judgeLine;
            if (shouldReturn || fatherLine is null)
            {
                progress?.Report(new ToolProgress(1.0));
                return judgeLineCopy;
            }

            progress?.Report(new ToolProgress(0.2, "合并通道"));
            var ch = FatherUnbindHelpers.MergeChannels(judgeLineCopy.EventLayers, fatherLine.EventLayers, Merge);

            var rangeResult = FatherUnbindHelpers.TryGetOverallRange(ch);
            if (rangeResult is null)
            {
                judgeLineCopy.Father = -1;
                cache.TryAdd(targetJudgeLineIndex, judgeLineCopy);
                progress?.Report(new ToolProgress(1.0));
                return judgeLineCopy;
            }

            var (overallMin, overallMax) = rangeResult.Value;
            var step = new Beat(1d / precision);

            progress?.Report(new ToolProgress(0.4, "收集关键帧"));
            var keyBeats = FatherUnbindHelpers.CollectKeyBeats(overallMin, overallMax, ch);

            progress?.Report(new ToolProgress(0.6, "自适应采样"));
            logDebug?.Invoke(
                $"FatherUnbindPlus[{targetJudgeLineIndex}]: 自适应采样，关键帧数={keyBeats.Count}，最大精度={precision}");
            var (resultX, resultY) = FatherUnbindHelpers.RunAdaptiveSampling(keyBeats, step, tolerance, ch);

            progress?.Report(new ToolProgress(0.9, "写回结果"));
            logDebug?.Invoke(
                $"FatherUnbindPlus[{targetJudgeLineIndex}]: 采样完成（生成 {resultX.Count} 段），压缩并写回");
            FatherUnbindHelpers.WriteResultToLine(
                judgeLineCopy, resultX, resultY, ch.Fr, Merge);

            cache.TryAdd(targetJudgeLineIndex, judgeLineCopy);
            logInfo?.Invoke($"FatherUnbindPlus[{targetJudgeLineIndex}]: 解绑完成");
            progress?.Report(new ToolProgress(1.0));
            return judgeLineCopy;

            List<Kpc.Event<double>> Merge(List<Kpc.Event<double>> a, List<Kpc.Event<double>> b)
                => Merger.EventMergePlus(a, b, precision, tolerance);
        }
        catch (Exception ex)
        {
            logError?.Invoke($"FatherUnbindPlus[{targetJudgeLineIndex}]: 解绑失败，返回原始数据: " + ex.Message);
            return allJudgeLines[targetJudgeLineIndex].Clone();
        }
    }
}
