namespace KaedePhi.Tool.Converter.RePhiEdit.Utils;

/// <summary>
/// RPE 与 KPC 元数据之间的双向转换工具。
/// </summary>
public static class MetaBuilder
{
    public static Kpc.Meta ConvertMeta(Rpe.Meta src) =>
        new()
        {
            Background = src.Background,
            Author = src.Charter,
            Composer = src.Composer,
            Artist = src.Illustration,
            Level = src.Level,
            Name = src.Name,
            Offset = src.Offset,
            Song = src.Song,
        };

    public static Rpe.Meta ConvertMeta(Kpc.Meta src) =>
        new()
        {
            Background = src.Background,
            Charter = src.Author,
            Composer = src.Composer,
            Illustration = src.Artist,
            Level = src.Level,
            Name = src.Name,
            Offset = src.Offset,
            Song = src.Song,
        };
}
