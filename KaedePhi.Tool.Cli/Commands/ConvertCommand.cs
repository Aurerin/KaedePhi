using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Settings;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Cli.Commands;

public sealed class ConvertCommand : AsyncCommand<ConvertCommand.Settings>
{
    public sealed class Settings : OperationSettings
    {
        [CommandOption("--target <TYPE>")]
        [LocalizedDescription("convert_command_opt_target")]
        public ChartType? TargetType { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings s, CancellationToken cancellationToken)
    {
        var c = s.AppConfig.ConvertConfig;
        s.TargetType ??= c.TargetType;
        s.StreamOutput ??= c.StreamOutput;
        s.FormatOutput ??= c.FormatOutput;
        s.DryRun ??= c.DryRun;

        var writer = new ConsoleWriter();
        var svc = new ChartService();

        var kpc = await svc.LoadKpcAsync(s.Input, s.Workspace, cancellationToken);
        if (kpc == null) { writer.Error(CliLocalizationString.err_unimplemented); return 1; }


        var output = svc.ResolveOutputPath(s.Input, s.Output, s.Workspace);
        var result = await ChartService.SaveAsAsync(kpc, output, s.TargetType ?? ChartType.RePhiEdit,
            s.StreamOutput ?? false, s.FormatOutput ?? false, s.DryRun ?? false, cancellationToken);

        if (result == null) { writer.Warn(CliLocalizationString.warn_rpe_convert); return 2; }
        writer.Info(string.Format(CliLocalizationString.msg_written, result));
        return 0;
    }
}
