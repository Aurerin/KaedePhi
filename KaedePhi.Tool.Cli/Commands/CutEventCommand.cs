using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Settings;
using KaedePhi.Tool.Event.KaedePhi;
using KaedePhi.Tool.Layer.KaedePhi;
using EventLayer = KaedePhi.Core.KaedePhi.EventLayer;

namespace KaedePhi.Tool.Cli.Commands;

public sealed class CutEventCommand : AsyncCommand<CutEventCommand.Settings>
{
    public sealed class Settings : OperationSettings;

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings s,
        CancellationToken cancellationToken)
    {
        var c = s.AppConfig.CutConfig;
        s.Precision ??= c.Precision;
        s.Tolerance ??= c.Tolerance;
        s.DisableCompress ??= c.DisableCompress;
        s.DryRun ??= c.DryRun;

        var svc = new ChartService();
        var nrc = await svc.LoadKpcAsync(s.Input, s.Workspace, cancellationToken);
        if (nrc == null)
        {
            ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
            return 1;
        }

        var nrcCopy = nrc.Clone();
        var layerProcessor = new LayerProcessor();
        var doubleCompressor = new EventCompressor<double>();
        var intCompressor = new EventCompressor<int>();

        foreach (var line in nrcCopy.JudgeLineList)
        {
            line.EventLayers = layerProcessor.CutLayerEvents(line.EventLayers, s.Precision ?? 64d);
            if (s.DisableCompress ?? false) continue;
            foreach (var el in line.EventLayers.OfType<EventLayer>())
            {
                el.MoveXEvents = doubleCompressor.EventListCompressSqrt(el.MoveXEvents ?? [], s.Tolerance ?? 5d);
                el.MoveYEvents = doubleCompressor.EventListCompressSqrt(el.MoveYEvents ?? [], s.Tolerance ?? 5d);
                el.RotateEvents = doubleCompressor.EventListCompressSlope(el.RotateEvents ?? [], s.Tolerance ?? 5d);
                el.AlphaEvents = intCompressor.EventListCompressSlope(el.AlphaEvents ?? [], s.Tolerance ?? 5d);
            }
        }

        var output = await ChartService.SaveAsRpeAsync(nrcCopy, svc.ResolveOutputPath(s.Input, s.Output, s.Workspace),
            s.DryRun ?? false, cancellationToken);
        ConsoleWriter.Info(string.Format(CliLocalizationString.msg_written, output));
        return 0;
    }
}