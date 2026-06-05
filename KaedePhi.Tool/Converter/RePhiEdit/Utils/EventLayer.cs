using KaedePhi.Tool.Converter.RePhiEdit.Model;

namespace KaedePhi.Tool.Converter.RePhiEdit.Utils;

public static class EventLayer
{
    #region RpeToKpc

    public static KpcEvents.EventLayer ConvertEventLayer(RpeEvents.EventLayer src)
    {
        var result = new KpcEvents.EventLayer();
        if (src.MoveXEvents is not null)
            result.MoveXEvents =
                src.MoveXEvents.ConvertAll(e => Event.ConvertFloatToDoubleEvent(e, Transform.TransformToKpcX));
        if (src.MoveYEvents is not null)
            result.MoveYEvents =
                src.MoveYEvents.ConvertAll(e => Event.ConvertFloatToDoubleEvent(e, Transform.TransformToKpcY));
        if (src.RotateEvents is not null)
            result.RotateEvents =
                src.RotateEvents.ConvertAll(e => Event.ConvertFloatToDoubleEvent(e, Transform.TransformToKpcAngle));
        if (src.AlphaEvents is not null) result.AlphaEvents = src.AlphaEvents.ConvertAll(Event.ConvertIntEvent);
        if (src.SpeedEvents is not null) result.SpeedEvents = src.SpeedEvents.ConvertAll(Event.ConvertFloatEvent);
        return result;
    }

    public static KpcEvents.ExtendLayer? ConvertExtendLayer(RpeEvents.ExtendLayer? src)
    {
        if (src == null) return null;
        var result = new KpcEvents.ExtendLayer();
        if (src.ColorEvents is not null) result.ColorEvents = src.ColorEvents.ConvertAll(Event.ConvertByteArrayEvent);
        if (src.ScaleXEvents is not null) result.ScaleXEvents = src.ScaleXEvents.ConvertAll(Event.ConvertFloatEvent);
        if (src.ScaleYEvents is not null) result.ScaleYEvents = src.ScaleYEvents.ConvertAll(Event.ConvertFloatEvent);
        if (src.TextEvents is not null) result.TextEvents = src.TextEvents.ConvertAll(Event.ConvertStringEvent);
        if (src.PaintEvents is not null) result.PaintEvents = src.PaintEvents.ConvertAll(Event.ConvertFloatEvent);
        if (src.GifEvents is not null) result.GifEvents = src.GifEvents.ConvertAll(Event.ConvertFloatEvent);
        if (src.InclineEvents is not null) result.InclineEvents = src.InclineEvents.ConvertAll(Event.ConvertFloatEvent);
        return result;
    }

    #endregion

    #region KpcToRpe

    public static RpeEvents.EventLayer ConvertEventLayer(KpcEvents.EventLayer src, ConvertOption.CuttingOptions options)
    {
        var rpe = new RpeEvents.EventLayer();
        if (src.MoveXEvents is not null)
        {
            rpe.MoveXEvents = [];
            foreach (var e in src.MoveXEvents)
                rpe.MoveXEvents.AddRange(Event.ConvertDoubleEventExpanding(e, options, Transform.TransformToRpeX));
        }

        if (src.MoveYEvents is not null)
        {
            rpe.MoveYEvents = [];
            foreach (var e in src.MoveYEvents)
                rpe.MoveYEvents.AddRange(Event.ConvertDoubleEventExpanding(e, options, Transform.TransformToRpeY));
        }

        if (src.RotateEvents is not null)
        {
            rpe.RotateEvents = [];
            foreach (var e in src.RotateEvents)
                rpe.RotateEvents.AddRange(Event.ConvertDoubleEventExpanding(e, options, Transform.TransformToRpeAngle));
        }

        if (src.AlphaEvents is not null)
        {
            rpe.AlphaEvents = [];
            foreach (var e in src.AlphaEvents) rpe.AlphaEvents.AddRange(Event.ConvertIntEventExpanding(e, options));
        }

        if (src.SpeedEvents is not null)
        {
            rpe.SpeedEvents = [];
            foreach (var e in src.SpeedEvents) rpe.SpeedEvents.AddRange(Event.ConvertFloatEventExpanding(e, options));
        }

        return rpe;
    }

    public static RpeEvents.ExtendLayer? ConvertExtendLayer(KpcEvents.ExtendLayer? src,
        ConvertOption.CuttingOptions options)
    {
        if (src is null) return null;
        var rpe = new RpeEvents.ExtendLayer();

        rpe.ColorEvents = src.ColorEvents.ConvertAll(Event.ConvertByteArrayEvent);
        rpe.ScaleXEvents = [];
        foreach (var e in src.ScaleXEvents)
            rpe.ScaleXEvents.AddRange(Event.ConvertFloatEventExpanding(e, options));
        rpe.ScaleYEvents = [];
        foreach (var e in src.ScaleYEvents)
            rpe.ScaleYEvents.AddRange(Event.ConvertFloatEventExpanding(e, options));
        rpe.TextEvents = src.TextEvents.ConvertAll(Event.ConvertStringEvent);
        rpe.PaintEvents = [];
        foreach (var e in src.PaintEvents)
            rpe.PaintEvents.AddRange(Event.ConvertFloatEventExpanding(e, options));
        rpe.GifEvents = [];
        foreach (var e in src.GifEvents)
            rpe.GifEvents.AddRange(Event.ConvertFloatEventExpanding(e, options));
        rpe.InclineEvents = [];
        foreach (var e in src.InclineEvents)
            rpe.InclineEvents.AddRange(Event.ConvertFloatEventExpanding(e, options));
        return rpe;
    }

    #endregion
}