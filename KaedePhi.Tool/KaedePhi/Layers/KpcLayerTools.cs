using EventLayer = KaedePhi.Core.KaedePhi.EventLayer;

namespace KaedePhi.Tool.KaedePhi.Layers;

/// <summary>
/// NRC 格式层级操作工具。提供多层级的合并与切割功能。
/// </summary>
[Obsolete("请改用 KaedePhi.Tool.Layer.KaedePhi.KpcLayerProcessor")]
public static class KpcLayerTools
{
    [Obsolete("请改用 KpcLayerProcessor.LayerMerge")]
    public static EventLayer LayerMerge(
        List<EventLayer> layers, double precision = 64d)
        => new Layer.KaedePhi.KpcLayerProcessor().LayerMerge(layers, precision);

    [Obsolete("请改用 KpcLayerProcessor.LayerMergePlus")]
    public static EventLayer LayerMergePlus(
        List<EventLayer> layers, double precision = 64d, double tolerance = 5d)
        => new Layer.KaedePhi.KpcLayerProcessor().LayerMergePlus(layers, precision, tolerance);

    [Obsolete("请改用 KpcLayerProcessor.CutLayerEvents")]
    public static EventLayer CutLayerEvents(
        EventLayer layer, double precision = 64d)
        => new Layer.KaedePhi.KpcLayerProcessor().CutLayerEvents(layer, precision);

    [Obsolete("请改用 KpcLayerProcessor.CutLayerEvents")]
    public static List<EventLayer> CutLayerEvents(
        List<EventLayer> layers, double precision = 64d)
        => new Layer.KaedePhi.KpcLayerProcessor().CutLayerEvents(layers, precision);

    [Obsolete("请改用 KpcLayerProcessor.LayerEventsCompress")]
    public static void LayerEventsCompress(
        EventLayer layer, double tolerance = 0.1d)
        => new Layer.KaedePhi.KpcLayerProcessor().LayerEventsCompress(layer, tolerance);
}

