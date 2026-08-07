using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Shared;

namespace KaedePhi.Tool.App.Cli.Commands;

public static class CutEventCommand
{
    private static readonly Option<string?> InputOpt = SharedOptions.CreateInputRpeOption();
    private static readonly Option<string?> OutputOpt = SharedOptions.CreateOutputAutoOption();
    private static readonly Option<string?> WorkspaceOpt = SharedOptions.CreateWorkspaceRpeOption();
    private static readonly Option<double> PrecisionOpt = SharedOptions.PrecisionOption;
    private static readonly Option<double> ToleranceOpt = SharedOptions.ToleranceOption;
    private static readonly Option<bool> NoCompressOpt = SharedOptions.NoCompressOption;
    private static readonly Option<bool> DryRunOpt = SharedOptions.DryRunOption;

    public static Command Create()
    {
        var cmd = new Command("cut", CliHelper.L("cmd_rpe_cut_event_desc"));
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
                var kpc = await svc.LoadKpcAsync(input, workspace, ct);
                if (kpc == null)
                {
                    ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
                    return 1;
                }

                var kpcClone = kpc.Clone();
                ChartProcessor.CutEvent(
                    kpcClone,
                    precision,
                    tolerance,
                    disableCompress,
                    ConsoleWriter.Info,
                    ConsoleWriter.Warn,
                    ConsoleWriter.Error,
                    ConsoleWriter.Debug
                );

                var output = await ChartService.SaveAsRpeAsync(
                    kpcClone,
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
