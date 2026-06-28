using System;
using System.Collections.Generic;
using KaedePhi.Core.Common;
using KaedePhi.Core.PhiFans;

namespace KaedePhi.Core.Extensions.PhiFans
{
    public static class EventListExtension
    {
        /// <summary>
        /// 将事件插入到列表
        /// 使用前必须确保列表已经按照Beat升序排序
        /// </summary>
        /// <param name="eventList">事件列表</param>
        /// <param name="event">事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public static void AppendEvent<T>(this List<T> eventList, T @event)
            where T : Event
        {
            if (eventList is null)
                throw new ArgumentNullException(nameof(eventList));
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            var index = eventList.FindIndex(e => e.Beat > @event.Beat);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }
    }
}
