using System.Collections.Generic;

namespace KaedePhi.Core.KaedePhi.Controls
{
    public class SkewControl : ControlBase
    {
        public float Skew { get; set; } = 1.0f;

        private static readonly List<SkewControl> DefaultInstance = new()
        {
            new SkewControl { Easing = new Easing(1), Skew = 0.0f, X = 0.0f },
            new SkewControl { Easing = new Easing(1), Skew = 0.0f, X = 9999999.0f }
        };

        public static List<SkewControl> Default
            => DefaultInstance.ConvertAll(input => input.Clone() as SkewControl);

        public override ControlBase Clone()
        {
            return new SkewControl { Easing = new Easing(Easing), X = X, Skew = Skew };
        }
    }
}