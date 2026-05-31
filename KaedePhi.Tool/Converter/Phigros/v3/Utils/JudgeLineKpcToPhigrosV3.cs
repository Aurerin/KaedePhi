using KaedePhi.Core.Common;
using KaedePhi.Tool.Converter.Phigros.v3.Model;
using global::KaedePhi.Tool.JudgeLines.KaedePhi;
using KpcJudgeLine = KaedePhi.Core.KaedePhi.JudgeLine;
using KpcSpeedEvent = KaedePhi.Core.KaedePhi.Events.Event<float>;
using PhigrosEvent = KaedePhi.Core.Phigros.v3.Event;
using PhigrosJudgeLine = KaedePhi.Core.Phigros.v3.JudgeLine;
using PhigrosSpeedEvent = KaedePhi.Core.Phigros.v3.SpeedEvent;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

public class JudgeLineKpcToPhigrosV3
{
    private const float FloatEpsilon = 1e-6f;
    private readonly KpcToPhigrosV3ConvertOptions _options;
    private readonly EventKpcToPhigrosV3 _eventConverter;
    private readonly float _globalBpm;
    private readonly float _chartEndBeat;
    private readonly Action<string>? _warnLogger;

    public JudgeLineKpcToPhigrosV3(KpcToPhigrosV3ConvertOptions options, float globalBpm, float chartEndBeat,
        Action<string>? warnLogger)
    {
        _options = options;
        _eventConverter = new EventKpcToPhigrosV3(options, warnLogger);
        _globalBpm = globalBpm;
        _chartEndBeat = chartEndBeat;
        _warnLogger = warnLogger;
    }

    public PhigrosJudgeLine ConvertJudgeLine(KpcJudgeLine src, List<KpcJudgeLine> allLine)
    {
        WarnIfUnsupportedJudgeLineFields(src);
        var trueSrc = src;

        if (!string.Equals(trueSrc.Texture, "line.png", StringComparison.Ordinal) ||
            _options.LineFilter.RemoveTextureLine || trueSrc.AttachUi.HasValue ||
            _options.LineFilter.RemoveAttachUiLine)
        {
            return new PhigrosJudgeLine()
            {
                Bpm = _globalBpm / trueSrc.BpmFactor
            };
        }

        if (trueSrc.Father != -1)
        {
            Warn($"PhigrosV3 不支持 JudgeLine.Father（值={src.Father}），将自动解除父子绑定");
            var unbinder = new JudgeLineUnbinder();
            if (_options.FatherLineUnbind.ClassicMode)
            {
                trueSrc = unbinder.FatherUnbind(allLine.FindIndex(l => l.GetHashCode() == src.GetHashCode()),
                    allLine, _options.FatherLineUnbind.Precision);
            }
            else
                trueSrc = unbinder.FatherUnbind(
                    allLine.FindIndex(l => l.GetHashCode() == src.GetHashCode()),
                    allLine, _options.FatherLineUnbind.Precision, _options.FatherLineUnbind.Tolerance);
        }

        var lineBpm = _globalBpm / trueSrc.BpmFactor;
        var speedEvents = CollectSpeedEvents(trueSrc.EventLayers);
        var (notesAbove, notesBelow) = NoteKpcToPhigrosV3.ConvertNotes(trueSrc.Notes, speedEvents, _warnLogger);

        var phigrosLine = new PhigrosJudgeLine
        {
            Bpm = lineBpm,
            NotesAbove = notesAbove,
            NotesBelow = notesBelow,
        };

        _eventConverter.ConvertLineEvents(phigrosLine, trueSrc.EventLayers ?? []);

        if (phigrosLine.JudgeLineDisappearEvents.Count == 0)
        {
            phigrosLine.JudgeLineDisappearEvents.Add(new PhigrosEvent
            {
                StartTime = 0,
                EndTime = _chartEndBeat,
                Start = 0f,
                End = 0f
            });
        }

        return phigrosLine;
    }

    private static List<KpcSpeedEvent> CollectSpeedEvents(List<KpcEvents.EventLayer>? layers)
    {
        if (layers is not { Count: > 0 }) return [];

        var firstLayer = layers[0];
        if (firstLayer.SpeedEvents is not { Count: > 0 }) return [];

        return firstLayer.SpeedEvents.OrderBy(e => (double)e.StartBeat).ToList();
    }

    private void WarnIfUnsupportedJudgeLineFields(KpcJudgeLine src)
    {
        var textureRemoveHint  = _options.LineFilter.RemoveTextureLine  ? "判定线将被自动移除。" : "";
        var attachUiRemoveHint = _options.LineFilter.RemoveAttachUiLine ? "，判定线将被自动移除。" : "";

        if (!string.Equals(src.Name, "Untitled", StringComparison.Ordinal))
            Warn($"PhigrosV3 不支持 JudgeLine.Name（值='{src.Name}'）");
        if (!string.Equals(src.Texture, "line.png", StringComparison.Ordinal))
            Warn($"PhigrosV3 不支持 JudgeLine.Texture（值='{src.Texture}'），{textureRemoveHint}");
        if (!IsDefaultAnchor(src.Anchor))
            Warn($"PhigrosV3 不支持 JudgeLine.Anchor（值='[{string.Join(", ", src.Anchor)}]'）");
        if (src.Father != -1)
            Warn($"PhigrosV3 不支持 JudgeLine.Father（值={src.Father}），将自动解除父子绑定");
        if (!src.IsCover)
            Warn($"PhigrosV3 不支持 JudgeLine.IsCover（值={src.IsCover}）");
        if (src.ZOrder != 0)
            Warn($"PhigrosV3 不支持 JudgeLine.ZOrder（值={src.ZOrder}）");
        if (src.AttachUi.HasValue)
            Warn($"PhigrosV3 不支持 JudgeLine.AttachUi（值={(int)src.AttachUi.Value}）{attachUiRemoveHint}");
        if (src.IsGif)
            Warn($"PhigrosV3 不支持 JudgeLine.IsGif（值={src.IsGif}）");
        if (src.RotateWithFather)
            Warn($"PhigrosV3 不支持 JudgeLine.RotateWithFather（值={src.RotateWithFather}）");

        WarnIfUnsupportedControlFields(src);
    }

    /// <summary>
    /// 检查判定线中 PhigrosV3 不支持的控件字段，并逐项告警。
    /// </summary>
    private void WarnIfUnsupportedControlFields(KpcJudgeLine src)
    {
        if (HasNonDefaultExtendLayer(src.Extended))
            Warn("PhigrosV3 不支持 JudgeLine.Extended（包含非默认数据）");
        if (!IsDefaultControls(src.PositionControls))
            Warn("PhigrosV3 不支持 JudgeLine.PositionControls（包含非默认数据）");
        if (!IsDefaultControls(src.AlphaControls))
            Warn("PhigrosV3 不支持 JudgeLine.AlphaControls（包含非默认数据）");
        if (!IsDefaultControls(src.SizeControls))
            Warn("PhigrosV3 不支持 JudgeLine.SizeControls（包含非默认数据）");
        if (!IsDefaultControls(src.SkewControls))
            Warn("PhigrosV3 不支持 JudgeLine.SkewControls（包含非默认数据）");
        if (!IsDefaultControls(src.YControls))
            Warn("PhigrosV3 不支持 JudgeLine.YControls（包含非默认数据）");
    }

    private static bool HasNonDefaultExtendLayer(KpcEvents.ExtendLayer? layer)
        => layer != null
           && ((layer.ColorEvents?.Count ?? 0) > 0
               || (layer.ScaleXEvents?.Count ?? 0) > 0
               || (layer.ScaleYEvents?.Count ?? 0) > 0
               || (layer.TextEvents?.Count ?? 0) > 0
               || (layer.PaintEvents?.Count ?? 0) > 0
               || (layer.GifEvents?.Count ?? 0) > 0);

    private static bool IsDefaultAnchor(float[]? anchor)
        => anchor is { Length: 2 }
           && Math.Abs(anchor[0] - 0.5f) <= FloatEpsilon
           && Math.Abs(anchor[1] - 0.5f) <= FloatEpsilon;

    private static bool IsDefaultControls<T>(List<T>? controls) where T : class
    {
        if (controls == null) return true;
        return controls.Count == 0;
    }

    private void Warn(string message) => _warnLogger?.Invoke($"[ToPhigrosV3] {message}");
}