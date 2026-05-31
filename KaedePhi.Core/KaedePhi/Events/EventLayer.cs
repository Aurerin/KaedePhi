using System.Collections.Generic;
using KaedePhi.Core.Common;

namespace KaedePhi.Core.KaedePhi.Events
{
    public class EventLayer
    {
        /// <summary>
        /// X移动事件
        /// </summary>
        public List<Event<double>>? MoveXEvents { get; set; } // 移动事件

        public List<Event<double>>? MoveYEvents { get; set; } // 移动事件

        public List<Event<double>>? RotateEvents { get; set; } // 旋转事件

        public List<Event<int>>? AlphaEvents { get; set; } // 透明度事件

        public List<Event<float>>? SpeedEvents { get; set; } // 速度事件

        /// <summary>
        /// 获取某个拍时，指定事件层级指定事件列表的数值
        /// </summary>
        /// <param name="events">事件数组</param>
        /// <param name="beat">指定拍</param>
        /// <returns>在指定拍时，指定事件列表的数值</returns>
        public static T GetValueAtBeat<T>(List<Event<T>> events, Beat beat)
        {
            Event<T>? selectedEvent = null;

            foreach (var e in events)
            {
                // 事件按开始拍排序时，后到(开始拍更晚)的重叠事件应覆盖先到事件
                if (beat >= e.StartBeat && beat <= e.EndBeat)
                    selectedEvent = e;
                // 如果当前拍小于事件的开始拍，说明后续事件都不符合条件，跳出循环
                if (beat < e.StartBeat)
                    break;
            }

            if (selectedEvent is not null)
                return selectedEvent.GetValueAtBeat(beat);

            var previousEvent = events.FindLast(e => beat > e.EndBeat);
            return previousEvent is not null ? previousEvent.EndValue : default;
        }

        /// <summary>
        /// 按事件的开始时间排序所有事件
        /// </summary>
        public void Sort()
        {
            var eventLists = new List<List<Event<double>>>
            {
                MoveXEvents, MoveYEvents, RotateEvents
            };
            var alphaEventList = AlphaEvents;
            var speedEventList = SpeedEvents;
            eventLists.ForEach(events => { events?.Sort((a, b) => a.StartBeat.CompareTo(b.StartBeat)); });
            alphaEventList?.Sort((a, b) => a.StartBeat.CompareTo(b.StartBeat));
            speedEventList?.Sort((a, b) => a.StartBeat.CompareTo(b.StartBeat));
        }

        /// <summary>
        /// 克隆，使用深拷贝
        /// </summary>
        /// <returns>一个从里到外都新新的事件层级！</returns>
        public EventLayer Clone()
        {
            // 深拷贝，包括Event列表
            var clone = new EventLayer();
            // 保证列表中的元素也被深拷贝（通过LINQ调用Event的Clone方法）
            if (MoveXEvents is not null)
                clone.MoveXEvents = MoveXEvents.ConvertAll(e => e.Clone());
            if (MoveYEvents is not null)
                clone.MoveYEvents = MoveYEvents.ConvertAll(e => e.Clone());
            if (RotateEvents is not null)
                clone.RotateEvents = RotateEvents.ConvertAll(e => e.Clone());
            if (AlphaEvents is not null)
                clone.AlphaEvents = AlphaEvents.ConvertAll(e => e.Clone());
            if (SpeedEvents is not null)
                clone.SpeedEvents = SpeedEvents.ConvertAll(e => e.Clone());
            return clone;
        }

        /// <summary>
        /// 强行预期化，将空列表设置为null，保证Json序列化时不包含空列表
        /// </summary>
        public void Anticipation()
        {
            if (MoveXEvents is { Count: 0 })
                MoveXEvents = null;
            if (MoveYEvents is { Count: 0 })
                MoveYEvents = null;
            if (RotateEvents is { Count: 0 })
                RotateEvents = null;
            if (AlphaEvents is { Count: 0 })
                AlphaEvents = null;
            if (SpeedEvents is { Count: 0 })
                SpeedEvents = null;
        }
    }
}
