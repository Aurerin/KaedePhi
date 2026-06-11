using KaedePhi.Core.PhiChain.v6;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.PhiChain.Model;
using KaedePhi.Tool.Converter.PhiChain.Utils;

namespace KaedePhi.Tool.Converter.PhiChain;

/// <summary>
/// PhiChain 格式转换器。
/// </summary>
public class PhiChainConverter : LoggableBase, IChartConverter<Chart, PhiChainToKpcConvertOptions, KpcToPhiChainConvertOptions>
{
    /// <summary>
    /// 将 PhiChain 格式转换为 KPC 内部格式。
    /// </summary>
    /// <param name="source">PhiChain 谱面</param>
    /// <param name="options">转换选项</param>
    /// <returns>KPC 谱面</returns>
    public Kpc.Chart ToKpc(Chart source, PhiChainToKpcConvertOptions options)
    {
        var kpcChart = new Kpc.Chart
        {
            BpmList = source.BpmList.ConvertAll(BpmBuilder.ConvertBpmPoint),
            Meta = new Kpc.Meta
            {
                Offset = (int)source.Offset, // PhiChain 和 KPC 的 offset 单位均为毫秒
            }
        };

        // 展开树形线结构为扁平列表
        var lineIndex = 0;
        foreach (var line in source.Lines)
        {
            JudgeLineBuilder.FlattenLine(line, -1, kpcChart.JudgeLineList, ref lineIndex, options);
        }

        return kpcChart;
    }

    /// <summary>
    /// 将 KPC 内部格式转换为 PhiChain 格式。
    /// </summary>
    /// <param name="input">KPC 谱面</param>
    /// <param name="options">输出转换选项</param>
    /// <returns>PhiChain 谱面</returns>
    public Chart FromKpc(Kpc.Chart input, KpcToPhiChainConvertOptions options)
    {
        var chart = new Chart
        {
            Offset = input.Meta.Offset, // PhiChain 和 KPC 的 offset 单位均为毫秒
            BpmList = new BpmList(input.BpmList.ConvertAll(BpmBuilder.ConvertBpmItem)),
            // 构建父子关系树
            Lines = JudgeLineBuilder.BuildLineTree(input.JudgeLineList, options)
        };

        return chart;
    }
}
