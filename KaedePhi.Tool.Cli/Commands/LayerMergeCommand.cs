using System.Globalization;
using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Layer.KaedePhi;

namespace KaedePhi.Tool.Cli.Commands;

public static class LayerMergeCommand
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
    private static readonly Option<bool> NoCompressOpt = SharedOptions.NoCompressOption;
    private static readonly Option<bool> DryRunOpt = SharedOptions.DryRunOption;

    public static Command Create()
    {
        var cmd = new Command("layer-merge", L("cmd_rpe_layer_merge_desc"));
        cmd.Add(InputOpt);
        cmd.Add(OutputOpt);
        cmd.Add(WorkspaceOpt);
        cmd.Add(PrecisionOpt);
        cmd.Add(ToleranceOpt);
        cmd.Add(ClassicOpt);
        cmd.Add(NoCompressOpt);
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
            var c = config.LayerMergeConfig;
            var precision = SharedOptions.GetIfSpecified(result, PrecisionOpt) ?? c.Precision;
            var tolerance = SharedOptions.GetIfSpecified(result, ToleranceOpt) ?? c.Tolerance;
            var classic = SharedOptions.GetIfSpecified(result, ClassicOpt) ?? c.ClassicMode;
            var disableCompress = SharedOptions.GetIfSpecified(result, NoCompressOpt) ?? c.DisableCompress;
            var dryRun = SharedOptions.GetIfSpecified(result, DryRunOpt) ?? c.DryRun;

            if (disableCompress && classic != true)
            {
                ConsoleWriter.Error(CliLocalizationString.err_classic_disablsed);
                return 1;
            }

            var svc = new ChartService();
            var nrc = await svc.LoadKpcAsync(input, workspace, ct);
            if (nrc == null)
            {
                ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
                return 1;
            }

            var nrcCopy = nrc.Clone();
            var processor = new LayerProcessor();
            foreach (var line in nrcCopy.JudgeLineList)
            {
                if (line.EventLayers is not { Count: > 1 })
                    continue;
                line.EventLayers =
                [
                    classic == true
                        ? processor.LayerMerge(line.EventLayers, precision)
                        : processor.LayerMergePlus(line.EventLayers, precision, tolerance),
                ];
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
