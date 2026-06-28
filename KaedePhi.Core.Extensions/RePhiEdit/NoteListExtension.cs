using System;
using System.Collections.Generic;
using KaedePhi.Core.RePhiEdit;

namespace KaedePhi.Core.Extensions.RePhiEdit
{
    public static class NoteListExtension
    {
        /// <summary>
        /// 将音符加入音符列表
        /// 使用前必须确保列表已经按照StartBeat升序排序
        /// </summary>
        /// <param name="noteList">音符列表</param>
        /// <param name="note">音符</param>
        /// <exception cref="ArgumentNullException">音符或音符列表为 <see langword="null"/></exception>
        public static void AppendNote(this List<Note> noteList, Note note)
        {
            if (noteList is null)
                throw new ArgumentNullException(nameof(noteList));
            if (note is null)
                throw new ArgumentNullException(nameof(note));
            // 对比列表中note的StartBeat，然后插入到中间，保持列表升序
            var index = noteList.FindIndex(n => n.StartBeat > note.StartBeat);
            if (index == -1)
                noteList.Add(note);
            else
                noteList.Insert(index, note);
        }
    }
}
