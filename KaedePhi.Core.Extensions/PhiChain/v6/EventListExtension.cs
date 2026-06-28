using System;
using System.Collections.Generic;
using KaedePhi.Core.Common;
using KaedePhi.Core.PhiChain.v6;

namespace KaedePhi.Core.Extensions.PhiChain.v6
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
        public static void AppendEvent(this List<LineEvent> eventList, LineEvent @event)
        {
            if (eventList == null)
                throw new ArgumentNullException(nameof(eventList));
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));
            var index = eventList.FindIndex(e => e.StartBeat > @event.StartBeat);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }
    }
}
