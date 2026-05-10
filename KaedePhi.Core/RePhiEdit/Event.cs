using System;
using System.Linq;
using KaedePhi.Core.Common;
using KaedePhi.Core.RePhiEdit.JsonConverter;
using KaedePhi.Core.Utils;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit
{
    public class Event<T>
    {
        /// <summary>
        /// 是否为贝塞尔曲线
        /// </summary>
        [JsonProperty("bezier")]
        [JsonConverter(typeof(BoolConverter))]
        public bool IsBezier { get; set; } // 是否为贝塞尔曲线

        /// <summary>
        /// 贝塞尔曲线控制点
        /// </summary>
        [JsonProperty("bezierPoints")]
        public float[] BezierPoints { get; set; } = new float[4]; // 贝塞尔曲线点

        /// <summary>
        /// 缓动截取左界限
        /// </summary>
        [JsonProperty("easingLeft")]
        public float EasingLeft { get; set; } // 缓动开始

        /// <summary>
        /// 缓动截取右界限
        /// </summary>
        [JsonProperty("easingRight")]
        public float EasingRight { get; set; } = 1.0f; // 缓动结束

        /// <summary>
        /// 缓动类型
        /// </summary>
        [JsonProperty("easingType")]
        public Easing Easing { get; set; } = new(1); // 缓动类型

        /// <summary>
        /// 事件开始数值
        /// </summary>
        [JsonProperty("start")]
        public T StartValue { get; set; } // 开始值

        /// <summary>
        /// 事件结束数值
        /// </summary>
        [JsonProperty("end")]
        public T EndValue { get; set; } // 结束值

        /// <summary>
        /// 事件开始拍
        /// </summary>
        [JsonProperty("startTime")]
        public Beat StartBeat { get; set; } = new(new[] { 0, 0, 1 }); // 开始时间

        /// <summary>
        /// 事件结束拍
        /// </summary>
        [JsonProperty("endTime")]
        public Beat EndBeat { get; set; } = new(new[] { 1, 0, 1 }); // 结束时间

        /// <summary>
        /// 当此事件为文字事件时，此值为字体文件相对路径，默认cmdysj.ttf
        /// </summary>
        [JsonProperty("font", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore)]
#nullable enable
        public string? Font { get; set; }
#nullable disable
        /// <summary>
        /// 获取某个拍在这个事件中的值
        /// </summary>
        /// <param name="beat">指定拍</param>
        /// <returns>指定拍时，此事件的数值</returns>
        public T GetValueAtBeat(Beat beat)
        {
            var t = (beat - StartBeat) / (EndBeat - StartBeat);

            // 如果启用了贝塞尔曲线,使用 Bezier.Do
            if (IsBezier)
            {
                // 检查 T 的类型并调用相应的方法
                if (typeof(T) == typeof(float))
                    return (T)(object)Bezier.Do(BezierPoints, t, Convert.ToSingle(StartValue),
                        Convert.ToSingle(EndValue), EasingLeft, EasingRight);
                else if (typeof(T) == typeof(double))
                    return (T)(object)Bezier.Do(BezierPoints, t, Convert.ToDouble(StartValue),
                        Convert.ToDouble(EndValue), EasingLeft, EasingRight);
                else if (typeof(T) == typeof(int))
                    return (T)(object)Bezier.Do(BezierPoints, t, Convert.ToInt32(StartValue),
                        Convert.ToInt32(EndValue), EasingLeft, EasingRight);
                else if (typeof(T) == typeof(byte[]))
                {
                    byte[] startBytes = StartValue as byte[];
                    byte[] endBytes = EndValue as byte[];
                    if (startBytes == null || endBytes == null)
                        throw new InvalidOperationException("Start or End is not a byte array, or is null.");
                    if (startBytes.Length != endBytes.Length)
                        throw new InvalidOperationException(
                            "Byte arrays must be of the same length for interpolation.");
                    byte[] result = new byte[startBytes.Length];
                    for (int i = 0; i < startBytes.Length; i++)
                        result[i] = Bezier.Do(BezierPoints, t, startBytes[i], endBytes[i], EasingLeft, EasingRight);
                    return (T)(object)result;
                }
                else
                    throw new NotSupportedException($"类型 {typeof(T)} 不受支持。");
            }

            // 检查 T 的类型并调用相应的方法
            if (typeof(T) == typeof(float))
                return (T)(object)Easing.Do(EasingLeft, EasingRight, Convert.ToSingle(StartValue),
                    Convert.ToSingle(EndValue),
                    t);
            else if (typeof(T) == typeof(double))
                return (T)(object)Easing.Do(EasingLeft, EasingRight, Convert.ToDouble(StartValue),
                    Convert.ToDouble(EndValue),
                    t);
            else if (typeof(T) == typeof(int))
                return (T)(object)Easing.Do(EasingLeft, EasingRight, Convert.ToInt32(StartValue),
                    Convert.ToInt32(EndValue), t);
            else if (typeof(T) == typeof(byte[]))
            {
                byte[] startBytes = StartValue as byte[];
                byte[] endBytes = EndValue as byte[];
                if (startBytes == null || endBytes == null)
                    throw new InvalidOperationException("Start or End is not a byte array, or is null.");
                if (startBytes.Length != endBytes.Length)
                    throw new InvalidOperationException(
                        "Byte arrays must be of the same length for interpolation.");
                byte[] result = new byte[startBytes.Length];
                for (int i = 0; i < startBytes.Length; i++)
                    result[i] = Easing.Do(EasingLeft, EasingRight, startBytes[i], endBytes[i], t);
                return (T)(object)result;
            }
            else
                throw new NotSupportedException($"类型 {typeof(T)} 不受支持。");
        }

        /// <summary>
        /// 针对已知T类型（int/byte/byte[]/string/float/double）的DeepClone实现
        /// 完全避免反射，直接处理已知类型
        /// </summary>
        private static TValue DeepClone<TValue>(TValue value)
        {
            if (value == null)
                return default;

            var type = typeof(TValue);

            // 值类型：int, float, double, byte
            if (type == typeof(int) || type == typeof(float) ||
                type == typeof(double) || type == typeof(byte))
                return value;

            // 不可变引用类型：string
            if (type == typeof(string))
                return value;

            // byte[]：需要深拷贝
            if (type == typeof(byte[]))
            {
                var arr = (byte[])(object)value;
                return (TValue)(object)arr.ToArray();
            }

            // 不应到达此处，但提供兜底
            return value;
        }

        public Event<T> Clone()
        {
            var clone = new Event<T>
            {
                IsBezier = IsBezier,
                EasingLeft = EasingLeft,
                EasingRight = EasingRight,
                Easing = Easing,
                Font = Font
            };

            // BezierPoints: 直接Array.Copy，避免LINQ的ToArray()分配
            if (BezierPoints != null)
            {
                var bp = new float[BezierPoints.Length];
                Array.Copy(BezierPoints, bp, BezierPoints.Length);
                clone.BezierPoints = bp;
            }

            // 针对已知T类型优化：int/float/double/byte直接赋值，byte[]/string特殊处理
            if (typeof(T) == typeof(int) || typeof(T) == typeof(float) ||
                typeof(T) == typeof(double) || typeof(T) == typeof(byte))
            {
                // 值类型直接赋值，无开销
                clone.StartValue = StartValue;
                clone.EndValue = EndValue;
            }
            else if (typeof(T) == typeof(byte[]))
            {
                // byte[]需要深拷贝
                clone.StartValue = StartValue != null ? (T)(object)((byte[])(object)StartValue).ToArray() : default(T);
                clone.EndValue = EndValue != null ? (T)(object)((byte[])(object)EndValue).ToArray() : default(T);
            }
            else if (typeof(T) == typeof(string))
            {
                // string是不可变类型，直接赋值
                clone.StartValue = StartValue;
                clone.EndValue = EndValue;
            }
            else
            {
                // 兜底：使用DeepClone（不应到达此处）
                clone.StartValue = DeepClone(StartValue);
                clone.EndValue = DeepClone(EndValue);
            }

            // Beat拷贝
            clone.StartBeat = new Beat((int[])StartBeat);
            clone.EndBeat = new Beat((int[])EndBeat);

            return clone;
        }
    }
}