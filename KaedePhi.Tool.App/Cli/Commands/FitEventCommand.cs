using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Shared;

namespace KaedePhi.Tool.App.Cli.Commands;

public static class FitEventCommand
{
    private static readonly Option<string?> InputOpt = SharedOptions.CreateInputRpeOption();
    private static readonly Option<string?> OutputOpt = SharedOptions.CreateOutputAutoOption();
    private static readonly Option<string?> WorkspaceOpt = SharedOptions.CreateWorkspaceRpeOption();
    private static readonly Option<double> ToleranceOpt = SharedOptions.ToleranceOption;
    private static readonly Option<bool> DryRunOpt = SharedOptions.DryRunOption;

    public static Command Create()
    {
        var cmd = new Command("fit", CliHelper.L("fit_command_desc"));
        cmd.Aliases.Add("fit-event");
        cmd.Add(InputOpt);
        cmd.Add(OutputOpt);
        cmd.Add(WorkspaceOpt);
        cmd.Add(ToleranceOpt);
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
                var c = config.FitConfig;
                var tolerance = SharedOptions.GetIfSpecified(result, ToleranceOpt) ?? c.Tolerance;
                var dryRun = SharedOptions.GetIfSpecified(result, DryRunOpt) ?? c.DryRun;

                var svc = new ChartService();
                var kpc = await svc.LoadKpcAsync(input, workspace, ct);
                if (kpc == null)
                {
                    ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
                    return 1;
                }

                var kpcClone = kpc.Clone();
                ChartProcessor.FitEvent(
                    kpcClone,
                    tolerance,
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
