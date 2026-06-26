using KaedePhi.Core.Common;
using KaedePhi.Core.PhiChain.v6;

namespace KaedePhi.Core.Extensions.PhiChain.v6;

public static class EventListExtension
{
    extension(List<LineEvent> eventList)
    {
        /// <summary>
        /// 将事件插入到列表
        /// 使用前必须确保列表已经按照StartBeat升序排序
        /// </summary>
        /// <param name="event">事件</param>
        /// <exception cref="ArgumentNullException">事件列表或事件为 <see langword="null"/></exception>
        public void AppendEvent(LineEvent @event)
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
