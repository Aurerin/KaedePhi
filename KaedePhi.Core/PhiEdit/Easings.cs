using JetBrains.Annotations;
using static KaedePhi.Core.Utils.Easings;

namespace KaedePhi.Core.PhiEdit
{
    public static class Easings
    {
        // Method to evaluate easing between any start and end point
        [PublicAPI]
        public static double Evaluate(EasingFunction function, double t)
        {
            return function(t);
        }

        // Overload, using int to specify the corresponding EasingFunction
        public static double Evaluate(int easingType, double t)
        {
            return Evaluate(GetFunction(easingType), t);
        }
    }

    public class Easing
    {
        private readonly int _easingNumber;
        private readonly EasingFunction _function;

        private static readonly Easing[] Cache = new Easing[30];

        /// <summary>获取缓存的 Easing 实例，避免重复创建。</summary>
        public static Easing Get(int easingNumber)
        {
            if (easingNumber is >= 1 and <= 29)
                return Cache[easingNumber] ??= new Easing(easingNumber);
            return new Easing(easingNumber);
        }

        public Easing(int easingNumber)
        {
            _easingNumber = easingNumber;
            _function = GetFunction(easingNumber);
        }

        /// <summary>对 [start, end] 区间在 t 处进行插值</summary>
        [PublicAPI]
        public float Interpolate(float start, float end, float t)
        {
            var easedTime = _function(t);
            return (float)(start + (end - start) * easedTime);
        }

        /// <inheritdoc cref="Interpolate(float,float,float)"/>
        [PublicAPI]
        public double Interpolate(double start, double end, double t)
        {
            var easedTime = _function(t);
            return start + (end - start) * easedTime;
        }

        public static implicit operator int(Easing easing) => easing._easingNumber;
        public static implicit operator Easing(int easingNumber) => Get(easingNumber);
    }
}