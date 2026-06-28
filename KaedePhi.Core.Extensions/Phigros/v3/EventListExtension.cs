using System;
using System.Collections.Generic;
using KaedePhi.Core.Phigros.v3;

namespace KaedePhi.Core.Extensions.Phigros.v3
{
    public static class EventListExtension
    {
        /// <summary>
        /// 将事件插入到列表
        /// 使用前必须确保列表已经按照StartTime升序排序
        /// </summary>
        /// <param name="event">事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public static void AppendEvent<T>(this List<T> eventList, T @event)
            where T : Event
        {
            if (eventList is null)
                throw new ArgumentNullException(nameof(eventList));
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            var index = eventList.FindIndex(e => e.StartTime > @event.StartTime);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }

        /// <summary>
        /// 将速度事件插入到列表
        /// 使用前必须确保列表已经按照StartTime升序排序
        /// </summary>
        /// <param name="event">速度事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public static void AppendSpeedEvent<T>(this List<T> eventList, T @event)
            where T : SpeedEvent
        {
            if (eventList is null)
                throw new ArgumentNullException(nameof(eventList));
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            var index = eventList.FindIndex(e => e.StartTime > @event.StartTime);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }
    }
}
