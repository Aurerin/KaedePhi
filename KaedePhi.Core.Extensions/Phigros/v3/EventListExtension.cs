using KaedePhi.Core.Phigros.v3;

namespace KaedePhi.Core.Extensions.Phigros.v3;

public static class EventListExtension
{
    extension<T>(List<T> eventList)
        where T : Event
    {
        /// <summary>
        /// 将事件插入到列表
        /// 使用前必须确保列表已经按照StartTime升序排序
        /// </summary>
        /// <param name="event">事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public void AppendEvent(T @event)
        {
            ArgumentNullException.ThrowIfNull(eventList);
            ArgumentNullException.ThrowIfNull(@event);
            var index = eventList.FindIndex(e => e.StartTime > @event.StartTime);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }
    }

    extension<T>(List<T> eventList)
        where T : SpeedEvent
    {
        /// <summary>
        /// 将速度事件插入到列表
        /// 使用前必须确保列表已经按照StartTime升序排序
        /// </summary>
        /// <param name="event">速度事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public void AppendSpeedEvent(T @event)
        {
            ArgumentNullException.ThrowIfNull(eventList);
            ArgumentNullException.ThrowIfNull(@event);
            var index = eventList.FindIndex(e => e.StartTime > @event.StartTime);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }
    }
}
