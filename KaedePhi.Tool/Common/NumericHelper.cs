using System.Runtime.CompilerServices;

namespace KaedePhi.Tool.Common;

/// <summary>
/// 为 int/float/double 提供无装箱的算术与类型转换操作，替代 dynamic 分发。
/// </summary>
public static class NumericHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Add<T>(T left, T right)
    {
        if (typeof(T) == typeof(double))
            return (T)(object)((double)(object)left! + (double)(object)right!);
        if (typeof(T) == typeof(float))
            return (T)(object)((float)(object)left! + (float)(object)right!);
        if (typeof(T) == typeof(int))
            return (T)(object)((int)(object)left! + (int)(object)right!);
        throw new NotSupportedException($"NumericHelper.Add does not support type {typeof(T)}.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToDouble<T>(T value)
    {
        if (typeof(T) == typeof(double))
            return (double)(object)value!;
        if (typeof(T) == typeof(float))
            return (float)(object)value!;
        if (typeof(T) == typeof(int))
            return (int)(object)value!;
        throw new NotSupportedException($"NumericHelper.ToDouble does not support type {typeof(T)}.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDefault<T>(T value)
    {
        if (typeof(T) == typeof(double))
            return (double)(object)value! == 0.0;
        if (typeof(T) == typeof(float))
            return (float)(object)value! == 0.0f;
        if (typeof(T) == typeof(int))
            return (int)(object)value! == 0;
        return EqualityComparer<T>.Default.Equals(value, default);
    }
}
