using System.Runtime.CompilerServices;
using KaedePhi.Core.Common;
using KaedePhi.Core.RePhiEdit.JsonConverter;
using KaedePhi.Core.Utils;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit.Events
{
    public class Event<T> : EventBase<T>
    {
        /// <summary>
        /// 缓动类型
        /// </summary>
        [JsonProperty("easingType")]
        public Easing Easing { get; set; } = new(1);

        /// <inheritdoc/>
        [JsonProperty("bezier")]
        [JsonConverter(typeof(BoolConverter))]
        public new bool IsBezier
        {
            get => base.IsBezier;
            set => base.IsBezier = value;
        }

        /// <inheritdoc/>
        [JsonProperty("bezierPoints")]
        public new float[] BezierPoints
        {
            get => base.BezierPoints;
            set => base.BezierPoints = value;
        }

        /// <inheritdoc/>
        [JsonProperty("easingLeft")]
        public new float EasingLeft
        {
            get => base.EasingLeft;
            set => base.EasingLeft = value;
        }

        /// <inheritdoc/>
        [JsonProperty("easingRight")]
        public new float EasingRight
        {
            get => base.EasingRight;
            set => base.EasingRight = value;
        }

        /// <inheritdoc/>
        [JsonProperty("start")]
#pragma warning disable CS8618
        public new T StartValue
        {
            get => base.StartValue;
            set => base.StartValue = value;
        }

        /// <inheritdoc/>
        [JsonProperty("end")]
        public new T EndValue
        {
            get => base.EndValue;
            set => base.EndValue = value;
        }
#pragma warning restore CS8618

        /// <inheritdoc/>
        [JsonProperty("startTime")]
        public new Beat StartBeat
        {
            get => base.StartBeat;
            set => base.StartBeat = value;
        }

        /// <inheritdoc/>
        [JsonProperty("endTime")]
        public new Beat EndBeat
        {
            get => base.EndBeat;
            set => base.EndBeat = value;
        }

        /// <inheritdoc/>
        [JsonProperty(
            "font",
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        )]
        public new string? Font
        {
            get => base.Font;
            set => base.Font = value;
        }

        /// <summary>
        /// 获取某个拍在这个事件中的值
        /// </summary>
        public T GetValueAtBeat(Beat beat)
        {
            var t = (beat - StartBeat) / (EndBeat - StartBeat);
            if (t <= 0)
                return StartValue;
            if (t >= 1)
                return EndValue;
            return IsBezier ? InterpolateBezier(t) : InterpolateEasing(t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T InterpolateBezier(float t)
        {
            if (typeof(T) == typeof(float))
                return Cast<float, T>(
                    Bezier.Do(BezierPoints, t, GetStartValueAsSingle(), GetEndValueAsSingle())
                );
            if (typeof(T) == typeof(double))
                return Cast<double, T>(
                    Bezier.Do(BezierPoints, t, GetStartValueAsDouble(), GetEndValueAsDouble())
                );
            if (typeof(T) == typeof(int))
                return Cast<int, T>(
                    Bezier.Do(BezierPoints, t, GetStartValueAsInt32(), GetEndValueAsInt32())
                );
            if (typeof(T) == typeof(byte[]))
                return InterpolateByteArray(t, useBezier: true);
            throw new System.NotSupportedException($"类型 {typeof(T)} 不受支持。");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T InterpolateEasing(float t)
        {
            if (typeof(T) == typeof(float))
                return Cast<float, T>(
                    Easing.Interpolate(
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
                    Easing.Interpolate(
                        EasingLeft, EasingRight,
                        GetStartValueAsInt32(), GetEndValueAsInt32(), t
                    )
                );
            if (typeof(T) == typeof(byte[]))
                return InterpolateByteArray(t, useBezier: false);
            throw new System.NotSupportedException($"类型 {typeof(T)} 不受支持。");
        }

        private T InterpolateByteArray(float t, bool useBezier)
        {
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
                result[i] = useBezier
                    ? Bezier.Do(BezierPoints, t, startBytes[i], endBytes[i])
                    : (byte)Easing.Interpolate(
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
            };
            CopyBaseTo(clone);
            return clone;
        }
    }
}
