using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Event;

public interface IEventFit<TEvent> : ILoggable
{
    /// <summary>
    /// 将事件列表中连续的线性事件序列拟合为带缓动函数的事件。
    /// </summary>
    /// <param name="events">待处理的事件列表，允许为 null 或空列表。</param>
    /// <param name="tolerance">
    /// 容差百分比，取值范围 [0, 100]。
    /// 例如 0.1 表示允许每个采样点的偏差不超过整段值变化量的 0.1%。
    /// </param>
    /// <returns>拟合后的事件列表。</returns>
    List<TEvent> FitEvents(List<TEvent>? events, double tolerance);
}
