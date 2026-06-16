using KaedePhi.Core.Common;
using PhigrosJudgeLine = KaedePhi.Core.Phigros.v3.JudgeLine;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

/// <summary>
/// PhigrosV3 BPM 项到 KPC BPM 项的转换工具。
/// </summary>
public static class BpmItemBuilder
{
    public static List<Kpc.BpmItem> ConvertBpmList(List<PhigrosJudgeLine> judgeLines)
    {
        if (judgeLines is not { Count: > 0 })
            return [new Kpc.BpmItem { Bpm = 120f, StartBeat = new Beat(0) }];

        return [new Kpc.BpmItem { Bpm = judgeLines[0].Bpm, StartBeat = new Beat(0) }];
    }
}
