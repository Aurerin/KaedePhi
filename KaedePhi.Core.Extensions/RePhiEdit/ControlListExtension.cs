using KaedePhi.Core.RePhiEdit.Controls;

namespace KaedePhi.Core.Extensions.RePhiEdit;

public static class ControlListExtension
{
    /// <param name="controls">被插入的Control列表</param>
    extension<T>(List<T> controls)
        where T : ControlBase
    {
        /// <summary>
        /// 将Control插入Control列表，并确保顺序正确
        /// 使用本方法前，必须确保列表已经按X升序排列
        /// </summary>
        /// <param name="control">被插入的Control元素</param>
        /// <exception cref="ArgumentNullException">controls 或 control 为 <see langword="null"/></exception>
        public void AppendControl(T control)
        {
            ArgumentNullException.ThrowIfNull(controls);
            ArgumentNullException.ThrowIfNull(control);

            // 找到第一个 X 大于待插入控制点的位置，将其插入两控制点之间
            var index = controls.FindIndex(c => c.X > control.X);
            if (index == -1)
                controls.Add(control);
            else
                controls.Insert(index, control);
        }
    }
}
