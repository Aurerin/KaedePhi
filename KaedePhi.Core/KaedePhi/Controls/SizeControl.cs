using System.Collections.Generic;

namespace KaedePhi.Core.KaedePhi.Controls
{
    public class SizeControl : ControlBase
    {
        public float Size { get; set; } = 1.0f;

        private static readonly List<SizeControl> DefaultInstance = new()
        {
            new SizeControl { Easing = new Easing(1), Size = 1.0f, X = 0.0f },
            new SizeControl { Easing = new Easing(1), Size = 1.0f, X = 9999999.0f }
        };

        public static List<SizeControl> Default
            => DefaultInstance.ConvertAll(input => input.Clone() as SizeControl);

        public override ControlBase Clone()
        {
            return new SizeControl { Easing = new Easing(Easing), X = X, Size = Size };
        }
    }
}