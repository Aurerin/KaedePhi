using System;
using System.Collections.Generic;
using KaedePhi.Core.RePhiEdit.Events;

namespace KaedePhi.Core.Extensions.RePhiEdit
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
        public static void AppendEvent<T, TPayload>(this List<T> eventList, T @event)
            where T : Event<TPayload>
            where TPayload : notnull
        {
            if (eventList is null)
                throw new ArgumentNullException(nameof(eventList));
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            // 对比列表中evt的StartBeat，然后插入到中间，保持列表升序
            var index = eventList.FindIndex(e => e.StartBeat > @event.StartBeat);
            if (index == -1)
                eventList.Add(@event);
            else
                eventList.Insert(index, @event);
        }
    }
}
