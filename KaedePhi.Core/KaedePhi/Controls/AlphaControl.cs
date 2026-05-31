using System.Collections.Generic;

namespace KaedePhi.Core.KaedePhi.Controls
{
    public class AlphaControl : ControlBase
    {
        public float Alpha { get; set; } = 1.0f;

        private static readonly List<AlphaControl> DefaultInstance = new()
        {
            new AlphaControl { Easing = new Easing(1), Alpha = 1.0f, X = 0.0f },
            new AlphaControl { Easing = new Easing(1), Alpha = 1.0f, X = 9999999.0f }
        };

        public static List<AlphaControl> Default
            => DefaultInstance.ConvertAll(input => input.Clone() as AlphaControl);

        public override ControlBase Clone()
        {
            return new AlphaControl { Easing = new Easing(Easing), X = X, Alpha = Alpha };
        }
    }
}