// 向前兼容层 — RePhiEdit 格式
//
// 这些类型已迁移至子命名空间以改善目录结构的可维护性：
//   事件系统  → KaedePhi.Core.RePhiEdit.Events
//   控制点    → KaedePhi.Core.RePhiEdit.Controls
//
// 此文件保留原命名空间下的过时别名，以便旧代码仅需添加 [Obsolete] 警告
// 而无需立即修改即可继续编译。
//
// ⚠ 迁移说明：
//   - Event<T>       : List<Event<T>> 与 List<Events.Event<T>> 不兼容，
//                      请直接使用 KaedePhi.Core.RePhiEdit.Events.Event<T>。
//   - 其余类型        : 直接替换命名空间即可。

using System;

namespace KaedePhi.Core.RePhiEdit
{
    /// <inheritdoc cref="Events.Event{T}"/>
    [Obsolete(
        "Event<T> has been moved to KaedePhi.Core.RePhiEdit.Events.Event<T>. "
            + "WARNING: List<Event<T>> is NOT assignment-compatible with List<Events.Event<T>>; "
            + "please migrate all collection usages to the new namespace directly."
    )]
    public class Event<T> : Events.Event<T> { }

    /// <inheritdoc cref="Events.EventLayer"/>
    [Obsolete("EventLayer has been moved to KaedePhi.Core.RePhiEdit.Events.EventLayer.")]
    public class EventLayer : Events.EventLayer { }

    /// <inheritdoc cref="Events.ExtendLayer"/>
    [Obsolete("ExtendLayer has been moved to KaedePhi.Core.RePhiEdit.Events.ExtendLayer.")]
    public class ExtendLayer : Events.ExtendLayer { }

    /// <inheritdoc cref="Controls.ControlBase"/>
    [Obsolete("ControlBase has been moved to KaedePhi.Core.RePhiEdit.Controls.ControlBase.")]
    public abstract class ControlBase : Controls.ControlBase { }

    /// <inheritdoc cref="Controls.AlphaControl"/>
    [Obsolete("AlphaControl has been moved to KaedePhi.Core.RePhiEdit.Controls.AlphaControl.")]
    public class AlphaControl : Controls.AlphaControl { }

    /// <inheritdoc cref="Controls.XControl"/>
    [Obsolete("XControl has been moved to KaedePhi.Core.RePhiEdit.Controls.XControl.")]
    public class XControl : Controls.XControl { }

    /// <inheritdoc cref="Controls.SizeControl"/>
    [Obsolete("SizeControl has been moved to KaedePhi.Core.RePhiEdit.Controls.SizeControl.")]
    public class SizeControl : Controls.SizeControl { }

    /// <inheritdoc cref="Controls.SkewControl"/>
    [Obsolete("SkewControl has been moved to KaedePhi.Core.RePhiEdit.Controls.SkewControl.")]
    public class SkewControl : Controls.SkewControl { }

    /// <inheritdoc cref="Controls.YControl"/>
    [Obsolete("YControl has been moved to KaedePhi.Core.RePhiEdit.Controls.YControl.")]
    public class YControl : Controls.YControl { }
}
