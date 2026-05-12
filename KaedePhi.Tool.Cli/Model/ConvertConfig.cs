using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Cli.Model;

/// <summary>
/// 格式转换命令默认配置
/// </summary>
public class ConvertConfig
{
    /// <summary>
    /// 转换目标格式
    /// </summary>
    public ChartType TargetType { get; set; } = ChartType.RePhiEdit;

    /// <summary>
    /// 是否美化格式化输出 JSON（仅在输出为文件时生效）
    /// </summary>
    public bool FormatOutput { get; set; } = false;

    /// <summary>
    /// 是否流式输出到文件（大文件推荐）
    /// </summary>
    public bool StreamOutput { get; set; } = false;

    /// <summary>
    /// 是否为干运行（仅计算不写入）
    /// </summary>
    public bool DryRun { get; set; } = false;

    // ---- PhiEdit 转换选项 ----

    /// <summary>
    /// PE 速度帧值到 KPC 速度事件值的转换比率
    /// </summary>
    public double PeSpeedConversionRatio { get; set; } = 14d / 9d;

    /// <summary>
    /// PE 尾部拍填充量（拍）
    /// </summary>
    public double PeTrailingBeatPadding { get; set; } = 1d / 64d;

    // ---- KPC -> PhiEdit 转换选项 ----

    /// <summary>
    /// 非支持缓动切割精度
    /// </summary>
    public double PeUnsupportedEasingPrecision { get; set; } = 64d;

    /// <summary>
    /// 非对齐 XY 事件切割精度
    /// </summary>
    public double PeMisalignedXyEventPrecision { get; set; } = 64d;

    /// <summary>
    /// Alpha 事件切割精度
    /// </summary>
    public double PeAlphaCutPrecision { get; set; } = 64d;

    /// <summary>
    /// Alpha 事件切割后是否压缩
    /// </summary>
    public bool PeAlphaCutCompress { get; set; } = true;

    /// <summary>
    /// Alpha 事件切割后压缩容差百分比
    /// </summary>
    public double PeAlphaCutTolerance { get; set; } = 0.1d;

    /// <summary>
    /// 速度事件切割精度
    /// </summary>
    public double PeSpeedCutPrecision { get; set; } = 64d;

    /// <summary>
    /// 速度事件切割后是否压缩
    /// </summary>
    public bool PeSpeedCutCompress { get; set; } = true;

    /// <summary>
    /// 速度事件切割后压缩容差百分比
    /// </summary>
    public double PeSpeedCutTolerance { get; set; } = 0.1d;

    // ---- KPC -> PhigrosV3 转换选项 ----

    /// <summary>
    /// PhigrosV3 默认 BPM（当谱面 BPM 列表为空时使用）
    /// </summary>
    public float PhigrosDefaultBpm { get; set; } = 120f;

    /// <summary>
    /// PhigrosV3 非支持缓动切割精度
    /// </summary>
    public double PhigrosEasingPrecision { get; set; } = 64d;

    /// <summary>
    /// PhigrosV3 非对齐 XY 事件切割精度
    /// </summary>
    public double PhigrosMisalignedXyEventPrecision { get; set; } = 64d;

    /// <summary>
    /// PhigrosV3 Alpha 事件切割精度
    /// </summary>
    public double PhigrosAlphaCutPrecision { get; set; } = 64d;

    /// <summary>
    /// PhigrosV3 Alpha 事件切割后是否压缩
    /// </summary>
    public bool PhigrosAlphaCutCompress { get; set; } = true;

    /// <summary>
    /// PhigrosV3 Alpha 事件切割后压缩容差百分比
    /// </summary>
    public double PhigrosAlphaCutTolerance { get; set; } = 0.1d;

    /// <summary>
    /// PhigrosV3 速度事件切割精度
    /// </summary>
    public double PhigrosSpeedCutPrecision { get; set; } = 64d;

    // ---- 通用父子线解绑选项 ----

    /// <summary>
    /// 父子线解绑精度
    /// </summary>
    public double UnbindPrecision { get; set; } = 64d;

    /// <summary>
    /// 父子线解绑容差百分比
    /// </summary>
    public double UnbindTolerance { get; set; } = 0.1d;

    /// <summary>
    /// 父子线解绑是否使用经典模式
    /// </summary>
    public bool UnbindClassicMode { get; set; } = false;

    // ---- 通用多层级合并选项 ----

    /// <summary>
    /// 多层级合并精度
    /// </summary>
    public double MultiLayerMergePrecision { get; set; } = 64d;

    /// <summary>
    /// 多层级合并容差百分比
    /// </summary>
    public double MultiLayerMergeTolerance { get; set; } = 0.1d;

    /// <summary>
    /// 多层级合并是否使用经典模式
    /// </summary>
    public bool MultiLayerMergeClassicMode { get; set; } = false;
}