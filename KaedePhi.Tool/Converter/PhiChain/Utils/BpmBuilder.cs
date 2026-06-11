using KaedePhi.Core.Common;
using KaedePhi.Core.PhiChain.v6;

namespace KaedePhi.Tool.Converter.PhiChain.Utils;

/// <summary>
/// PhiChain 与 KPC BPM 之间的双向转换工具。
/// </summary>
public static class BpmBuilder
{
    /// <summary>
    /// 将 PhiChain BPM 点转换为 KPC BPM 项。
    /// </summary>
    /// <param name="src">PhiChain BPM 点</param>
    /// <returns>KPC BPM 项</returns>
    public static Kpc.BpmItem ConvertBpmPoint(BpmPoint src) =>
        new()
        {
            Bpm = src.Bpm,
            StartBeat = new Beat((int[])src.Beat),
        };

    /// <summary>
    /// 将 KPC BPM 项转换为 PhiChain BPM 点。
    /// </summary>
    /// <param name="src">KPC BPM 项</param>
    /// <returns>PhiChain BPM 点</returns>
    public static BpmPoint ConvertBpmItem(Kpc.BpmItem src) =>
        new()
        {
            Bpm = src.Bpm,
            Beat = new Beat((int[])src.StartBeat),
        };
}
