using KaedePhi.Core.Common;

namespace KaedePhi.Tool.Converter.PhiEdit.Utils;

/// <summary>
/// PE 与 KPC BPM 项之间的双向转换工具。
/// </summary>
public static class BpmItemBuilder
{
    public static Kpc.BpmItem ConvertBpmItem(Pe.BpmItem src) =>
        new() { Bpm = src.Bpm, StartBeat = new Beat(src.StartBeat) };

    public static Pe.BpmItem ConvertBpmItem(Kpc.BpmItem src) =>
        new() { Bpm = src.Bpm, StartBeat = (float)(double)src.StartBeat };
}
