namespace KaedePhi.Tool.Converter.RePhiEdit.Utils;

public static class Control
{
    public static KpcControls.XControl ConvertXControl(RpeControls.XControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Pos = src.Pos };

    public static RpeControls.XControl ConvertXControl(KpcControls.XControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Pos = src.Pos };

    public static KpcControls.AlphaControl ConvertAlphaControl(RpeControls.AlphaControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Alpha = src.Alpha };

    public static RpeControls.AlphaControl ConvertAlphaControl(KpcControls.AlphaControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Alpha = src.Alpha };

    public static KpcControls.SizeControl ConvertSizeControl(RpeControls.SizeControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Size = src.Size };

    public static RpeControls.SizeControl ConvertSizeControl(KpcControls.SizeControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Size = src.Size };

    public static KpcControls.SkewControl ConvertSkewControl(RpeControls.SkewControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Skew = src.Skew };

    public static RpeControls.SkewControl ConvertSkewControl(KpcControls.SkewControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Skew = src.Skew };

    public static KpcControls.YControl ConvertYControl(RpeControls.YControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Y = src.Y };

    public static RpeControls.YControl ConvertYControl(KpcControls.YControl src) =>
        new() { Easing = Easing.ConvertEasing(src.Easing), X = src.X, Y = src.Y };
}