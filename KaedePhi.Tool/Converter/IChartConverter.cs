using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter;

public interface IChartConverter<TPayload, in TInOptions, in TOutOptions> : ILoggable
{
    Kpc.Chart ToKpc(TPayload input, TInOptions options);
    TPayload FromKpc(Kpc.Chart input, TOutOptions options);
}