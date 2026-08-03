using System.Globalization;
using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.JudgeLines.KaedePhi;

namespace KaedePhi.Tool.Cli.Commands;

public static class UnbindFatherCommand
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
    private static readonly Option<bool> ClassicOpt = SharedOptions.ClassicOption;
    private static readonly Option<bool> DryRunOpt = SharedOptions.DryRunOption;

    public static Command Create()
    {
        var cmd = new Command("unbind-father", L("cmd_rpe_unbind_father_desc"));
        cmd.Aliases.Add("unbind");
        cmd.Add(InputOpt);
        cmd.Add(OutputOpt);
        cmd.Add(WorkspaceOpt);
        cmd.Add(PrecisionOpt);
        cmd.Add(ToleranceOpt);
        cmd.Add(ClassicOpt);
        cmd.Add(DryRunOpt);

        cmd.SetAction(async (result, ct) =>
        {
            var input = result.GetValue(InputOpt);
            var workspace = result.GetValue(WorkspaceOpt);
            if (string.IsNullOrWhiteSpace(input) && string.IsNullOrWhiteSpace(workspace))
            {
                ConsoleWriter.Error(CliLocalizationString.err_input_required);
                return 1;
            }

            var config = AppConfigHelper.Load();
            var c = config.UnbindConfig;
            var precision = SharedOptions.GetIfSpecified(result, PrecisionOpt) ?? c.Precision;
            var tolerance = SharedOptions.GetIfSpecified(result, ToleranceOpt) ?? c.Tolerance;
            var classic = SharedOptions.GetIfSpecified(result, ClassicOpt) ?? c.ClassicMode;
            var dryRun = SharedOptions.GetIfSpecified(result, DryRunOpt) ?? c.DryRun;

            var svc = new ChartService();
            var nrc = await svc.LoadKpcAsync(input, workspace, ct);
            if (nrc == null)
            {
                ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
                return 1;
            }

            var nrcCopy = nrc.Clone();
            var unbinder = new JudgeLineUnbinder();
            unbinder.SubscribeLog(ConsoleWriter.Info, ConsoleWriter.Warn, ConsoleWriter.Error, ConsoleWriter.Debug);

            for (var i = 0; i < nrc.JudgeLineList.Count; i++)
            {
                if (nrc.JudgeLineList[i].Father != -1)
                    nrcCopy.JudgeLineList[i] = classic == true
                        ? unbinder.FatherUnbind(i, nrc.JudgeLineList, precision)
                        : unbinder.FatherUnbind(i, nrc.JudgeLineList, precision, tolerance);
            }

            var output = await ChartService.SaveAsRpeAsync(
                nrcCopy,
                svc.ResolveOutputPath(input, result.GetValue(OutputOpt), workspace),
                dryRun,
                ct
            );
            ConsoleWriter.Info(string.Format(CliLocalizationString.msg_written, output));
            return 0;
        });

        return cmd;
    }
}
