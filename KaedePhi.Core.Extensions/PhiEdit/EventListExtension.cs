using KaedePhi.Core.PhiEdit;

namespace KaedePhi.Core.Extensions.PhiEdit;

public static class EventListExtension
{
    extension(List<Event> eventList)
    {
        /// <summary>
        /// 将事件插入到列表
        /// 使用前必须确保列表已经按照StartBeat升序排序
        /// </summary>
        /// <param name="event">事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public void AppendEvent(Event @event)
        {
            ArgumentNullException.ThrowIfNull(eventList);
            ArgumentNullException.ThrowIfNull(@event);
            var index = eventList.FindIndex(e => e.StartBeat > @event.StartBeat);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }
    }

    extension(List<MoveEvent> eventList)
    {
        /// <summary>
        /// 将移动事件插入到列表
        /// 使用前必须确保列表已经按照StartBeat升序排序
        /// </summary>
        /// <param name="event">移动事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public void AppendMoveEvent(MoveEvent @event)
        {
            ArgumentNullException.ThrowIfNull(eventList);
            ArgumentNullException.ThrowIfNull(@event);
            var index = eventList.FindIndex(e => e.StartBeat > @event.StartBeat);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }
    }
}
