using System;
using System.Collections.Generic;
using KaedePhi.Core.PhiEdit;

namespace KaedePhi.Core.Extensions.PhiEdit
{
    public static class EventListExtension
    {
        /// <summary>
        /// 将事件插入到列表
        /// 使用前必须确保列表已经按照StartBeat升序排序
        /// </summary>
        /// <param name="eventList">事件列表</param>
        /// <param name="event">事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public static void AppendEvent(this List<Event> eventList, Event @event)
        {
            if (eventList is null)
                throw new ArgumentNullException(nameof(eventList));
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            var index = eventList.FindIndex(e => e.StartBeat > @event.StartBeat);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }

        /// <summary>
        /// 将移动事件插入到列表
        /// 使用前必须确保列表已经按照StartBeat升序排序
        /// </summary>
        /// <param name="eventList">事件列表</param>
        /// <param name="event">移动事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public static void AppendMoveEvent(this List<MoveEvent> eventList, MoveEvent @event)
        {
            if (eventList is null)
                throw new ArgumentNullException(nameof(eventList));
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            var index = eventList.FindIndex(e => e.StartBeat > @event.StartBeat);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }
    }
}
