using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.Phigros.v3.Model;
using KaedePhi.Tool.Converter.Phigros.v3.Utils;
using KpcMeta = KaedePhi.Core.KaedePhi.Meta;
using PhigrosChart = KaedePhi.Core.Phigros.v3.Chart;

namespace KaedePhi.Tool.Converter.Phigros.v3;

public class PhigrosV3Converter : LoggableBase, IChartConverter<PhigrosChart, Unit?, KpcToPhigrosV3ConvertOptions>
{
    /// <summary>
    /// Phigros 格式默认 BPM（当谱面未提供 BPM 时使用）。
    /// </summary>
    private const float DefaultPhigrosBpm = 120f;

    public Kpc.Chart ToKpc(PhigrosChart input, Unit? options)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new Kpc.Chart
        {
            BpmList = BpmItem.ConvertBpmList(input.JudgeLineList),
            Meta = Meta.ConvertMeta(input),
            JudgeLineList = input.JudgeLineList?
                .Select((j, i) =>
                    JudgeLine.ConvertJudgeLine(j, i, input.JudgeLineList.Count > 0 ? input.JudgeLineList[0].Bpm : DefaultPhigrosBpm))
                .ToList() ?? []
        };
    }

    public PhigrosChart FromKpc(Kpc.Chart input, KpcToPhigrosV3ConvertOptions options)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(options);

        WarnIfUnsupportedMeta(input.Meta);

        var globalBpm = input.BpmList is { Count: > 0 } ? input.BpmList[0].Bpm : options.DefaultBpm;
        var chartEndBeat = CalculateChartEndBeat(input);

        var judgeLineConverter = new JudgeLineKpcToPhigrosV3(options, globalBpm, chartEndBeat, OnWarning);

        return new PhigrosChart
        {
            Offset = GetPhigrosV3Offset(input.Meta),
            JudgeLineList = input.JudgeLineList?
                .ConvertAll(j => judgeLineConverter.ConvertJudgeLine(j, input.JudgeLineList)) ?? []
        };
    }

    private static float CalculateChartEndBeat(Kpc.Chart input)
    {
        var maxBeat = 0d;

        if (input.JudgeLineList is { Count: > 0 })
        {
            foreach (var line in input.JudgeLineList)
            {
                if (line.Notes is { Count: > 0 })
                {
                    foreach (var note in line.Notes)
                    {
                        maxBeat = Math.Max(maxBeat, (double)note.EndBeat);
                    }
                }

                if (line.EventLayers is { Count: > 0 })
                {
                    foreach (var layer in line.EventLayers)
                    {
                        maxBeat = Math.Max(maxBeat, GetMaxEventEndBeat(layer.MoveXEvents));
                        maxBeat = Math.Max(maxBeat, GetMaxEventEndBeat(layer.MoveYEvents));
                        maxBeat = Math.Max(maxBeat, GetMaxEventEndBeat(layer.RotateEvents));
                        maxBeat = Math.Max(maxBeat, GetMaxEventEndBeat(layer.AlphaEvents));
                        maxBeat = Math.Max(maxBeat, GetMaxEventEndBeat(layer.SpeedEvents));
                    }
                }
            }
        }

        return (float)(maxBeat * 32) + 1f;
    }

    private static double GetMaxEventEndBeat<T>(List<Kpc.Event<T>>? events)
    {
        if (events is not { Count: > 0 }) return 0;
        return events.Max(e => (double)e.EndBeat);
    }

    private static float GetPhigrosV3Offset(KpcMeta meta) => meta.Offset / 1000f;

    private void WarnIfUnsupportedMeta(KpcMeta src)
    {
        var defaults = new KpcMeta();
        if (src.Background != defaults.Background)
            Warn($"PhigrosV3 不支持 Meta.Background（值='{src.Background}'）");
        if (src.Author != defaults.Author)
            Warn($"PhigrosV3 不支持 Meta.Author（值='{src.Author}'）");
        if (src.Composer != defaults.Composer)
            Warn($"PhigrosV3 不支持 Meta.Composer（值='{src.Composer}'）");
        if (src.Artist != defaults.Artist)
            Warn($"PhigrosV3 不支持 Meta.Artist（值='{src.Artist}'）");
        if (src.Level != defaults.Level)
            Warn($"PhigrosV3 不支持 Meta.Level（值='{src.Level}'）");
        if (src.Name != defaults.Name)
            Warn($"PhigrosV3 不支持 Meta.Name（值='{src.Name}'）");
        if (src.Song != defaults.Song)
            Warn($"PhigrosV3 不支持 Meta.Song（值='{src.Song}'）");
    }

    private void Warn(string message) => LogWarning($"[ToPhigrosV3] {message}");
}