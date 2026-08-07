namespace KaedePhi.Tool.Converter;

/// <summary>
/// 支持取消的谱面转换器。转换器若实现此接口，调用方可在转换前注入取消令牌。
/// </summary>
public interface ICancellableChartConverter
{
    /// <summary>
    /// 设置转换过程中使用的取消令牌。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    void SetCancellationToken(CancellationToken ct);
}
