using System.Collections.Generic;

namespace KaedePhi.Core.KaedePhi.Controls
{
    public class XControl : ControlBase
    {
        public float Pos { get; set; } = 1.0f;

        private static readonly List<XControl> DefaultInstance = new()
        {
            new XControl { Easing = new Easing(1), Pos = 1.0f, X = 0.0f },
            new XControl { Easing = new Easing(1), Pos = 1.0f, X = 9999999.0f }
        };

        public static List<XControl> Default
            => DefaultInstance.ConvertAll(input => input.Clone() as XControl);

        public override ControlBase Clone()
        {
            return new XControl { Easing = new Easing(Easing), X = X, Pos = Pos };
        }
    }
}