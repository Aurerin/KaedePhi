using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter;

/// <summary>
/// 谱面格式转换器接口。
/// </summary>
/// <typeparam name="TPayload">目标谱面格式</typeparam>
/// <typeparam name="TInOptions">输入转换选项</typeparam>
/// <typeparam name="TOutOptions">输出转换选项</typeparam>
public interface IChartConverter<TPayload, in TInOptions, in TOutOptions> : ILoggable
{
    /// <summary>
    /// 将外部格式转换为 KPC 内部格式。
    /// </summary>
    /// <param name="input">源谱面</param>
    /// <param name="options">转换选项</param>
    /// <returns>KPC 谱面</returns>
    Kpc.Chart ToKpc(TPayload input, TInOptions options);

    /// <summary>
    /// 将 KPC 内部格式转换为外部格式。
    /// </summary>
    /// <param name="input">KPC 谱面</param>
    /// <param name="options">转换选项</param>
    /// <returns>目标格式谱面</returns>
    TPayload FromKpc(Kpc.Chart input, TOutOptions options);
}
