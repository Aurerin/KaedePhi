namespace KaedePhi.Tool.Gui.Models;

public sealed class GuiAppConfig
{
    public int MaxLogFiles { get; set; } = 5;
    public ToolDefaultsConfig Unbind { get; set; } = new()
    {
        Precision = 64, Tolerance = 0.1, ClassicMode = false, DisableCompress = false
    };
    public ToolDefaultsConfig LayerMerge { get; set; } = new()
    {
        Precision = 64, Tolerance = 0.1, ClassicMode = false, DisableCompress = false
    };
    public ToolDefaultsConfig Cut { get; set; } = new()
    {
        Precision = 64, Tolerance = 0.1, DisableCompress = false
    };
    public ToolDefaultsConfig Fit { get; set; } = new()
    {
        Tolerance = 0.5
    };
    public RenderDefaultsConfig Render { get; set; } = new();
}

public sealed class ToolDefaultsConfig
{
    public double Precision { get; set; } = 64;
    public double Tolerance { get; set; } = 0.1;
    public bool ClassicMode { get; set; }
    public bool DisableCompress { get; set; }
}

public sealed class RenderDefaultsConfig
{
    public int PixelsPerBeat { get; set; } = 100;
    public int ChannelWidth { get; set; } = 150;
    public int SamplesPerEvent { get; set; } = 64;
    public int BeatSubdivisions { get; set; } = 2;
}
