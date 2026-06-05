using KaedePhi.Tool.Converter.PhiEdit.Utils;

namespace KaedePhi.Tool.Converter.RePhiEdit.Utils;

public static class Easing
{
    public static Rpe.Easing ConvertEasing(Kpc.Easing src, bool isBezier)
        => isBezier ? new Rpe.Easing(1) : new Rpe.Easing(MapToPe((int)src));
    public static Kpc.Easing ConvertEasing(Rpe.Easing src) => new(MapToKpc((int)src));
    public static Rpe.Easing ConvertEasing(Kpc.Easing src) => new(MapToPe((int)src));

    public static int MapToKpc(int rpe) => rpe switch
    {
        1 => 1,
        2 => 3,
        3 => 2,
        4 => 6,
        5 => 5,
        6 => 4,
        7 => 7,
        8 => 9,
        9 => 8,
        10 => 12,
        11 => 11,
        12 => 10,
        13 => 13,
        14 => 15,
        15 => 14,
        16 => 18,
        17 => 17,
        18 => 21,
        19 => 20,
        20 => 24,
        21 => 23,
        22 => 22,
        23 => 25,
        24 => 27,
        25 => 26,
        26 => 30,
        27 => 29,
        28 => 31,
        29 => 28,
        _ => 1
    };

    public static int MapToPe(int nrcEasing)
        => EasingConverter.MapToPe(nrcEasing);
}