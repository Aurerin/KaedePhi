using KaedePhi.Core.PhiEdit;

namespace KaedePhi.Core.Extensions.PhiEdit;

public static class FrameListExtension
{
    extension(List<Frame> frameList)
    {
        /// <summary>
        /// 将帧插入到列表
        /// 使用前必须确保列表已经按照Beat升序排序
        /// </summary>
        /// <param name="frame">帧</param>
        /// <exception cref="ArgumentNullException">帧列表或帧为 <see langword="null"/></exception>
        public void AppendFrame(Frame frame)
        {
            ArgumentNullException.ThrowIfNull(frameList);
            ArgumentNullException.ThrowIfNull(frame);
            var index = frameList.FindIndex(f => f.Beat > frame.Beat);
            if (index == -1)
                frameList.Add(frame);
            else
                frameList.Insert(index, frame);
        }
    }

    extension(List<MoveFrame> frameList)
    {
        /// <summary>
        /// 将移动帧插入到列表
        /// 使用前必须确保列表已经按照Beat升序排序
        /// </summary>
        /// <param name="frame">移动帧</param>
        /// <exception cref="ArgumentNullException">帧列表或帧为 <see langword="null"/></exception>
        public void AppendMoveFrame(MoveFrame frame)
        {
            ArgumentNullException.ThrowIfNull(frameList);
            ArgumentNullException.ThrowIfNull(frame);
            var index = frameList.FindIndex(f => f.Beat > frame.Beat);
            if (index == -1)
                frameList.Add(frame);
            else
                frameList.Insert(index, frame);
        }
    }
}
