using static KaedePhi.Core.Utils.Easings;

namespace KaedePhi.Core.KaedePhi
{
    public static class Easings
    {
        // 在任意起点和终点之间评估缓动
        private static double Evaluate(EasingFunction function, double start, double end, double t)
        {
            // 代码来自 PhiZone Player
            double progress = function(start + (end - start) * t);
            double progressStart = function(start);
            double progressEnd = function(end);
            return (progress - progressStart) / (progressEnd - progressStart);
        }

        // 使用 int 指定对应的缓动函数
        public static double Evaluate(int easingType, double start, double end, double t)
        {
            EasingFunction function = easingType switch
            {
                1 => Linear,
                // 正弦
                2 => EaseInSine,
                3 => EaseOutSine,
                4 => EaseInOutSine,
                // 二次
                5 => EaseInQuad,
                6 => EaseOutQuad,
                7 => EaseInOutQuad,
                // 三次
                8 => EaseInCubic,
                9 => EaseOutCubic,
                10 => EaseInOutCubic,
                // 四次
                11 => EaseInQuart,
                12 => EaseOutQuart,
                13 => EaseInOutQuart,
                // 五次
                14 => EaseInQuint,
                15 => EaseOutQuint,
                16 => EaseInOutQuint,
                // 指数
                17 => EaseInExpo,
                18 => EaseOutExpo,
                19 => EaseInOutExpo,
                // 圆形
                20 => EaseInCirc,
                21 => EaseOutCirc,
                22 => EaseInOutCirc,
                // 回弹
                23 => EaseInBack,
                24 => EaseOutBack,
                25 => EaseInOutBack,
                // 弹性
                26 => EaseInElastic,
                27 => EaseOutElastic,
                28 => EaseInOutElastic,
                // 弹跳
                29 => EaseInBounce,
                30 => EaseOutBounce,
                31 => EaseInOutBounce,
                // 兜底
                _ => Linear,
            };

            return Evaluate(function, start, end, t);
        }
    }
}
