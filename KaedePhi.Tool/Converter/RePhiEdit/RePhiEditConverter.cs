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
    private CancellationToken _ct;

    /// <summary>
    /// 设置取消令牌。
    /// </summary>
    public void SetCancellationToken(CancellationToken ct) => _ct = ct;

    /// <summary>
    /// 将 RePhiEdit 格式转换为 KPC 内部格式。
    /// </summary>
    /// <param name="source">RePhiEdit 谱面</param>
    /// <param name="_">未使用</param>
    /// <returns>KPC 谱面</returns>
    public Kpc.Chart ToKpc(Rpe.Chart source, Unit? _)
    {
        _ct.ThrowIfCancellationRequested();
        return new Kpc.Chart
        {
            BpmList = source.BpmList.ConvertAll(ConvertBpmItem),
            Meta = MetaBuilder.ConvertMeta(source.Meta),
            JudgeLineList = ConvertJudgeLinesWithCancellation(source.JudgeLineList),
        };
    }

    private List<Kpc.JudgeLine> ConvertJudgeLinesWithCancellation(List<Rpe.JudgeLine> judgeLines)
    {
        var result = new List<Kpc.JudgeLine>(judgeLines.Count);
        for (var i = 0; i < judgeLines.Count; i++)
        {
            _ct.ThrowIfCancellationRequested();
            result.Add(JudgeLineBuilder.ConvertJudgeLine(judgeLines[i]));
        }
        return result;
    }

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
            Meta = MetaBuilder.ConvertMeta(input.Meta),
            JudgeLineList = input.JudgeLineList.ConvertAll(r =>
                JudgeLineBuilder.ConvertJudgeLine(r, options.Cutting)
            ),
        };

    private static Kpc.BpmItem ConvertBpmItem(Rpe.BpmItem src) =>
        new() { Bpm = src.Bpm, StartBeat = new Beat((int[])src.StartBeat) };

    private static Rpe.BpmItem ConvertBpmItem(Kpc.BpmItem src) =>
        new() { Bpm = src.Bpm, StartBeat = new Beat((int[])src.StartBeat) };
}
