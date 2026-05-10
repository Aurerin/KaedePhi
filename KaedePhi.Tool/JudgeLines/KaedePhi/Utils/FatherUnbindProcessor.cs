using System.Collections.Concurrent;
using KaedePhi.Core.Common;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Event.KaedePhi;
using JudgeLine = KaedePhi.Core.KaedePhi.JudgeLine;

namespace KaedePhi.Tool.JudgeLines.KaedePhi.Utils;

/// <summary>
/// KPC 判定线父子解绑同步处理器。
/// 所有采样算法均委托给 <see cref="FatherUnbindHelpers"/> 中的共享实现，
/// 本类只负责缓存检查、父线递归解绑、通道合并及日志记录。
/// </summary>
public class FatherUnbindProcessor
{
    private readonly EventListMerger<double> _merger = new();
    private readonly EventCutter<double> _cutter = new();

    private readonly ConcurrentDictionary<int, JudgeLine> _cache;
    private readonly Action<string>? _logInfo;
    private readonly Action<string>? _logWarning;
    private readonly Action<string>? _logError;
    private readonly Action<string>? _logDebug;

    public FatherUnbindProcessor(
        ConcurrentDictionary<int, JudgeLine> cache,
        Action<string>? logInfo = null,
        Action<string>? logWarning = null,
        Action<string>? logError = null,
        Action<string>? logDebug = null)
    {
        _cache = cache;
        _logInfo = logInfo;
        _logWarning = logWarning;
        _logError = logError;
        _logDebug = logDebug;
    }

    private readonly record struct PrepareResult(
        JudgeLine JudgeLine,
        JudgeLine? FatherLine,
        bool ShouldReturn);

    private PrepareResult PrepareUnbindContext(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        string logTag,
        string startAction,
        Func<int, List<JudgeLine>, JudgeLine> recursiveUnbind)
    {
        if (FatherUnbindHelpers.TryGetCachedClone(targetJudgeLineIndex, _cache, logTag, out var cached, _logDebug))
        {
            return new PrepareResult(cached, null, true);
        }

        var judgeLineCopy = allJudgeLines[targetJudgeLineIndex].Clone();
        var allJudgeLinesCopy = allJudgeLines.Select(jl => jl.Clone()).ToList();

        if (FatherUnbindHelpers.TryReturnWhenNoFather(targetJudgeLineIndex, judgeLineCopy, _cache, logTag, _logWarning))
        {
            return new PrepareResult(judgeLineCopy, null, true);
        }

        _logInfo?.Invoke($"{logTag}[{targetJudgeLineIndex}]: {startAction}，父线索引={judgeLineCopy.Father}");

        var fatherLineCopy = allJudgeLinesCopy[judgeLineCopy.Father].Clone();
        if (fatherLineCopy.Father >= 0)
        {
            _logDebug?.Invoke($"{logTag}[{targetJudgeLineIndex}]: 父线 {judgeLineCopy.Father} 仍有父线，递归解绑");
            fatherLineCopy = recursiveUnbind(judgeLineCopy.Father, allJudgeLinesCopy);
        }

        FatherUnbindHelpers.CleanupRedundantLayers(judgeLineCopy, fatherLineCopy);

        return new PrepareResult(judgeLineCopy, fatherLineCopy, false);
    }

    /// <summary>
    /// 等间隔采样解绑（同步版）：将判定线与父线解绑，以等间隔拍步长采样保持原始行为。
    /// </summary>
    public JudgeLine FatherUnbind(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        double precision,
        IProgress<ToolProgress>? progress = null)
    {
        JudgeLine judgeLineCopy;
        try
        {
            progress?.Report(new ToolProgress(0.1, "准备解绑上下文"));

            var (judgeLine, fatherLine, shouldReturn) = PrepareUnbindContext(
                targetJudgeLineIndex,
                allJudgeLines,
                logTag: "FatherUnbind",
                startAction: "开始解绑",
                recursiveUnbind: (idx, lines) => FatherUnbind(idx, lines, precision, progress));

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
                Task.Run(() => _cutter.CutEventsInRange(mergedChannels.Tx, txMin, txMax, cutLength)),
                Task.Run(() => _cutter.CutEventsInRange(mergedChannels.Ty, tyMin, tyMax, cutLength)),
                Task.Run(() => _cutter.CutEventsInRange(mergedChannels.Fx, fxMin, fxMax, cutLength)),
                Task.Run(() => _cutter.CutEventsInRange(mergedChannels.Fy, fyMin, fyMax, cutLength)),
                Task.Run(() => _cutter.CutEventsInRange(mergedChannels.Fr, frMin, frMax, cutLength))
            };
            // Task.WaitAll 只读取数组元素进行等待，不会向数组写入任何对象，
            // 因此此处协变数组转换（Task<List<Event<double>>>[] → Task[]）绝不会触发 ArrayTypeMismatchException。
            // ReSharper disable once CoVariantArrayConversion
            Task.WaitAll(cutTasks);

            var cutChannels = new FatherUnbindHelpers.EventChannels(
                Fx: cutTasks[2].Result, Fy: cutTasks[3].Result, Fr: cutTasks[4].Result,
                Tx: cutTasks[0].Result, Ty: cutTasks[1].Result);

            var overallMin = new Beat(Math.Min(Math.Min(Math.Min(txMin, tyMin), Math.Min(fxMin, fyMin)), frMin));
            var overallMax = new Beat(Math.Max(Math.Max(Math.Max(txMax, tyMax), Math.Max(fxMax, fyMax)), frMax));
            var step = new Beat(1d / precision);
            var beats = FatherUnbindHelpers.BuildBeatList(overallMin, overallMax, step);

            progress?.Report(new ToolProgress(0.6, "等间隔采样"));
            _logDebug?.Invoke($"FatherUnbind[{targetJudgeLineIndex}]: 等间隔采样 {beats.Count} 段，精度={precision}");
            var (sortedX, sortedY) = FatherUnbindHelpers.EqualSpacingSampling(beats, overallMax, step, cutChannels);

            progress?.Report(new ToolProgress(0.9, "写回结果"));
            _logDebug?.Invoke($"FatherUnbind[{targetJudgeLineIndex}]: 采样完成，写回");
            FatherUnbindHelpers.WriteResultToLine(judgeLineCopy, sortedX, sortedY, cutChannels.Fr, Merge);

            _cache.TryAdd(targetJudgeLineIndex, judgeLineCopy);
            _logInfo?.Invoke($"FatherUnbind[{targetJudgeLineIndex}]: 解绑完成");
            progress?.Report(new ToolProgress(1.0));
            return judgeLineCopy;

            List<Kpc.Event<double>> Merge(List<Kpc.Event<double>> a, List<Kpc.Event<double>> b)
                => _merger.EventListMerge(a, b, precision);
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"FatherUnbind[{targetJudgeLineIndex}]: 解绑失败，返回原始数据: " + ex.Message);
            return allJudgeLines[targetJudgeLineIndex].Clone();
        }
    }

    /// <summary>
    /// 自适应采样解绑（同步版）：以事件边界为强制切割点，仅在误差超过容差时插入新采样段，
    /// 相较等间隔版可减少冗余段数。
    /// </summary>
    public JudgeLine FatherUnbindPlus(
        int targetJudgeLineIndex,
        List<JudgeLine> allJudgeLines,
        double precision, double tolerance,
        IProgress<ToolProgress>? progress = null)
    {
        JudgeLine judgeLineCopy;
        try
        {
            progress?.Report(new ToolProgress(0.1, "准备解绑上下文"));

            var (judgeLine, fatherLine, shouldReturn) = PrepareUnbindContext(
                targetJudgeLineIndex,
                allJudgeLines,
                logTag: "FatherUnbindPlus",
                startAction: "开始解绑（自适应采样）",
                recursiveUnbind: (idx, lines) => FatherUnbindPlus(idx, lines, precision, tolerance, progress));

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
                _cache.TryAdd(targetJudgeLineIndex, judgeLineCopy);
                progress?.Report(new ToolProgress(1.0));
                return judgeLineCopy;
            }

            var (overallMin, overallMax) = rangeResult.Value;
            var step = new Beat(1d / precision);

            progress?.Report(new ToolProgress(0.4, "收集关键帧"));
            var keyBeats = FatherUnbindHelpers.CollectKeyBeats(overallMin, overallMax, ch);

            progress?.Report(new ToolProgress(0.6, "自适应采样"));
            _logDebug?.Invoke(
                $"FatherUnbindPlus[{targetJudgeLineIndex}]: 自适应采样，关键帧数={keyBeats.Count}，最大精度={precision}");
            var (resultX, resultY) = FatherUnbindHelpers.RunAdaptiveSampling(keyBeats, step, tolerance, ch);

            progress?.Report(new ToolProgress(0.9, "写回结果"));
            _logDebug?.Invoke(
                $"FatherUnbindPlus[{targetJudgeLineIndex}]: 采样完成（生成 {resultX.Count} 段），压缩并写回");
            FatherUnbindHelpers.WriteResultToLine(
                judgeLineCopy, resultX, resultY, ch.Fr, Merge);

            _cache.TryAdd(targetJudgeLineIndex, judgeLineCopy);
            _logInfo?.Invoke($"FatherUnbindPlus[{targetJudgeLineIndex}]: 解绑完成");
            progress?.Report(new ToolProgress(1.0));
            return judgeLineCopy;

            List<Kpc.Event<double>> Merge(List<Kpc.Event<double>> a, List<Kpc.Event<double>> b)
                => _merger.EventListMergePlus(a, b, precision, tolerance);
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"FatherUnbindPlus[{targetJudgeLineIndex}]: 解绑失败，返回原始数据: " + ex.Message);
            return allJudgeLines[targetJudgeLineIndex].Clone();
        }
    }
}
