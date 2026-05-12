using System;

namespace KaedePhi.Core.Utils
{
    public static class Bezier
    {
        /// <summary>
        /// 三次贝塞尔缓动插值。
        /// points = [x1, y1, x2, y2]，隐含端点 (0,0) → (1,1)。
        /// 控制点可以是负数或大于1，产生过冲/欠冲效果。
        /// </summary>
        public static T Do<T>(float[] points, float t, T startValue, T endValue,
            float left = 0.0f, float right = 1.0f)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            if (points == null || points.Length < 4)
                throw new ArgumentException("points 需要至少 4 个元素 [x1, y1, x2, y2]");

            var type = typeof(T);
            if (type != typeof(float) && type != typeof(double)
                && type != typeof(int) && type != typeof(byte))
                throw new NotSupportedException("T must be float, double, int, or byte");

            float x1 = points[0], y1 = points[1];
            float x2 = points[2], y2 = points[3];

            // t → 映射到参数区间
            float mappedT = left + t * (right - left);

            // 边界快速返回
            if (mappedT <= 0f) return startValue;
            if (mappedT >= 1f) return endValue;

            // 从 x(u) = mappedT 反解参数 u
            float u = SolveU(x1, x2, mappedT);

            // ★ 核心修正：不 Clamp，保留过冲/欠冲
            float easing = SampleCurveY(u, y1, y2);

            double start = Convert.ToDouble(startValue);
            double end   = Convert.ToDouble(endValue);
            double result = start + easing * (end - start);

            return (T)Convert.ChangeType(result, typeof(T));
        }

        #region 核心数学

        /// <summary>
        /// 牛顿迭代 + 二分法兜底，求 u 使得 x(u) = targetX
        /// </summary>
        private static float SolveU(float x1, float x2, float targetX)
        {
            float u = targetX;

            // --- 第一阶段：牛顿迭代（快速收敛） ---
            bool converged = false;
            for (int i = 0; i < 8; i++)
            {
                float xU = SampleCurveX(u, x1, x2);
                float err = xU - targetX;

                if (MathF.Abs(err) < 1e-7f)
                {
                    converged = true;
                    break;
                }

                float deriv = SampleCurveXDerivative(u, x1, x2);

                // 导数太小 → 牛顿不稳定，切二分
                if (MathF.Abs(deriv) < 1e-7f)
                    break;

                u -= err / deriv;
            }

            // --- 第二阶段：二分法兜底 ---
            if (!converged)
            {
                // 在 [0, 1] 上搜索最后一个满足 x(u) ≤ targetX 的区间
                // 保证取到"前进方向"上的解
                const int subdivisions = 32;
                float step = 1f / subdivisions;

                float uLo = 0f, uHi = 1f;
                float prevX = 0f;

                for (int i = 1; i <= subdivisions; i++)
                {
                    float ui = step * i;
                    float xi = SampleCurveX(ui, x1, x2);

                    // 找到跨越 targetX 的最后一段（取最右侧的根）
                    if ((prevX <= targetX && xi >= targetX) ||
                        (prevX >= targetX && xi <= targetX))
                    {
                        uLo = step * (i - 1);
                        uHi = ui;
                    }

                    prevX = xi;
                }

                // 在定位到的区间内做精确二分
                for (int i = 0; i < 20; i++)
                {
                    u = (uLo + uHi) * 0.5f;
                    float xU = SampleCurveX(u, x1, x2);
                    float err = xU - targetX;

                    if (MathF.Abs(err) < 1e-7f)
                        break;

                    if (err < 0f)
                        uLo = u;
                    else
                        uHi = u;
                }

                u = (uLo + uHi) * 0.5f;
            }

            return Math.Clamp(u, 0f, 1f);
        }

        // x(u) = 3(1−u)²u·x1 + 3(1−u)u²·x2 + u³
        private static float SampleCurveX(float u, float x1, float x2)
        {
            float omu = 1f - u;
            return 3f * omu * omu * u   * x1
                 + 3f * omu * u   * u   * x2
                 + u   * u   * u;
        }

        // y(u) = 3(1−u)²u·y1 + 3(1−u)u²·y2 + u³
        private static float SampleCurveY(float u, float y1, float y2)
        {
            float omu = 1f - u;
            return 3f * omu * omu * u   * y1
                 + 3f * omu * u   * u   * y2
                 + u   * u   * u;
        }

        // x'(u) = 3(1−u)²·x1 + 6(1−u)u·(x2−x1) + 3u²·(1−x2)
        private static float SampleCurveXDerivative(float u, float x1, float x2)
        {
            float omu = 1f - u;
            return 3f * omu * omu           * x1
                 + 6f * omu * u             * (x2 - x1)
                 + 3f * u   * u             * (1f - x2);
        }

        #endregion
    }
}
