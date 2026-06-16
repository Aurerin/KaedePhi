using static KaedePhi.Core.Utils.Easings;

namespace KaedePhi.Core.RePhiEdit
{
    public static class Easings
    {
        // 在任意起点和终点之间评估缓动
        private static double Evaluate(EasingFunction function, double start, double end, double t)
        {
            // 代码来自 PhiZone Player
            var progress = function(start + (end - start) * t);
            var progressStart = function(start);
            var progressEnd = function(end);
            return (progress - progressStart) / (progressEnd - progressStart);
        }

        // 使用 int 指定对应的缓动函数
        public static double Evaluate(int easingType, double start, double end, double t)
        {
            return Evaluate(PhiEdit.Easings.GetFunction(easingType), start, end, t);
        }
    }
}
