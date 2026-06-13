using System.Runtime.CompilerServices;
using KaedePhi.Core.Common;
using KaedePhi.Core.Utils;

namespace KaedePhi.Core.KaedePhi.Events
{
    public class Event<T> : EventBase<T>
    {
        /// <summary>
        /// 缓动类型
        /// </summary>
        public Easing Easing { get; set; } = new(1);

        /// <summary>
        /// 模拟器保留字段
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        /// 模拟器保留字段
        /// </summary>
        public float EndTime { get; set; }

        /// <summary>
        /// 保留字段
        /// </summary>
        public float FloorPosition { get; set; }

        /// <summary>
        /// 获取指定拍在此事件中的插值（返回 double）。
        /// </summary>
        public double GetValueAtBeatAsDouble(Beat beat)
        {
            var t = (beat - StartBeat) / (EndBeat - StartBeat);
            if (t <= 0)
                return GetStartValueAsDouble();
            if (t >= 1)
                return GetEndValueAsDouble();

            return Easing.Interpolate(
                EasingLeft,
                EasingRight,
                GetStartValueAsDouble(),
                GetEndValueAsDouble(),
                t
            );
        }

        /// <summary>
        /// 获取某个拍在这个事件中的值
        /// </summary>
        public T GetValueAtBeat(Beat beat)
        {
            var t = (beat - StartBeat) / (EndBeat - StartBeat);
            return t switch
            {
                <= 0 => StartValue,
                >= 1 => EndValue,
                _ => IsBezier ? InterpolateBezier(t) : InterpolateEasing(t),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T InterpolateBezier(double t)
        {
            var ft = (float)t;
            if (typeof(T) == typeof(float))
                return Cast<float, T>(
                    Bezier.Do(BezierPoints, ft, GetStartValueAsSingle(), GetEndValueAsSingle())
                );
            if (typeof(T) == typeof(double))
                return Cast<double, T>(
                    Bezier.Do(BezierPoints, ft, GetStartValueAsDouble(), GetEndValueAsDouble())
                );
            if (typeof(T) == typeof(int))
                return Cast<int, T>(
                    Bezier.Do(BezierPoints, ft, GetStartValueAsInt32(), GetEndValueAsInt32())
                );
            if (typeof(T) == typeof(byte[]))
                return InterpolateByteArray(t, useBezier: true);
            throw new System.NotSupportedException($"类型 {typeof(T)} 不受支持。");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T InterpolateEasing(double t)
        {
            if (typeof(T) == typeof(float))
                return Cast<float, T>(
                    (float)Easing.Interpolate(
                        EasingLeft, EasingRight,
                        GetStartValueAsSingle(), GetEndValueAsSingle(), t
                    )
                );
            if (typeof(T) == typeof(double))
                return Cast<double, T>(
                    Easing.Interpolate(
                        EasingLeft, EasingRight,
                        GetStartValueAsDouble(), GetEndValueAsDouble(), t
                    )
                );
            if (typeof(T) == typeof(int))
                return Cast<int, T>(
                    (int)Easing.Interpolate(
                        EasingLeft, EasingRight,
                        GetStartValueAsInt32(), GetEndValueAsInt32(), t
                    )
                );
            if (typeof(T) == typeof(byte[]))
                return InterpolateByteArray(t, useBezier: false);
            throw new System.NotSupportedException($"类型 {typeof(T)} 不受支持。");
        }

        private T InterpolateByteArray(double t, bool useBezier)
        {
            var ft = (float)t;
            var startBytes =
                StartValue as byte[]
                ?? throw new System.InvalidOperationException(
                    "StartValue 或 EndValue 不是 byte[] 或为 null。"
                );
            var endBytes =
                EndValue as byte[]
                ?? throw new System.InvalidOperationException(
                    "StartValue 或 EndValue 不是 byte[] 或为 null。"
                );
            if (startBytes.Length != endBytes.Length)
                throw new System.InvalidOperationException(
                    "插值要求两个 byte[] 长度一致。"
                );

            var result = new byte[startBytes.Length];
            for (var i = 0; i < startBytes.Length; i++)
                if (useBezier)
                    result[i] = Bezier.Do(
                        BezierPoints, ft, startBytes[i], endBytes[i],
                        EasingLeft, EasingRight
                    );
                else
                    result[i] = (byte)Easing.Interpolate(
                        EasingLeft, EasingRight, startBytes[i], endBytes[i], t
                    );
            return Cast<byte[], T>(result);
        }

        /// <summary>
        /// 深拷贝事件。
        /// </summary>
        public Event<T> Clone()
        {
            var clone = new Event<T>
            {
                Easing = Easing,
                StartTime = StartTime,
                EndTime = EndTime,
                FloorPosition = FloorPosition,
            };
            CopyBaseTo(clone);
            return clone;
        }
    }
}
