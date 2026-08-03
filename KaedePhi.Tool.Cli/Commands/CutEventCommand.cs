using System.Globalization;
using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Event.KaedePhi;
using KaedePhi.Tool.Layer.KaedePhi;
using EventLayer = KaedePhi.Core.KaedePhi.Events.EventLayer;

namespace KaedePhi.Tool.Cli.Commands;

public static class CutEventCommand
{
    private static string L(string key) =>
        CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentCulture)
        ?? key;

    private static readonly Option<string?> InputOpt = SharedOptions.CreateInputRpeOption();
    private static readonly Option<string?> OutputOpt = SharedOptions.CreateOutputAutoOption();
    private static readonly Option<string?> WorkspaceOpt = SharedOptions.CreateWorkspaceRpeOption();
    private static readonly Option<double> PrecisionOpt = SharedOptions.PrecisionOption;
    private static readonly Option<double> ToleranceOpt = SharedOptions.ToleranceOption;
    private static readonly Option<bool> NoCompressOpt = SharedOptions.NoCompressOption;
    private static readonly Option<bool> DryRunOpt = SharedOptions.DryRunOption;

    public static Command Create()
    {
        var cmd = new Command("cut", L("cmd_rpe_cut_event_desc"));
        cmd.Aliases.Add("cut-event");
        cmd.Aliases.Add("cut-all");
        cmd.Add(InputOpt);
        cmd.Add(OutputOpt);
        cmd.Add(WorkspaceOpt);
        cmd.Add(PrecisionOpt);
        cmd.Add(ToleranceOpt);
        cmd.Add(NoCompressOpt);
        cmd.Add(DryRunOpt);

        cmd.SetAction(
            async (result, ct) =>
            {
                var input = result.GetValue(InputOpt);
                var workspace = result.GetValue(WorkspaceOpt);
                if (string.IsNullOrWhiteSpace(input) && string.IsNullOrWhiteSpace(workspace))
                {
                    ConsoleWriter.Error(CliLocalizationString.err_input_required);
                    return 1;
                }

                var config = AppConfigHelper.Load();
                var c = config.CutConfig;
                var precision = SharedOptions.GetIfSpecified(result, PrecisionOpt) ?? c.Precision;
                var tolerance = SharedOptions.GetIfSpecified(result, ToleranceOpt) ?? c.Tolerance;
                var disableCompress =
                    SharedOptions.GetIfSpecified(result, NoCompressOpt) ?? c.DisableCompress;
                var dryRun = SharedOptions.GetIfSpecified(result, DryRunOpt) ?? c.DryRun;

                var svc = new ChartService();
                var nrc = await svc.LoadKpcAsync(input, workspace, ct);
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
                    line.EventLayers = layerProcessor.CutLayerEvents(line.EventLayers, precision);
                    if (disableCompress)
                        continue;
                    foreach (var el in line.EventLayers.OfType<EventLayer>())
                    {
                        el.MoveXEvents = doubleCompressor.EventListCompressSqrt(
                            el.MoveXEvents ?? [],
                            tolerance
                        );
                        el.MoveYEvents = doubleCompressor.EventListCompressSqrt(
                            el.MoveYEvents ?? [],
                            tolerance
                        );
                        el.RotateEvents = doubleCompressor.EventListCompressSlope(
                            el.RotateEvents ?? [],
                            tolerance
                        );
                        el.AlphaEvents = intCompressor.EventListCompressSlope(
                            el.AlphaEvents ?? [],
                            tolerance
                        );
                    }
                }

                var output = await ChartService.SaveAsRpeAsync(
                    nrcCopy,
                    svc.ResolveOutputPath(input, result.GetValue(OutputOpt), workspace),
                    dryRun,
                    ct
                );
                ConsoleWriter.Info(string.Format(CliLocalizationString.msg_written, output));
                return 0;
            }
        );

        return cmd;
    }
}
