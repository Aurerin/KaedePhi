using KaedePhi.Core.Common;
using KpcNote = KaedePhi.Core.KaedePhi.Note;
using KpcNoteType = KaedePhi.Core.KaedePhi.NoteType;
using KpcSpeedEvent = KaedePhi.Core.KaedePhi.Events.Event<float>;
using PhigrosNote = KaedePhi.Core.Phigros.v3.Note;
using PhigrosNoteType = KaedePhi.Core.Phigros.v3.NoteType;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

public static class NoteKpcToPhigrosV3
{
    private const float NotePositionXRatio = 0.1125f;
    private const float FloatEpsilon = 1e-6f;
    private const double SpeedValueRatio = 4.5d;

    public static (List<PhigrosNote> above, List<PhigrosNote> below) ConvertNotes(
        List<KpcNote>? notes, List<KpcSpeedEvent>? speedEvents, Action<string>? warnLogger)
    {
        if (notes is not { Count: > 0 }) return ([], []);

        var above = new List<PhigrosNote>();
        var below = new List<PhigrosNote>();

        foreach (var note in notes)
        {
            var converted = ConvertNote(note, speedEvents, warnLogger);
            if (note.Above)
                above.Add(converted);
            else
                below.Add(converted);
        }

        return (above, below);
    }

    public static PhigrosNote ConvertNote(KpcNote src, List<KpcSpeedEvent>? speedEvents, Action<string>? warnLogger)
    {
        WarnIfUnsupportedNoteFields(src, warnLogger);

        var startBeat = (double)src.StartBeat;
        var time = (int)Math.Round(startBeat * 32);

        var phigrosType = ConvertNoteType(src.Type);

        float holdTime = 0;
        float speed = src.SpeedMultiplier;

        if (phigrosType == PhigrosNoteType.Hold)
        {
            var endBeat = (double)src.EndBeat;
            holdTime = (float)((endBeat - startBeat) * 32);
            speed = GetSpeedAtBeat(speedEvents, endBeat);
        }

        return new PhigrosNote
        {
            Type = phigrosType,
            Time = time,
            PositionX = (float)(src.PositionX / NotePositionXRatio),
            HoldTime = holdTime,
            Speed = speed
        };
    }

    private static float GetSpeedAtBeat(List<KpcSpeedEvent>? speedEvents, double beat)
    {
        if (speedEvents is not { Count: > 0 })
            return 1f;

        var beatObj = new Beat(beat);
        foreach (var ev in speedEvents)
        {
            var startBeat = (double)ev.StartBeat;
            var endBeat = (double)ev.EndBeat;

            if (beat >= startBeat - FloatEpsilon && beat < endBeat - FloatEpsilon)
            {
                return (float)(ev.GetValueAtBeat(beatObj) / SpeedValueRatio);
            }
        }

        return (float)(speedEvents[^1].EndValue / SpeedValueRatio);
    }

    public static PhigrosNoteType ConvertNoteType(KpcNoteType type) => type switch
    {
        KpcNoteType.Tap => PhigrosNoteType.Tap,
        KpcNoteType.Hold => PhigrosNoteType.Hold,
        KpcNoteType.Flick => PhigrosNoteType.Flick,
        KpcNoteType.Drag => PhigrosNoteType.Drag,
        _ => PhigrosNoteType.Tap
    };

    private static void WarnIfUnsupportedNoteFields(KpcNote src, Action<string>? warnLogger)
    {
        if (src.Alpha != 255)
            Warn($"PhigrosV3 不支持 Note.Alpha（值={src.Alpha}）");
        if (Math.Abs(src.JudgeArea - 1f) > FloatEpsilon)
            Warn($"PhigrosV3 不支持 Note.JudgeArea（值={src.JudgeArea}）");
        if (Math.Abs(src.VisibleTime - 999999f) > FloatEpsilon)
            Warn($"PhigrosV3 不支持 Note.VisibleTime（值={src.VisibleTime}）");
        if (Math.Abs(src.YOffset) > FloatEpsilon)
            Warn($"PhigrosV3 不支持 Note.YOffset（值={src.YOffset}）");
        if (!IsDefaultTint(src.Tint))
            Warn($"PhigrosV3 不支持 Note.Tint（值='[{string.Join(", ", src.Tint)}]'）");
        if (src.HitFxColor != null)
            Warn($"PhigrosV3 不支持 Note.HitFxColor（值='[{string.Join(", ", src.HitFxColor)}]'）");
        if (!string.IsNullOrWhiteSpace(src.HitSound))
            Warn($"PhigrosV3 不支持 Note.HitSound（值='{src.HitSound}'）");
        if (src.IsFake)
            Warn($"PhigrosV3 不支持 Note.IsFake（值={src.IsFake}）");
        if (Math.Abs(src.WidthRatio - 1f) > FloatEpsilon)
            Warn($"PhigrosV3 不支持 Note.WidthRatio（值={src.WidthRatio}）");
        return;
        void Warn(string message) => warnLogger?.Invoke($"[ToPhigrosV3] {message}");
    }

    private static bool IsDefaultTint(byte[]? tint) => tint is [255, 255, 255];
}
