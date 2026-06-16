using PhigrosChart = KaedePhi.Core.Phigros.v3.Chart;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

/// <summary>
/// PhigrosV3 元数据到 KPC 元数据的转换工具。
/// </summary>
public static class MetaBuilder
{
    public static Kpc.Meta ConvertMeta(PhigrosChart src) =>
        new() { Offset = (int)(src.Offset * 1000) };
}
