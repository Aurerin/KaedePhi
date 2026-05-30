using KaedePhi.Core.Common;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Event.KaedePhi;

/// <summary>
/// 将连续的线性事件序列拟合为带缓动函数的单一事件。
/// </summary>
public class EventFit<TPayload> : LoggableBase, IEventFit<Kpc.Event<TPayload>>
{
    private static readonly int[] AllEasingIds = Enumerable.Range(1, 31).ToArray();

    /// <inheritdoc/>
    public List<Kpc.Event<TPayload>> FitEvents(
        List<Kpc.Event<TPayload>>? events, double tolerance)
    {
        EnsureSupportedNumericType();

        if (events == null || events.Count == 0)
            return [];

        var sortedEvents = events
            .OrderBy(e => e.StartBeat)
            .ToList();

        return FitEventsCore(sortedEvents, tolerance);
    }

    /// <summary>
    /// 遍历已排序的事件序列，将连续的同方向线性事件分组后贪心拟合；
    /// 常量事件和非线性事件直接原样写入输出。
    /// </summary>
    private static List<Kpc.Event<TPayload>> FitEventsCore(
        List<Kpc.Event<TPayload>> sortedEvents, double tolerance)
    {
        var result = new List<Kpc.Event<TPayload>>();
        var i = 0;

        while (i < sortedEvents.Count)
        {
            var current = sortedEvents[i];

            // 常量事件或非线性事件不参与拟合，直接输出
            if (IsNumericConstant(current) || !IsLinear(current))
            {
                result.Add(current.Clone());
                i++;
                continue;
            }

            var run = CollectRun(sortedEvents, i, out var nextI);
            FitRunInto(run, result, tolerance);
            i = nextI;
        }

        return result;
    }

    /// <summary>
    /// 从指定位置起收集一段时间连续、数值连续、方向一致的线性事件序列（run）。
    /// </summary>
    private static List<Kpc.Event<TPayload>> CollectRun(
        List<Kpc.Event<TPayload>> events, int start, out int nextIndex)
    {
        var run = new List<Kpc.Event<TPayload>> { events[start] };
        var firstDir = GetDirection(
            events[start].GetStartValueAsDouble(),
            events[start].GetEndValueAsDouble());

        nextIndex = start + 1;
        while (nextIndex < events.Count)
        {
            var next = events[nextIndex];
            if (IsNumericConstant(next)) break;
            if (!IsLinear(next)) break;
            if (!IsContiguous(run[^1], next)) break;
            if (GetDirection(next.GetStartValueAsDouble(), next.GetEndValueAsDouble()) != firstDir) break;

            run.Add(next);
            nextIndex++;
        }

        return run;
    }

    /// <summary>
    /// 对 run 中的事件序列进行贪心迭代拟合：
    /// 优先尝试整段拟合，失败后逐步缩短前缀，直到每段都能拟合或退化为单个事件。
    /// 使用迭代替代递归以避免深层调用栈。
    /// </summary>
    private static void FitRunInto(
        List<Kpc.Event<TPayload>> run,
        List<Kpc.Event<TPayload>> target,
        double tolerance)
    {
        var remaining = run;

        while (remaining.Count > 0)
        {
            if (remaining.Count == 1)
            {
                target.Add(remaining[0].Clone());
                return;
            }

            // 尝试将当前剩余序列整体拟合为单一缓动事件
            var fitted = TryFitEasing(remaining, tolerance);
            if (fitted != null)
            {
                target.Add(fitted);
                return;
            }

            // 整体拟合失败，寻找可拟合的最长前缀
            var found = false;
            for (var split = remaining.Count - 1; split >= 2; split--)
            {
                fitted = TryFitEasing(remaining.GetRange(0, split), tolerance);
                if (fitted != null)
                {
                    target.Add(fitted);
                    remaining = remaining.GetRange(split, remaining.Count - split);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // 无法拟合任何长度 >= 2 的前缀，输出第一个事件并继续处理剩余
                target.Add(remaining[0].Clone());
                remaining = remaining.GetRange(1, remaining.Count - 1);
            }
        }
    }

    /// <summary>
    /// 遍历所有支持的缓动函数，返回第一个在容差范围内能覆盖所有事件边界的拟合结果；
    /// 无法拟合时返回 null。
    /// </summary>
    private static Kpc.Event<TPayload>? TryFitEasing(
        List<Kpc.Event<TPayload>> events, double tolerance)
    {
        if (events.Count <= 1)
            return null;

        var first = events[0];
        var last = events[^1];
        var startBeat = first.StartBeat;
        var endBeat = last.EndBeat;
        var startValue = first.GetStartValueAsDouble();
        var endValue = last.GetEndValueAsDouble();

        foreach (var easingId in AllEasingIds)
        {
            var candidate = CreateMergedEvent(first, startBeat, startValue, endBeat, endValue, easingId);

            if (FitsWithinTolerance(candidate, events, tolerance, startBeat, endBeat, startValue, endValue))
                return candidate;
        }

        return null;
    }

    /// <summary>
    /// 在所有原始事件的起止边界处采样候选缓动，验证每处的相对误差百分比均不超过容差。
    /// 相对误差（%）= 绝对偏差 / 整段值域跨度 × 100。
    /// </summary>
    private static bool FitsWithinTolerance(
        Kpc.Event<TPayload> candidate,
        List<Kpc.Event<TPayload>> events,
        double tolerance,
        Beat segStartBeat, Beat segEndBeat,
        double segStartValue, double segEndValue)
    {
        var segSpan = (double)segEndBeat - (double)segStartBeat;
        if (segSpan <= 1e-9)
            return true;

        var valueDelta = segEndValue - segStartValue;
        var valueRange = Math.Abs(valueDelta);

        // 值域近似为零时无法定义相对误差，视为拟合成功
        if (valueRange <= 1e-9)
            return true;

        foreach (var evt in events)
        {
            var evtStart = (double)evt.StartBeat;
            var evtEnd = (double)evt.EndBeat;

            var normStart = (evtStart - (double)segStartBeat) / segSpan;
            var normEnd = (evtEnd - (double)segStartBeat) / segSpan;

            var easedStart = segStartValue + valueDelta * GetEasingValue(candidate.Easing, normStart);
            var easedEnd = segStartValue + valueDelta * GetEasingValue(candidate.Easing, normEnd);

            var srcStartVal = evt.GetStartValueAsDouble();
            var srcEndVal = evt.GetEndValueAsDouble();

            // 相对误差（%）：绝对偏差 / 值域 × 100 与容差百分比比较
            if (Math.Abs(easedStart - srcStartVal) / valueRange * 100.0 > tolerance ||
                Math.Abs(easedEnd - srcEndVal) / valueRange * 100.0 > tolerance)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 获取指定缓动函数在归一化时间 t 处的输出值（范围 [0, 1]）。
    /// </summary>
    private static double GetEasingValue(Kpc.Easing easing, double t)
    {
        return Kpc.Easings.Evaluate(easing, 0d, 1d, t);
    }

    /// <summary>
    /// 判断两个事件是否在时间上首尾相接且数值连续。
    /// </summary>
    private static bool IsContiguous(Kpc.Event<TPayload> first, Kpc.Event<TPayload> second)
    {
        return first.EndBeat == second.StartBeat &&
               Math.Abs(first.GetEndValueAsDouble() - second.GetStartValueAsDouble()) < 1e-9;
    }

    /// <summary>
    /// 判断事件起止值是否相同（常量事件）。
    /// </summary>
    private static bool IsNumericConstant(Kpc.Event<TPayload> evt)
    {
        return Math.Abs(evt.GetStartValueAsDouble() - evt.GetEndValueAsDouble()) < 1e-9;
    }

    /// <summary>
    /// 判断事件是否为全范围线性缓动，即可参与拟合的基本条件。
    /// </summary>
    private static bool IsLinear(Kpc.Event<TPayload> evt)
    {
        return (int)evt.Easing == 1 &&
               Math.Abs(evt.EasingLeft) < 1e-6f &&
               Math.Abs(evt.EasingRight - 1f) < 1e-6f;
    }

    /// <summary>
    /// 返回数值从 start 到 end 的变化方向：递增为 1，递减为 -1，不变为 0。
    /// </summary>
    private static int GetDirection(double start, double end)
    {
        var diff = end - start;
        if (Math.Abs(diff) < 1e-9) return 0;
        return diff > 0 ? 1 : -1;
    }

    /// <summary>
    /// 使用指定缓动函数创建覆盖给定时间区间和值域的新事件。
    /// Font 字段从模板事件继承，其余字段均为合并后的值。
    /// </summary>
    private static Kpc.Event<TPayload> CreateMergedEvent(
        Kpc.Event<TPayload> template,
        Beat startBeat, double startValue,
        Beat endBeat, double endValue,
        int easingId)
    {
        var evt = new Kpc.Event<TPayload>
        {
            StartBeat = new Beat((int[])startBeat),
            EndBeat = new Beat((int[])endBeat),
            Easing = new Kpc.Easing(easingId),
            Font = template.Font
        };

        if (typeof(TPayload) == typeof(double))
        {
            evt.StartValue = (TPayload)(object)startValue;
            evt.EndValue = (TPayload)(object)endValue;
        }
        else if (typeof(TPayload) == typeof(float))
        {
            evt.StartValue = (TPayload)(object)(float)startValue;
            evt.EndValue = (TPayload)(object)(float)endValue;
        }
        else if (typeof(TPayload) == typeof(int))
        {
            evt.StartValue = (TPayload)(object)(int)startValue;
            evt.EndValue = (TPayload)(object)(int)endValue;
        }

        return evt;
    }

    private static void EnsureSupportedNumericType()
    {
        if (typeof(TPayload) != typeof(int) && typeof(TPayload) != typeof(float) && typeof(TPayload) != typeof(double))
            throw new NotSupportedException("EventFit only supports int, float, and double types.");
    }
}
