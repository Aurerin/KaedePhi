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
            return Evaluate(PhiEdit.Easings.GetFunction(easingType), start, end, t);
        }
    }
}
