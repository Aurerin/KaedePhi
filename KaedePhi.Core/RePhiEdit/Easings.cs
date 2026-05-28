using KaedePhi.Core.RePhiEdit.JsonConverter;
using Newtonsoft.Json;
using static KaedePhi.Core.Utils.Easings;

namespace KaedePhi.Core.RePhiEdit
{
    public static class Easings
    {
        // Method to evaluate easing between any start and end point
        private static double Evaluate(EasingFunction function, double start, double end, double t)
        {
            // code by PhiZone Player
            var progress = function(start + (end - start) * t);
            var progressStart = function(start);
            var progressEnd = function(end);
            return (progress - progressStart) / (progressEnd - progressStart);
        }

        // Overload, using int to specify the corresponding EasingFunction
        public static double Evaluate(int easingType, double start, double end, double t)
        {
            return Evaluate(GetFunction(easingType), start, end, t);
        }
    }

    [JsonConverter(typeof(EasingJsonConverter))] 
    public class Easing
    {
        public Easing(int easingNumber)
        {
            _easingNumber = easingNumber;
        }

        [JsonIgnore]
        private int _easingNumber;

        public float Do(float minLim, float maxLim, float start, float end, float t)
        {
            var easedTime = Easings.Evaluate(_easingNumber, minLim, maxLim, t);
            // 插值后返回
            return (float)(start + (end - start) * easedTime);
        }

        public double Do(float minLim, float maxLim, double start, double end, double t)
        {
            var easedTime = Easings.Evaluate(_easingNumber, minLim, maxLim, t);
            // 插值后返回
            return start + (end - start) * easedTime;
        }

        public int Do(float minLim, float maxLim, int start, int end, float t)
        {
            var easedTime = Easings.Evaluate(_easingNumber, minLim, maxLim, t);
            // 插值后返回
            return (int)(start + (end - start) * easedTime);
        }

        public byte Do(float minLim, float maxLim, byte start, byte end, float t)
        {
            var easedTime = Easings.Evaluate(_easingNumber, minLim, maxLim, t);
            // 插值后返回
            return (byte)(start + (end - start) * easedTime);
        }

        // 以int访问时，返回缓动编号
        public static implicit operator int(Easing easing) => easing._easingNumber;
    }
}