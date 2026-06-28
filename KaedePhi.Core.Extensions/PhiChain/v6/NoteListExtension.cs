using System;
using System.Collections.Generic;
using KaedePhi.Core.Common;
using KaedePhi.Core.PhiChain.v6;

namespace KaedePhi.Core.Extensions.PhiChain.v6
{
    public static class NoteListExtension
    {
        /// <summary>
        /// 将音符加入音符列表
        /// 使用前必须确保列表已经按照Beat升序排序
        /// </summary>
        /// <param name="noteList">音符列表</param>
        /// <param name="note">音符</param>
        /// <exception cref="ArgumentNullException">音符或音符列表为 <see langword="null"/></exception>
        public static void AppendNote(this List<Note> noteList, Note note)
        {
            if (noteList == null)
                throw new ArgumentNullException(nameof(noteList));
            if (note == null)
                throw new ArgumentNullException(nameof(note));
            var index = noteList.FindIndex(n => n.Beat > note.Beat);
            if (index == -1)
                noteList.Add(note);
            else
                noteList.Insert(index, note);
        }
    }
}
