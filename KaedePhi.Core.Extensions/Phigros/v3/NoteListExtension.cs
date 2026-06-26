using KaedePhi.Core.Phigros.v3;

namespace KaedePhi.Core.Extensions.Phigros.v3;

public static class NoteListExtension
{
    extension(List<Note> noteList)
    {
        /// <summary>
        /// 将音符加入音符列表
        /// 使用前必须确保列表已经按照Time升序排序
        /// </summary>
        /// <param name="note">音符</param>
        /// <exception cref="ArgumentNullException">音符或音符列表为 <see langword="null"/></exception>
        public void AppendNote(Note note)
        {
            ArgumentNullException.ThrowIfNull(noteList);
            ArgumentNullException.ThrowIfNull(note);
            var index = noteList.FindIndex(n => n.Time > note.Time);
            if (index == -1)
                noteList.Add(note);
            else
                noteList.Insert(index, note);
        }
    }
}
