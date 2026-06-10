namespace KaedePhi.Tool.Converter.PhiChain.Model;

/// <summary>
/// Phichain 转 KPC 的转换选项。
/// </summary>
public class PhichainToKpcConvertOptions
{
    /// <summary>
    /// 不支持的缓动切段精度（每拍细分数量），默认 64。
    /// 传入 64 表示每 1/64 拍采样一次。
    /// </summary>
    public int UnsupportedEasingPrecision { get; set; } = 64;
}

/// <summary>
/// KPC 转 Phichain 的转换选项。
/// </summary>
public class KpcToPhichainConvertOptions
{
    /// <summary>
    /// 是否对 rotateWithFather 为 false 的判定线进行父子线解绑。
    /// </summary>
    public bool UnbindNonRotatingChildren { get; set; } = true;
}
