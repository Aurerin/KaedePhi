using KaedePhi.Core.Common;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.RePhiEdit.Model;
using KaedePhi.Tool.Converter.RePhiEdit.Utils;

namespace KaedePhi.Tool.Converter.RePhiEdit;

/// <summary>
/// RePhiEdit 格式转换器。
/// </summary>
public class RePhiEditConverter : LoggableBase, IChartConverter<Rpe.Chart, Unit?, ConvertOption>
{
    /// <summary>
    /// 将 RePhiEdit 格式转换为 KPC 内部格式。
    /// </summary>
    /// <param name="source">RePhiEdit 谱面</param>
    /// <param name="_">未使用</param>
    /// <returns>KPC 谱面</returns>
    public Kpc.Chart ToKpc(Rpe.Chart source, Unit? _) =>
        new()
        {
            BpmList = source.BpmList.ConvertAll(ConvertBpmItem),
            Meta = Meta.ConvertMeta(source.Meta),
            JudgeLineList = source.JudgeLineList.ConvertAll(JudgeLine.ConvertJudgeLine),
        };

    /// <summary>
    /// 将 KPC 内部格式转换为 RePhiEdit 格式。
    /// </summary>
    /// <param name="input">KPC 谱面</param>
    /// <param name="options">输出转换选项</param>
    /// <returns>RePhiEdit 谱面</returns>
    public Rpe.Chart FromKpc(Kpc.Chart input, ConvertOption options) =>
        new()
        {
            BpmList = input.BpmList.ConvertAll(ConvertBpmItem),
            Meta = Meta.ConvertMeta(input.Meta),
            JudgeLineList = input.JudgeLineList.ConvertAll(r =>
                JudgeLine.ConvertJudgeLine(r, options.Cutting)
            ),
        };

    private static Kpc.BpmItem ConvertBpmItem(Rpe.BpmItem src) =>
        new() { Bpm = src.Bpm, StartBeat = new Beat((int[])src.StartBeat) };

    private static Rpe.BpmItem ConvertBpmItem(Kpc.BpmItem src) =>
        new() { Bpm = src.Bpm, StartBeat = new Beat((int[])src.StartBeat) };
}
