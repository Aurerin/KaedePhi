using KaedePhi.Core.Common;
using KaedePhi.Core.PhiFans;

namespace KaedePhi.Core.Extensions.PhiFans;

public static class NoteListExtension
{
    extension<T>(List<T> noteList)
        where T : Note
    {
        /// <summary>
        /// 将音符加入音符列表
        /// 使用前必须确保列表已经按照Beat升序排序
        /// </summary>
        /// <param name="note">音符</param>
        /// <exception cref="ArgumentNullException">音符或音符列表为 <see langword="null"/></exception>
        public void AppendNote(T note)
        {
            ArgumentNullException.ThrowIfNull(noteList);
            ArgumentNullException.ThrowIfNull(note);
            var index = noteList.FindIndex(n => n.Beat > note.Beat);
            if (index == -1)
                noteList.Add(note);
            else
                noteList.Insert(index, note);
        }
    }
}
