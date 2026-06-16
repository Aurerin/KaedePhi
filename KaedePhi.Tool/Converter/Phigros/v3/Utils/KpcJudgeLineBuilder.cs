using PhigrosJudgeLine = KaedePhi.Core.Phigros.v3.JudgeLine;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

/// <summary>
/// PhigrosV3 判定线到 KPC 判定线的构建器。
/// </summary>
public static class KpcJudgeLineBuilder
{
    public static Kpc.JudgeLine ConvertJudgeLine(PhigrosJudgeLine src, int index, float defaultBpm)
    {
        var horizonBeat = EventBuilder.GetJudgeLineHorizonBeat(src);

        return new Kpc.JudgeLine
        {
            Name = $"PhigrosLine_{index}",
            Notes = NoteBuilder.ConvertNotes(src.NotesAbove, src.NotesBelow),
            EventLayers = [EventLayerBuilder.ConvertEventLayer(src, horizonBeat)],
            BpmFactor = defaultBpm / src.Bpm,
        };
    }
}
