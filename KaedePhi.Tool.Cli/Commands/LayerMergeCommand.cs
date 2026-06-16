using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Settings;
using KaedePhi.Tool.Layer.KaedePhi;

namespace KaedePhi.Tool.Cli.Commands;

public sealed class LayerMergeCommand : AsyncCommand<LayerMergeCommand.Settings>
{
    public sealed class Settings : OperationSettings;

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings s,
        CancellationToken cancellationToken
    )
    {
        var c = s.AppConfig.LayerMergeConfig;
        s.Precision ??= c.Precision;
        s.Tolerance ??= c.Tolerance;
        s.Classic ??= c.ClassicMode;
        s.DisableCompress ??= c.DisableCompress;
        s.DryRun ??= c.DryRun;

        if (s.DisableCompress == true && s.Classic != true)
        {
            ConsoleWriter.Error(CliLocalizationString.err_classic_disablsed);
            return 1;
        }

        var svc = new ChartService();
        var nrc = await svc.LoadKpcAsync(s.Input, s.Workspace, cancellationToken);
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
                s.Classic == true
                    ? processor.LayerMerge(line.EventLayers, s.Precision ?? 64d)
                    : processor.LayerMergePlus(
                        line.EventLayers,
                        s.Precision ?? 64d,
                        s.Tolerance ?? 5d
                    ),
            ];
        }

        var output = await ChartService.SaveAsRpeAsync(
            nrcCopy,
            svc.ResolveOutputPath(s.Input, s.Output, s.Workspace),
            s.DryRun ?? false,
            cancellationToken
        );
        ConsoleWriter.Info(string.Format(CliLocalizationString.msg_written, output));
        return 0;
    }
}
