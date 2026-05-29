using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Event;

/// <summary>
/// 定义将两组事件列表按值叠加合并的操作契约。
/// <para>
/// 合并语义：目标轨道（<c>toEvents</c>）与来源轨道（<c>fromEvents</c>）的值在时间轴上逐拍相加。
/// 非重叠区段以对方轨道最近已结束事件的终值作为常量偏移直接叠加；
/// 重叠区段根据实现策略（固定采样或自适应采样）将两轨道的瞬时值求和后生成分段线性近似。
/// </para>
/// </summary>
/// <typeparam name="TEvent">事件类型。</typeparam>
public interface IEventListMerger<TEvent> : ILoggable
{
    /// <summary>
    /// 将 <paramref name="fromEvents"/> 叠加到 <paramref name="toEvents"/> 上，返回合并后的新事件列表。
    /// <para>
    /// 对于两轨道无时间重叠的区段，以对方轨道最近一个已结束事件的终值作为常量偏移；
    /// 对于重叠区段，将两轨道的事件按 <paramref name="precision"/> 等间隔切片后逐片求和，
    /// 生成分段线性近似。切片精度越高，结果越精确，但产生的事件数量也越多。
    /// </para>
    /// <para>任意一个输入列表为 <see langword="null"/> 或空时直接返回另一列表的深拷贝；两者均为空时返回空列表。</para>
    /// </summary>
    /// <param name="toEvents">目标轨道事件列表；为 <see langword="null"/> 或空时直接返回 <paramref name="fromEvents"/> 的克隆。</param>
    /// <param name="fromEvents">来源轨道事件列表；为 <see langword="null"/> 或空时直接返回 <paramref name="toEvents"/> 的克隆。</param>
    /// <param name="precision">重叠区段的切片精度（每拍切片数）；值越大精度越高，事件数量也越多。</param>
    /// <returns>叠加后的新事件列表，已按起始拍升序排序。</returns>
    List<TEvent> EventListMerge(
        List<TEvent>? toEvents,
        List<TEvent>? fromEvents,
        double precision);
}