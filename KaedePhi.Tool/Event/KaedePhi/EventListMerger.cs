using KaedePhi.Core.Common;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Event.KaedePhi;

/// <summary>
/// 对两组 <see cref="Kpc.Event{TPayload}"/> 进行叠加合并的实现类。
/// <para>
/// 合并语义：目标轨道（<c>toEvents</c>）与来源轨道（<c>fromEvents</c>）的值在时间轴上逐拍相加。
/// 非重叠区段直接以对方轨道的前序终值作为偏移；重叠区段通过固定步长采样（<see cref="EventListMerge"/>）
/// 或自适应采样（<see cref="EventListMergePlus"/>）将两轨道在每个切片处的瞬时值直接求和。
/// </para>
/// </summary>
public class EventListMerger<TPayload> : LoggableBase, IEventListMerger<Kpc.Event<TPayload>>
{
    private static readonly EventCutter<TPayload> Cutter = new();

    #region 入口

    /// <inheritdoc/>
    public List<Kpc.Event<TPayload>> EventListMerge(
        List<Kpc.Event<TPayload>>? toEvents,
        List<Kpc.Event<TPayload>>? fromEvents,
        double precision)
    {
        if (TryGetMergeEarlyReturn(toEvents, fromEvents, out var earlyReturn)) return earlyReturn;
        if (toEvents == null || fromEvents == null) return [];
        EnsureSupportedNumericType();

        var toEventsCopy = CloneEventList(toEvents);
        var fromEventsCopy = CloneEventList(fromEvents);

        if (!HasOverlap(toEventsCopy, fromEventsCopy))
            return MergeWithoutOverlap(toEventsCopy, fromEventsCopy);

        return MergeWithOverlapFixedSampling(
            toEvents, toEventsCopy, fromEventsCopy, precision);
    }

    /// <inheritdoc/>
    public List<Kpc.Event<TPayload>> EventListMergePlus(
        List<Kpc.Event<TPayload>>? toEvents,
        List<Kpc.Event<TPayload>>? fromEvents,
        double precision,
        double tolerance)
    {
        if (TryGetMergeEarlyReturn(toEvents, fromEvents, out var earlyReturn)) return earlyReturn;
        if (toEvents == null || fromEvents == null) return [];

        EnsureSupportedNumericType();

        var toEventsCopy = CloneEventList(toEvents);
        var fromEventsCopy = CloneEventList(fromEvents);
        SortByStartBeat(toEventsCopy);
        SortByStartBeat(fromEventsCopy);

        if (!HasOverlap(toEventsCopy, fromEventsCopy))
            return MergeWithoutOverlap(toEventsCopy, fromEventsCopy);

        return MergeWithOverlapAdaptiveSampling(
            toEvents, toEventsCopy, fromEventsCopy, precision, tolerance);
    }

    #endregion

    // 快速返回

    /// <summary>
    /// 当任意一个输入列表为 <see langword="null"/> 或空时，直接给出合并结果并跳过完整流程。
    /// <list type="bullet">
    ///   <item>两者均为空 → 返回空列表。</item>
    ///   <item>仅 <paramref name="toEvents"/> 为空 → 返回 <paramref name="fromEvents"/> 的深拷贝。</item>
    ///   <item>仅 <paramref name="fromEvents"/> 为空 → 返回 <paramref name="toEvents"/> 的深拷贝。</item>
    ///   <item>两者均非空 → 不命中，输出空列表并返回 <see langword="false"/>。</item>
    /// </list>
    /// </summary>
    /// <param name="toEvents">目标轨道事件列表（可为 <see langword="null"/>）。</param>
    /// <param name="fromEvents">来源轨道事件列表（可为 <see langword="null"/>）。</param>
    /// <param name="result">命中提前返回时的结果；未命中时为空列表。</param>
    /// <returns>命中提前返回条件时为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    private static bool TryGetMergeEarlyReturn(
        List<Kpc.Event<TPayload>>? toEvents,
        List<Kpc.Event<TPayload>>? fromEvents,
        out List<Kpc.Event<TPayload>> result)
    {
        if (toEvents == null || toEvents.Count == 0)
        {
            result = fromEvents == null || fromEvents.Count == 0
                ? []
                : fromEvents.Select(e => e.Clone()).ToList();
            return true;
        }

        if (fromEvents == null || fromEvents.Count == 0)
        {
            result = toEvents.Select(e => e.Clone()).ToList();
            return true;
        }

        result = [];
        return false;
    }

    #region 共用工具

    /// <summary>
    /// 校验 <typeparamref name="TPayload"/> 是否为合并器支持的数值类型（<see cref="int"/>、<see cref="float"/> 或 <see cref="double"/>）。
    /// 不满足时抛出 <see cref="NotSupportedException"/>。
    /// </summary>
    /// <exception cref="NotSupportedException"><typeparamref name="TPayload"/> 不是受支持的数值类型。</exception>
    private static void EnsureSupportedNumericType()
    {
        if (typeof(TPayload) != typeof(int) && typeof(TPayload) != typeof(float) && typeof(TPayload) != typeof(double))
            throw new NotSupportedException("EventMerge only supports int, float, and double types.");
    }

    /// <summary>
    /// 对 <paramref name="events"/> 执行深拷贝，返回元素全部克隆的新列表。
    /// </summary>
    /// <param name="events">待拷贝的事件列表。</param>
    /// <returns>与原列表元素数量相同、但全部为新实例的克隆列表。</returns>
    private static List<Kpc.Event<TPayload>> CloneEventList(List<Kpc.Event<TPayload>> events)
        => events.Select(e => e.Clone()).ToList();

    /// <summary>
    /// 将 <paramref name="events"/> 按 <see cref="Kpc.Event{TPayload}.StartBeat"/> 升序原地排序。
    /// </summary>
    /// <param name="events">待排序的事件列表（原地修改）。</param>
    private static void SortByStartBeat(List<Kpc.Event<TPayload>> events)
        => events.Sort((a, b) => a.StartBeat.CompareTo(b.StartBeat));

    /// <summary>
    /// 使用双指针算法判断两个事件列表是否存在时间重叠，时间复杂度 O(N+M)。
    /// 调用前会对两个列表按 <see cref="Kpc.Event{TPayload}.StartBeat"/> 排序（原地）。
    /// </summary>
    /// <param name="toEvents">目标轨道事件列表（将被原地排序）。</param>
    /// <param name="fromEvents">来源轨道事件列表（将被原地排序）。</param>
    /// <returns>存在至少一对事件的时间范围互相交叉时返回 <see langword="true"/>。</returns>
    private static bool HasOverlap(List<Kpc.Event<TPayload>> toEvents, List<Kpc.Event<TPayload>> fromEvents)
    {
        // 确保两个列表都已排序
        SortByStartBeat(toEvents);
        SortByStartBeat(fromEvents);

        var i = 0;
        var j = 0;
        while (i < toEvents.Count && j < fromEvents.Count)
        {
            var te = toEvents[i];
            var fe = fromEvents[j];

            // 检查重叠：fe.StartBeat < te.EndBeat && fe.EndBeat > te.StartBeat
            if (fe.StartBeat < te.EndBeat && fe.EndBeat > te.StartBeat)
                return true;

            // 移动结束时间较早的指针
            if (te.EndBeat <= fe.EndBeat)
                i++;
            else
                j++;
        }

        return false;
    }

    #endregion

    #region 非重叠路径

    /// <summary>
    /// 在两轨道无时间重叠的前提下合并事件列表。
    /// <para>
    /// 对目标轨道的每条事件，取来源轨道中结束时间不晚于该事件起始拍的最后一条事件的终值作为偏移，
    /// 加到该事件的起/终值上；对来源轨道做对称处理。
    /// 使用二分查找实现前序终值查询，时间复杂度 O((N+M)·log(N+M))。
    /// </para>
    /// <para>调用前需确保两列表已按 StartBeat 排序；方法内会再次排序以保证正确性。</para>
    /// </summary>
    /// <param name="toEventsCopy">目标轨道事件的深拷贝列表（原地排序）。</param>
    /// <param name="fromEventsCopy">来源轨道事件的深拷贝列表（原地排序）。</param>
    /// <returns>两轨道合并后按 StartBeat 升序排列的新事件列表。</returns>
    private static List<Kpc.Event<TPayload>> MergeWithoutOverlap(
        List<Kpc.Event<TPayload>> toEventsCopy,
        List<Kpc.Event<TPayload>> fromEventsCopy)
    {
        // 确保排序
        SortByStartBeat(toEventsCopy);
        SortByStartBeat(fromEventsCopy);

        var newEvents = new List<Kpc.Event<TPayload>>(toEventsCopy.Count + fromEventsCopy.Count);

        // 处理 toEvents，查找 fromEvents 中的前序值
        foreach (var toEvent in toEventsCopy)
        {
            var formOffset = GetPreviousEndValue(fromEventsCopy, toEvent.StartBeat);
            newEvents.Add(new Kpc.Event<TPayload>
            {
                StartBeat = toEvent.StartBeat,
                EndBeat = toEvent.EndBeat,
                StartValue = (dynamic?)toEvent.StartValue + (dynamic?)formOffset,
                EndValue = (dynamic?)toEvent.EndValue + (dynamic?)formOffset,
                BezierPoints = toEvent.BezierPoints,
                Easing = toEvent.Easing,
                EasingLeft = toEvent.EasingLeft,
                EasingRight = toEvent.EasingRight,
                IsBezier = toEvent.IsBezier,
            });
        }

        // 处理 fromEvents，查找 toEvents 中的前序值
        newEvents.AddRange(from formEvent in fromEventsCopy
            let toEventValue = GetPreviousEndValue(toEventsCopy, formEvent.StartBeat)
            select new Kpc.Event<TPayload>
            {
                StartBeat = formEvent.StartBeat,
                EndBeat = formEvent.EndBeat,
                StartValue = (dynamic?)formEvent.StartValue + (dynamic?)toEventValue,
                EndValue = (dynamic?)formEvent.EndValue + (dynamic?)toEventValue,
                BezierPoints = formEvent.BezierPoints,
                Easing = formEvent.Easing,
                EasingLeft = formEvent.EasingLeft,
                EasingRight = formEvent.EasingRight,
                IsBezier = formEvent.IsBezier,
            });

        SortByStartBeat(newEvents);
        return newEvents;
    }

    #endregion

    #region 重叠区间构建

    /// <summary>
    /// 使用扫描线算法构建两组事件的重叠区间集合，时间复杂度 O(N+M+K)（K 为重叠区间数）。
    /// <para>
    /// 相邻或包含关系的重叠区间会被 <see cref="AddOrMergeOverlapInterval"/> 归并为单一区间。
    /// 返回前按区间起始拍升序排序。
    /// </para>
    /// <para>调用前需确保两列表已按 StartBeat 排序；方法内会再次排序以保证正确性。</para>
    /// </summary>
    /// <param name="toEvents">目标轨道事件列表（将被原地排序）。</param>
    /// <param name="fromEvents">来源轨道事件列表（将被原地排序）。</param>
    /// <returns>已去重、归并并按起始拍升序排序的重叠区间列表。</returns>
    private static List<(Beat Start, Beat End)> BuildOverlapIntervals(
        List<Kpc.Event<TPayload>> toEvents,
        List<Kpc.Event<TPayload>> fromEvents)
    {
        // 确保排序
        SortByStartBeat(toEvents);
        SortByStartBeat(fromEvents);

        var overlapIntervals = new List<(Beat Start, Beat End)>();

        // 使用双指针扫描
        var i = 0;
        var j = 0;
        while (i < toEvents.Count && j < fromEvents.Count)
        {
            var te = toEvents[i];
            var fe = fromEvents[j];

            // 检查重叠
            if (fe.StartBeat < te.EndBeat && fe.EndBeat > te.StartBeat)
            {
                var start = fe.StartBeat > te.StartBeat ? fe.StartBeat : te.StartBeat;
                var end = fe.EndBeat < te.EndBeat ? fe.EndBeat : te.EndBeat;
                AddOrMergeOverlapInterval(overlapIntervals, start, end);
            }

            // 移动结束时间较早的指针
            if (te.EndBeat <= fe.EndBeat)
                i++;
            else
                j++;
        }

        SortIntervals(overlapIntervals);
        return overlapIntervals;
    }

    /// <summary>
    /// 将新区间 [<paramref name="start"/>, <paramref name="end"/>] 加入 <paramref name="overlapIntervals"/>。
    /// 若新区间与集合中已有区间存在交叉或相邻，则将其归并（取两者的并集），并迭代直至无新归并发生。
    /// </summary>
    /// <param name="overlapIntervals">待更新的重叠区间集合。</param>
    /// <param name="start">新区间的起始拍。</param>
    /// <param name="end">新区间的结束拍。</param>
    private static void AddOrMergeOverlapInterval(
        List<(Beat Start, Beat End)> overlapIntervals, Beat start, Beat end)
    {
        if (!overlapIntervals.Any(iv => start < iv.End && end > iv.Start))
        {
            overlapIntervals.Add((start, end));
            return;
        }

        for (var i = 0; i < overlapIntervals.Count; i++)
        {
            var iv = overlapIntervals[i];
            if (!(start < iv.End && end > iv.Start)) continue;
            var newStart = start < iv.Start ? start : iv.Start;
            var newEnd = end > iv.End ? end : iv.End;
            overlapIntervals[i] = (newStart, newEnd);
            start = newStart;
            end = newEnd;
        }
    }

    /// <summary>
    /// 将 <paramref name="overlapIntervals"/> 按区间起始拍升序排序；起始拍相同时按结束拍升序排序。
    /// </summary>
    /// <param name="overlapIntervals">待排序的区间集合（原地修改）。</param>
    private static void SortIntervals(List<(Beat Start, Beat End)> overlapIntervals)
        => overlapIntervals.Sort((a, b)
            => a.Start != b.Start ? a.Start.CompareTo(b.Start) : a.End.CompareTo(b.End));

    #endregion

    #region 固定采样路径

    /// <summary>
    /// 对存在时间重叠的两轨道事件执行固定步长采样合并。
    /// <para>流程：① 构建重叠区间列表；② 生成重叠区间外的基础事件；
    /// ③ 将重叠区间内的事件按 <paramref name="precision"/> 切片后逐片求和；④ 合并并排序。</para>
    /// </summary>
    /// <param name="toEventsForOffsetLookup">目标轨道原始事件序列，仅用于在重叠区间外查询前序终值偏移，不会被修改。</param>
    /// <param name="toEventsCopy">目标轨道事件的深拷贝，将被切片操作原地裁剪。</param>
    /// <param name="fromEventsCopy">来源轨道事件的深拷贝，将被切片操作原地裁剪。</param>
    /// <param name="precision">重叠区段的切片精度（每拍切片数）。</param>
    /// <returns>合并后按 StartBeat 升序排列的新事件列表。</returns>
    private static List<Kpc.Event<TPayload>> MergeWithOverlapFixedSampling(
        List<Kpc.Event<TPayload>> toEventsForOffsetLookup,
        List<Kpc.Event<TPayload>> toEventsCopy,
        List<Kpc.Event<TPayload>> fromEventsCopy,
        double precision)
    {
        var overlapIntervals = BuildOverlapIntervals(toEventsCopy, fromEventsCopy);
        var newEvents = BuildBaseEventsOutsideOverlap(
            toEventsCopy, fromEventsCopy, toEventsForOffsetLookup, overlapIntervals);

        var cutLength = new Beat(1d / precision);
        var (cutTo, cutFrom) =
            CutAndRemoveOverlapEvents(toEventsCopy, fromEventsCopy, overlapIntervals, cutLength);
        newEvents.AddRange(
            MergeCutOverlapSegments(toEventsCopy, fromEventsCopy, cutTo, cutFrom, overlapIntervals, cutLength));

        SortByStartBeat(newEvents);
        return newEvents;
    }

    /// <summary>
    /// 生成两轨道在所有重叠区间之外的基础事件，分别叠加对方轨道的前序偏移值。
    /// <para>对目标轨道调用 <see cref="AppendTrackEventsOutsideOverlap"/>（偏移源为来源轨道拷贝），
    /// 对来源轨道调用 <see cref="AppendTrackEventsOutsideOverlap"/>（偏移源为目标轨道原始序列）。</para>
    /// </summary>
    /// <param name="toEventsCopy">目标轨道事件的深拷贝。</param>
    /// <param name="fromEventsCopy">来源轨道事件的深拷贝（同时作为目标轨道偏移查询源）。</param>
    /// <param name="toEventsForOffsetLookup">目标轨道原始事件序列，作为来源轨道事件的偏移查询源。</param>
    /// <param name="overlapIntervals">已归并并排序的重叠区间集合。</param>
    /// <returns>两轨道重叠区间外所有偏移已叠加的合并事件。</returns>
    private static List<Kpc.Event<TPayload>> BuildBaseEventsOutsideOverlap(
        List<Kpc.Event<TPayload>> toEventsCopy,
        List<Kpc.Event<TPayload>> fromEventsCopy,
        List<Kpc.Event<TPayload>> toEventsForOffsetLookup,
        List<(Beat Start, Beat End)> overlapIntervals)
    {
        var newEvents = new List<Kpc.Event<TPayload>>();
        AppendTrackEventsOutsideOverlap(newEvents, toEventsCopy, fromEventsCopy, overlapIntervals);
        AppendTrackEventsOutsideOverlap(newEvents, fromEventsCopy, toEventsForOffsetLookup, overlapIntervals);
        return newEvents;
    }

    /// <summary>
    /// 将 <paramref name="eventSource"/> 中位于所有重叠区间之外的事件（或其空隙片段）追加到 <paramref name="newEvents"/>，
    /// 并从 <paramref name="offsetLookup"/> 中查找前序终值作为偏移叠加到起/终值上。
    /// <list type="bullet">
    ///   <item>若事件完全处于重叠区间之外：直接以原事件的 StartValue/EndValue 加偏移后输出，保留所有缓动属性。</item>
    ///   <item>若事件与重叠区间有时间交叉：仅输出事件在重叠区间之外的空隙片段，每段以 <see cref="Kpc.Event{TPayload}.GetValueAtBeat"/> 计算端点值后加偏移输出。</item>
    /// </list>
    /// </summary>
    /// <param name="newEvents">结果追加目标列表。</param>
    /// <param name="eventSource">待处理的事件列表。</param>
    /// <param name="offsetLookup">用于查询前序终值的对方轨道事件列表，需已按 StartBeat 排序。</param>
    /// <param name="overlapIntervals">已归并并按 Start 升序排序的重叠区间集合。</param>
    private static void AppendTrackEventsOutsideOverlap(
        List<Kpc.Event<TPayload>> newEvents,
        List<Kpc.Event<TPayload>> eventSource,
        List<Kpc.Event<TPayload>> offsetLookup,
        List<(Beat Start, Beat End)> overlapIntervals)
    {
        foreach (var evt in eventSource)
        {
            if (!TouchesAnyOverlap(evt, overlapIntervals))
            {
                var offset = GetPreviousEndValue(offsetLookup, evt.StartBeat);
                newEvents.Add(CreateEventWithOffset(evt, evt.StartBeat, evt.EndBeat,
                    evt.StartValue, evt.EndValue, offset));
            }
            else
            {
                // 事件与重叠区间有交叉：补出超出重叠区间的片段
                foreach (var (gapStart, gapEnd) in GapsOutsideOverlap(evt, overlapIntervals))
                {
                    var offset = GetPreviousEndValue(offsetLookup, gapStart);
                    newEvents.Add(CreateEventWithOffset(evt, gapStart, gapEnd,
                        evt.GetValueAtBeat(gapStart), evt.GetValueAtBeat(gapEnd), offset));
                }
            }
        }
    }

    /// <summary>
    /// 创建一个新事件：时间范围为 [<paramref name="startBeat"/>, <paramref name="endBeat"/>]，
    /// 起/终值分别为 <paramref name="startValue"/>、<paramref name="endValue"/> 与 <paramref name="offset"/> 之和，
    /// 缓动属性（Easing、EasingLeft、EasingRight、IsBezier、BezierPoints）完整复制自 <paramref name="sourceEvent"/>。
    /// </summary>
    /// <param name="sourceEvent">缓动属性的来源事件。</param>
    /// <param name="startBeat">新事件的起始拍。</param>
    /// <param name="endBeat">新事件的结束拍。</param>
    /// <param name="startValue">叠加偏移前的起点值。</param>
    /// <param name="endValue">叠加偏移前的终点值。</param>
    /// <param name="offset">来自对方轨道的前序偏移值（以加法叠加）。</param>
    /// <returns>起/终值已叠加偏移、缓动属性已克隆的新事件实例。</returns>
    private static Kpc.Event<TPayload> CreateEventWithOffset(
        Kpc.Event<TPayload> sourceEvent, Beat startBeat, Beat endBeat,
        TPayload? startValue, TPayload? endValue, TPayload? offset)
        => new()
        {
            StartBeat = startBeat,
            EndBeat = endBeat,
            StartValue = (dynamic?)startValue + (dynamic?)offset,
            EndValue = (dynamic?)endValue + (dynamic?)offset,
            BezierPoints = sourceEvent.BezierPoints,
            Easing = sourceEvent.Easing,
            EasingLeft = sourceEvent.EasingLeft,
            EasingRight = sourceEvent.EasingRight,
            IsBezier = sourceEvent.IsBezier,
        };

    /// <summary>
    /// 判断 <paramref name="evt"/> 的时间范围是否与 <paramref name="overlapIntervals"/> 中任意区间存在时间交叉。
    /// </summary>
    /// <param name="evt">待判断的事件。</param>
    /// <param name="overlapIntervals">重叠区间集合。</param>
    /// <returns>存在交叉（即 <c>evt.StartBeat &lt; iv.End &amp;&amp; evt.EndBeat &gt; iv.Start</c>）时返回 <see langword="true"/>。</returns>
    private static bool TouchesAnyOverlap(
        Kpc.Event<TPayload> evt, List<(Beat Start, Beat End)> overlapIntervals)
        => overlapIntervals.Any(iv => evt.StartBeat < iv.End && evt.EndBeat > iv.Start);

    /// <summary>
    /// 以惰性迭代方式返回 <paramref name="evt"/> 在 <paramref name="overlapIntervals"/> 所有区间之外的时间片段（"空隙"）。
    /// <para>
    /// 算法：以游标 <c>cursor</c> 从事件起点开始，按 Start 升序遍历各重叠区间：
    /// 若重叠区间起点落在游标与事件终点之间，则输出 [cursor, iv.Start]；
    /// 然后将游标推进到 <c>max(cursor, iv.End)</c>；超过事件终点时提前退出。
    /// 遍历结束后若游标仍在事件终点之前，则输出 [cursor, evt.EndBeat]。
    /// </para>
    /// </summary>
    /// <param name="evt">需要计算空隙的源事件。</param>
    /// <param name="overlapIntervals">已按 Start 升序排序的重叠区间集合。</param>
    /// <returns>按时间顺序枚举的空隙区间序列；若事件完全处于重叠区间内则枚举为空。</returns>
    private static IEnumerable<(Beat Start, Beat End)> GapsOutsideOverlap(
        Kpc.Event<TPayload> evt, List<(Beat Start, Beat End)> overlapIntervals)
    {
        var cursor = evt.StartBeat;
        foreach (var iv in overlapIntervals)
        {
            if (iv.Start > cursor && iv.Start < evt.EndBeat)
                yield return (cursor, iv.Start); // 空隙在重叠区间左侧
            if (iv.End > cursor)
                cursor = iv.End; // 推进游标越过当前重叠区间
            if (cursor >= evt.EndBeat) yield break;
        }

        if (cursor < evt.EndBeat)
            yield return (cursor, evt.EndBeat); // 末尾剩余空隙
    }

    /// <summary>
    /// 对 <paramref name="overlapIntervals"/> 中的每个重叠区间，分别在两轨道上执行等间隔切片，
    /// 并从各自的拷贝列表中移除已被切片覆盖的原始事件（避免后续重复使用）。
    /// </summary>
    /// <param name="toEventsCopy">目标轨道事件深拷贝（将被原地移除重叠段事件）。</param>
    /// <param name="fromEventsCopy">来源轨道事件深拷贝（将被原地移除重叠段事件）。</param>
    /// <param name="overlapIntervals">已归并并排序的重叠区间集合。</param>
    /// <param name="cutLength">每个切片的拍长（= 1 / precision）。</param>
    /// <returns>两组切片事件列表的元组：<c>CutTo</c> 对应目标轨道，<c>CutFrom</c> 对应来源轨道。</returns>
    private static (List<Kpc.Event<TPayload>> CutTo, List<Kpc.Event<TPayload>> CutFrom) CutAndRemoveOverlapEvents(
        List<Kpc.Event<TPayload>> toEventsCopy,
        List<Kpc.Event<TPayload>> fromEventsCopy,
        List<(Beat Start, Beat End)> overlapIntervals,
        Beat cutLength)
    {
        var cutTo = new List<Kpc.Event<TPayload>>();
        var cutFrom = new List<Kpc.Event<TPayload>>();
        foreach (var (start, end) in overlapIntervals)
        {
            cutTo.AddRange(Cutter.CutEventsInRange(toEventsCopy, start, end, cutLength));
            cutFrom.AddRange(Cutter.CutEventsInRange(fromEventsCopy, start, end, cutLength));
            toEventsCopy.RemoveAll(e => e.StartBeat < end && e.EndBeat > start);
            fromEventsCopy.RemoveAll(e => e.StartBeat < end && e.EndBeat > start);
        }

        return (cutTo, cutFrom);
    }

    /// <summary>
    /// 遍历所有重叠区间，逐个调用 <see cref="MergeSingleOverlapInterval"/> 合并对应的切片事件，
    /// 并为每个区间查歧入口处的两轨道前序终值，作为区间内缺失切片的延续值。
    /// </summary>
    /// <param name="toEventsCopy">目标轨道事件深拷贝（已移除重叠段），用于查询各区间入口的前序终值。</param>
    /// <param name="fromEventsCopy">来源轨道事件深拷贝（已移除重叠段），用于查询各区间入口的前序终值。</param>
    /// <param name="cutTo">目标轨道在各重叠区间内的切片事件。</param>
    /// <param name="cutFrom">来源轨道在各重叠区间内的切片事件。</param>
    /// <param name="overlapIntervals">已归并并排序的重叠区间集合。</param>
    /// <param name="cutLength">每个切片的拍长。</param>
    /// <returns>所有重叠区间合并后的事件列表（未排序）。</returns>
    private static List<Kpc.Event<TPayload>> MergeCutOverlapSegments(
        List<Kpc.Event<TPayload>> toEventsCopy,
        List<Kpc.Event<TPayload>> fromEventsCopy,
        List<Kpc.Event<TPayload>> cutTo,
        List<Kpc.Event<TPayload>> cutFrom,
        List<(Beat Start, Beat End)> overlapIntervals,
        Beat cutLength)
    {
        var allCutEvents = new List<Kpc.Event<TPayload>>();
        foreach (var (start, end) in overlapIntervals)
        {
            var prevTo = toEventsCopy.FindLast(e => e.EndBeat <= start);
            var prevForm = fromEventsCopy.FindLast(e => e.EndBeat <= start);
            var toLastEnd = prevTo != null ? prevTo.EndValue : default;
            var formLastEnd = prevForm != null ? prevForm.EndValue : default;
            allCutEvents.AddRange(
                MergeSingleOverlapInterval(cutTo, cutFrom, start, end, cutLength, toLastEnd, formLastEnd));
        }

        return allCutEvents;
    }

    /// <summary>
    /// 在单个重叠区间 [<paramref name="start"/>, <paramref name="end"/>] 内，
    /// 按 <paramref name="cutLength"/> 步长遍历切片，将两轨道对应切片的起/终值逐片相加，生成合并事件。
    /// <para>
    /// 若某一轨道在当前切片位置无对应事件（即该切片未出现在 <paramref name="cutTo"/> 或 <paramref name="cutFrom"/> 中），
    /// 则以该轨道进入本区间时记录的上一终值（<paramref name="toLastEndValue"/> 或 <paramref name="formLastEndValue"/>）作为该切片的常量值延续；
    /// 并在处理完每片后更新对应的"上一终值"供下一切片使用。
    /// </para>
    /// </summary>
    /// <param name="cutTo">目标轨道在本区间内的切片事件列表（StartBeat/EndBeat 精确匹配）。</param>
    /// <param name="cutFrom">来源轨道在本区间内的切片事件列表（StartBeat/EndBeat 精确匹配）。</param>
    /// <param name="start">区间起始拍。</param>
    /// <param name="end">区间结束拍。</param>
    /// <param name="cutLength">切片拍长（= 1 / precision）。</param>
    /// <param name="toLastEndValue">目标轨道进入本区间之前最后一个事件的终值；用于填补缺失切片。</param>
    /// <param name="formLastEndValue">来源轨道进入本区间之前最后一个事件的终值；用于填补缺失切片。</param>
    /// <returns>本区间内逐片求和后的合并事件列表。</returns>
    private static List<Kpc.Event<TPayload>> MergeSingleOverlapInterval(
        List<Kpc.Event<TPayload>> cutTo,
        List<Kpc.Event<TPayload>> cutFrom,
        Beat start, Beat end, Beat cutLength,
        TPayload? toLastEndValue, TPayload? formLastEndValue)
    {
        var merged = new List<Kpc.Event<TPayload>>();
        var currentBeat = start;
        while (currentBeat < end)
        {
            var nextBeat = currentBeat + cutLength;
            if (nextBeat > end) nextBeat = end;

            var (toStart, toEnd, toNewLast) = ResolveSegmentValues(cutTo, currentBeat, nextBeat, toLastEndValue);
            var (formStart, formEnd, formNewLast) =
                ResolveSegmentValues(cutFrom, currentBeat, nextBeat, formLastEndValue);

            merged.Add(new Kpc.Event<TPayload>
            {
                StartBeat = currentBeat,
                EndBeat = nextBeat,
                StartValue = (dynamic?)toStart + (dynamic?)formStart,
                EndValue = (dynamic?)toEnd + (dynamic?)formEnd,
            });

            toLastEndValue = toNewLast;
            formLastEndValue = formNewLast;
            currentBeat = nextBeat;
        }

        return merged;
    }

    /// <summary>
    /// 在 <paramref name="cutEvents"/> 中查找 StartBeat 与 EndBeat 分别精确匹配
    /// <paramref name="currentBeat"/> 和 <paramref name="nextBeat"/> 的切片事件，
    /// 并据此返回该切片的起/终值和更新后的"上一终值"。
    /// <para>若未找到匹配切片，则三个返回值均为 <paramref name="lastEndValue"/>（值延续语义）。</para>
    /// </summary>
    /// <param name="cutEvents">目标或来源轨道的切片事件池。</param>
    /// <param name="currentBeat">期望切片的起始拍。</param>
    /// <param name="nextBeat">期望切片的结束拍。</param>
    /// <param name="lastEndValue">当前切片缺失时用于延续的前序终值。</param>
    /// <returns>
    /// 元组 <c>(Start, End, NewLastEnd)</c>：
    /// <c>Start</c> 为该切片起点值，<c>End</c> 为终点值，<c>NewLastEnd</c> 为处理完本切片后应记录的新"上一终值"。
    /// </returns>
    private static (TPayload? Start, TPayload? End, TPayload? NewLastEnd) ResolveSegmentValues(
        List<Kpc.Event<TPayload>> cutEvents, Beat currentBeat, Beat nextBeat, TPayload? lastEndValue)
    {
        var evt = cutEvents.FirstOrDefault(e => e.StartBeat == currentBeat && e.EndBeat == nextBeat);
        if (evt is null) return (lastEndValue, lastEndValue, lastEndValue);
        return (evt.StartValue, evt.EndValue, evt.EndValue);
    }

    #endregion

    #region 自适应采样路径

    /// <summary>
    /// 对存在时间重叠的两轨道事件执行自适应采样合并。
    /// <para>流程：① 构建重叠区间列表；② 生成重叠区间外的基础事件；
    /// ③ 对重叠区间执行自适应采样（见 <see cref="MergeAdaptiveSingleInterval"/>）；④ 合并并排序。</para>
    /// </summary>
    /// <param name="toEventsForOffsetLookup">目标轨道原始事件序列，仅用于非重叠区间的偏移查询，不会被修改。</param>
    /// <param name="toEventsCopy">目标轨道事件的深拷贝。</param>
    /// <param name="fromEventsCopy">来源轨道事件的深拷贝。</param>
    /// <param name="precision">自适应采样的最大切片粒度（每拍切片数）。</param>
    /// <param name="tolerance">允许的线性近似误差百分比。</param>
    /// <returns>合并后按 StartBeat 升序排列的新事件列表。</returns>
    private static List<Kpc.Event<TPayload>> MergeWithOverlapAdaptiveSampling(
        List<Kpc.Event<TPayload>> toEventsForOffsetLookup,
        List<Kpc.Event<TPayload>> toEventsCopy,
        List<Kpc.Event<TPayload>> fromEventsCopy,
        double precision,
        double tolerance)
    {
        var overlapIntervals = BuildOverlapIntervals(toEventsCopy, fromEventsCopy);
        var newEvents = BuildBaseEventsOutsideOverlap(
            toEventsCopy, fromEventsCopy, toEventsForOffsetLookup, overlapIntervals);

        newEvents.AddRange(
            MergeAdaptiveIntervals(toEventsCopy, fromEventsCopy, overlapIntervals, precision, tolerance));

        SortByStartBeat(newEvents);
        return newEvents;
    }

    /// <summary>
    /// 遍历所有重叠区间，逐个调用 <see cref="MergeAdaptiveSingleInterval"/> 执行自适应合并，汇总结果。
    /// </summary>
    /// <param name="toEventsCopy">目标轨道事件深拷贝，已按 StartBeat 排序。</param>
    /// <param name="fromEventsCopy">来源轨道事件深拷贝，已按 StartBeat 排序。</param>
    /// <param name="overlapIntervals">已归并并按 Start 升序排序的重叠区间集合。</param>
    /// <param name="precision">自适应采样的最大切片粒度（每拍切片数）；转换为拍长 = 1 / precision。</param>
    /// <param name="tolerance">允许的线性近似误差百分比。</param>
    /// <returns>所有重叠区间自适应合并后的事件列表（未排序）。</returns>
    private static List<Kpc.Event<TPayload>> MergeAdaptiveIntervals(
        List<Kpc.Event<TPayload>> toEventsCopy,
        List<Kpc.Event<TPayload>> fromEventsCopy,
        List<(Beat Start, Beat End)> overlapIntervals,
        double precision,
        double tolerance)
    {
        var cutLength = new Beat(1d / precision);
        var result = new List<Kpc.Event<TPayload>>();
        foreach (var (start, end) in overlapIntervals)
        {
            result.AddRange(
                MergeAdaptiveSingleInterval(toEventsCopy, fromEventsCopy, start, end, cutLength, tolerance));
        }

        return result;
    }

    /// <summary>
    /// 在单个重叠区间内以 <paramref name="cutLength"/> 为最大步长执行自适应采样合并。
    /// <para>
    /// 算法：以 <paramref name="cutLength"/> 为步进逐拍推进 <c>currentBeat</c>，
    /// 在每个采样点评估两轨道的瞬时叠加值。满足以下任一条件时立即提交当前段并开启新段：
    /// <list type="bullet">
    ///   <item>当前步跨越了任意一轨道的事件边界（<c>crossEvent == true</c>）；</item>
    ///   <item>当前段的线性近似误差（归一化欧几里得垂直距离）超过 <paramref name="tolerance"/>（见 <see cref="ShouldSplitAdaptiveSegment"/>）；</item>
    ///   <item>到达区间终点。</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="toEventsCopy">目标轨道事件深拷贝，已按 StartBeat 排序。</param>
    /// <param name="fromEventsCopy">来源轨道事件深拷贝，已按 StartBeat 排序。</param>
    /// <param name="start">区间起始拍。</param>
    /// <param name="end">区间结束拍。</param>
    /// <param name="cutLength">采样步长（= 1 / precision）。</param>
    /// <param name="tolerance">允许的线性近似误差百分比（0–100）。</param>
    /// <returns>本区间内自适应采样后的合并事件列表。</returns>
    private static List<Kpc.Event<TPayload>> MergeAdaptiveSingleInterval(
        List<Kpc.Event<TPayload>> toEventsCopy,
        List<Kpc.Event<TPayload>> fromEventsCopy,
        Beat start, Beat end, Beat cutLength, double tolerance)
    {
        var result = new List<Kpc.Event<TPayload>>();
        var currentBeat = start;

        var lastToValue = GetPreviousEndValue(toEventsCopy, start);
        var lastFormValue = GetPreviousEndValue(fromEventsCopy, start);

        var toEventAtCurrent = GetActiveEventAtBeat(toEventsCopy, currentBeat);
        var formEventAtCurrent = GetActiveEventAtBeat(fromEventsCopy, currentBeat);
        var toValAtCurrent = toEventAtCurrent != null ? toEventAtCurrent.GetValueAtBeat(currentBeat) : lastToValue;
        var formValAtCurrent =
            formEventAtCurrent != null ? formEventAtCurrent.GetValueAtBeat(currentBeat) : lastFormValue;

        var segmentStart = start;
        var segmentStartToValue = toValAtCurrent;
        var segmentStartFormValue = formValAtCurrent;
        var segmentStartSum = AddValues(segmentStartToValue, segmentStartFormValue);

        while (currentBeat < end)
        {
            var nextBeat = currentBeat + cutLength;
            if (nextBeat > end) nextBeat = end;

            var toEventAtNext = GetActiveEventAtBeat(toEventsCopy, nextBeat);
            var formEventAtNext = GetActiveEventAtBeat(fromEventsCopy, nextBeat);
            var crossEvent = !ReferenceEquals(toEventAtNext, toEventAtCurrent) ||
                             !ReferenceEquals(formEventAtNext, formEventAtCurrent);

            if (crossEvent && currentBeat > segmentStart)
            {
                AddSegmentEvent(result, segmentStart, currentBeat,
                    segmentStartToValue, segmentStartFormValue, toValAtCurrent, formValAtCurrent);
                segmentStart = currentBeat;
                segmentStartToValue = toValAtCurrent;
                segmentStartFormValue = formValAtCurrent;
                segmentStartSum = AddValues(toValAtCurrent, formValAtCurrent);
            }

            var (toValueAtNext, toValUpdate) =
                GetNextBeatValues(toEventsCopy, toEventAtCurrent, toEventAtNext, nextBeat);
            var (formValueAtNext, formValUpdate) =
                GetNextBeatValues(fromEventsCopy, formEventAtCurrent, formEventAtNext, nextBeat);

            var sumAtNext = AddValues(toValueAtNext, formValueAtNext);
            var sumAtEnd = AddValues(GetValueAtBeatOrPreviousEnd(toEventsCopy, end),
                GetValueAtBeatOrPreviousEnd(fromEventsCopy, end));

            if (crossEvent || ShouldSplitAdaptiveSegment(
                    segmentStart, nextBeat, end, segmentStartSum, sumAtNext, sumAtEnd, tolerance))
            {
                AddSegmentEvent(result, segmentStart, nextBeat,
                    segmentStartToValue, segmentStartFormValue, toValueAtNext, formValueAtNext);
                segmentStart = nextBeat;
                segmentStartToValue = toValUpdate;
                segmentStartFormValue = formValUpdate;
                segmentStartSum = AddValues(toValUpdate, formValUpdate);
            }

            toEventAtCurrent = toEventAtNext;
            formEventAtCurrent = formEventAtNext;
            toValAtCurrent = toValUpdate;
            formValAtCurrent = formValUpdate;
            currentBeat = nextBeat;
        }

        return result;
    }

    /// <summary>
    /// 获取指定拍点处处于激活状态且起始拍最晚的事件。
    /// 使用二分查找，O(logN) 复杂度。要求 events 已按 StartBeat 排序。
    /// </summary>
    private static Kpc.Event<TPayload>? GetActiveEventAtBeat(List<Kpc.Event<TPayload>> events, Beat beat)
    {
        if (events.Count == 0) return null;

        // 二分查找最后一个 StartBeat <= beat 的事件
        var lo = 0;
        var hi = events.Count - 1;
        var result = -1;

        while (lo <= hi)
        {
            var mid = lo + (hi - lo) / 2;
            if (events[mid].StartBeat <= beat)
            {
                result = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        // 从 result 开始向前找，找到第一个 EndBeat >= beat 的事件
        for (var i = result; i >= 0; i--)
        {
            if (events[i].EndBeat >= beat)
                return events[i];
        }

        return null;
    }

    /// <summary>
    /// 在已按 StartBeat 排序的 <paramref name="events"/> 中，
    /// 使用二分查找找到最后一个 <c>EndBeat &lt;= beat</c> 的事件并返回其 EndValue。
    /// 时间复杂度 O(logN)。
    /// </summary>
    /// <param name="events">已按 StartBeat 升序排序的事件列表（要求事件在轨道内不相互重叠，使得 EndBeat 与 StartBeat 同向有序）。</param>
    /// <param name="beat">查询拍点。</param>
    /// <returns>在 <paramref name="beat"/> 之前已结束的最后一个事件的终值；不存在时返回 <see langword="default"/>。</returns>
    private static TPayload? GetPreviousEndValue(List<Kpc.Event<TPayload>> events, Beat beat)
    {
        if (events.Count == 0) return default;

        // 二分查找最后一个 EndBeat <= beat 的事件
        var lo = 0;
        var hi = events.Count - 1;
        var result = -1;

        while (lo <= hi)
        {
            var mid = lo + (hi - lo) / 2;
            if (events[mid].EndBeat <= beat)
            {
                result = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return result >= 0 ? events[result].EndValue : default;
    }

    /// <summary>
    /// 获取 <paramref name="beat"/> 处的轨道瞬时值：
    /// 若存在覆盖该拍点的活动事件则调用 <see cref="Kpc.Event{TPayload}.GetValueAtBeat"/> 插值，
    /// 否则回退到 <see cref="GetPreviousEndValue"/> 返回的前序终值。
    /// </summary>
    /// <param name="events">已按 StartBeat 升序排序的事件列表。</param>
    /// <param name="beat">查询拍点。</param>
    /// <returns>该拍点处轨道的瞬时值。</returns>
    private static TPayload? GetValueAtBeatOrPreviousEnd(List<Kpc.Event<TPayload>> events, Beat beat)
    {
        var active = GetActiveEventAtBeat(events, beat);
        return active != null ? active.GetValueAtBeat(beat) : GetPreviousEndValue(events, beat);
    }

    /// <summary>
    /// 计算 <paramref name="nextBeat"/> 处当前事件的出站值（Outgoing）与下一事件的入站值（Incoming），
    /// 用于在事件边界处正确处理值的过渡。
    /// <list type="bullet">
    ///   <item><b>Outgoing</b>：若 <paramref name="eventAtCurrent"/> 延伸到 <paramref name="nextBeat"/> 或更晚，
    ///     则为该事件在 <paramref name="nextBeat"/> 处的插值；否则为 <paramref name="nextBeat"/> 的前序终值。</item>
    ///   <item><b>Incoming</b>：若 <paramref name="eventAtNext"/> 非空，则为该事件在 <paramref name="nextBeat"/> 处的插值；
    ///     否则与 Outgoing 相同（均为前序终值）。</item>
    /// </list>
    /// </summary>
    /// <param name="events">事件列表，用于查询前序终值。</param>
    /// <param name="eventAtCurrent">当前拍点的活动事件（可为 <see langword="null"/>）。</param>
    /// <param name="eventAtNext">下一拍点的活动事件（可为 <see langword="null"/>）。</param>
    /// <param name="nextBeat">下一拍点。</param>
    /// <returns>
    /// 元组 <c>(Outgoing, Incoming)</c>：<c>Outgoing</c> 为当前事件延续到 <paramref name="nextBeat"/> 的出站值，
    /// <c>Incoming</c> 为下一拍点活动事件的入站值。
    /// </returns>
    private static (TPayload? Outgoing, TPayload? Incoming) GetNextBeatValues(
        List<Kpc.Event<TPayload>> events, Kpc.Event<TPayload>? eventAtCurrent, Kpc.Event<TPayload>? eventAtNext,
        Beat nextBeat)
    {
        var prevEnd = GetPreviousEndValue(events, nextBeat);
        var outgoing = (eventAtCurrent != null && eventAtCurrent.EndBeat >= nextBeat)
            ? eventAtCurrent.GetValueAtBeat(nextBeat)
            : prevEnd;
        var incoming = eventAtNext != null
            ? eventAtNext.GetValueAtBeat(nextBeat)
            : prevEnd;
        return (outgoing, incoming);
    }

    /// <summary>
    /// 在归一化的 (时间, 值) 二维空间中，计算测试点到理想线段的垂直距离，
    /// 判断自适应分段是否需要在 <paramref name="nextBeat"/> 处切分。
    /// <para>
    /// 具体方法：将时间轴归一化到 [0, 1]（以 [<paramref name="segmentStart"/>, <paramref name="intervalEnd"/>] 为区间），
    /// 以段内值域最大绝对值（与 1e-9 取较大值）为比例尺对值轴归一化，
    /// 计算点 (<paramref name="nextBeat"/>, <paramref name="sumAtNext"/>) 到连接
    /// (<paramref name="segmentStart"/>, <paramref name="segmentStartSum"/>) 与
    /// (<paramref name="intervalEnd"/>, <paramref name="sumAtEnd"/>) 的线段的欧几里得垂直距离，
    /// 与 <paramref name="tolerance"/>/100 阈值比较。
    /// </para>
    /// <para>当 <paramref name="nextBeat"/> 到达或超过 <paramref name="intervalEnd"/> 时强制返回 <see langword="true"/>。</para>
    /// </summary>
    /// <param name="segmentStart">当前分段的起始拍。</param>
    /// <param name="nextBeat">待评估的拍点。</param>
    /// <param name="intervalEnd">所在重叠区间的结束拍。</param>
    /// <param name="segmentStartSum">当前分段起点处两轨道叠加值。</param>
    /// <param name="sumAtNext"><paramref name="nextBeat"/> 处两轨道叠加值。</param>
    /// <param name="sumAtEnd"><paramref name="intervalEnd"/> 处两轨道叠加值。</param>
    /// <param name="tolerance">允许的线性近似误差百分比（0–100）；负值按 0 处理。</param>
    /// <returns>垂直距离超过阈值或到达区间终点时返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    private static bool ShouldSplitAdaptiveSegment(
        Beat segmentStart, Beat nextBeat, Beat intervalEnd,
        TPayload? segmentStartSum, TPayload? sumAtNext, TPayload? sumAtEnd, double tolerance)
    {
        if (nextBeat >= intervalEnd) return true;
        if (nextBeat <= segmentStart) return false;

        var dtTotal = (double)(intervalEnd - segmentStart);
        var dtLocal = (double)(nextBeat - segmentStart);
        if (dtTotal <= 1e-12 || dtLocal <= 1e-12) return false;

        var p = Math.Clamp(dtLocal / dtTotal, 0.0, 1.0);

        var startNum = ToDouble(segmentStartSum);
        var nextNum = ToDouble(sumAtNext);
        var endNum = ToDouble(sumAtEnd);

        // 归一化比例尺：避免量纲混用
        var scale = Math.Max(Math.Max(Math.Abs(startNum), Math.Abs(endNum)), 1e-9);

        // 归一化 (时间, 值) 空间中的垂直距离：
        //   A'=(0, 0), C'=(1, dvNorm), 测试点 B'=(p, byNorm)
        //   d = |byNorm − dvNorm·p| / sqrt(1 + dvNorm²)
        // 当 dvNorm≈0（水平段）退化为纯值域偏差，当 dvNorm 很大（陡峭段）退化为时间偏差，
        // 两者通过欧几里得范数自然融合，无需手动加权。
        var dvNorm = (endNum - startNum) / scale;
        var byNorm = (nextNum - startNum) / scale;
        var det = byNorm - dvNorm * p;
        var len = Math.Sqrt(1.0 + dvNorm * dvNorm);
        var normalizedDist = Math.Abs(det) / len;

        return normalizedDist > Math.Max(0d, tolerance) / 100.0;
    }

    /// <summary>
    /// 将 <paramref name="value"/> 安全地转换为 <see cref="double"/>。
    /// </summary>
    /// <param name="value">待转换的动态数值；为 <see langword="null"/> 时抛出异常。</param>
    /// <returns>转换后的双精度浮点值。</returns>
    /// <exception cref="InvalidOperationException"><paramref name="value"/> 为 <see langword="null"/>。</exception>
    private static double ToDouble(dynamic? value)
    {
        if (value == null)
            throw new InvalidOperationException("Unexpected null numeric value.");
        return (double)value;
    }

    /// <summary>
    /// 对 <paramref name="left"/> 与 <paramref name="right"/> 执行动态加法并返回结果。
    /// </summary>
    /// <param name="left">左操作数（可为 <see langword="null"/>；由运行时动态绑定处理）。</param>
    /// <param name="right">右操作数（可为 <see langword="null"/>；由运行时动态绑定处理）。</param>
    /// <returns>两值相加的结果，类型为 <typeparamref name="TPayload"/>。</returns>
    private static TPayload AddValues(TPayload? left, TPayload? right)
        => (dynamic?)left + (dynamic?)right;

    /// <summary>
    /// 将一个由两轨道起/终值叠加得到的分段事件追加到 <paramref name="target"/>。
    /// 起点值为 <paramref name="startToValue"/> + <paramref name="startFormValue"/>，
    /// 终点值为 <paramref name="endToValue"/> + <paramref name="endFormValue"/>。
    /// 不复制缓动属性，生成的事件为纯线性段。
    /// </summary>
    /// <param name="target">目标事件列表。</param>
    /// <param name="startBeat">分段起始拍。</param>
    /// <param name="endBeat">分段结束拍。</param>
    /// <param name="startToValue">目标轨道在起点处的值。</param>
    /// <param name="startFormValue">来源轨道在起点处的值。</param>
    /// <param name="endToValue">目标轨道在终点处的值。</param>
    /// <param name="endFormValue">来源轨道在终点处的值。</param>
    private static void AddSegmentEvent(
        List<Kpc.Event<TPayload>> target, Beat startBeat, Beat endBeat,
        TPayload? startToValue, TPayload? startFormValue, TPayload? endToValue, TPayload? endFormValue)
    {
        target.Add(new Kpc.Event<TPayload>
        {
            StartBeat = startBeat,
            EndBeat = endBeat,
            StartValue = AddValues(startToValue, startFormValue),
            EndValue = AddValues(endToValue, endFormValue),
        });
    }

    #endregion
}