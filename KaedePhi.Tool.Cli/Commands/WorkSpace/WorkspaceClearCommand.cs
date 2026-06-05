using KaedePhi.Tool.Cli.Infrastructure;

namespace KaedePhi.Tool.Cli.Commands.WorkSpace;

public sealed class WorkspaceClearCommand : Command<WorkspaceClearCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--id <ID>")]
        [LocalizedDescription("cli_opt_workspace_clear_id_desc")]
        public string? Id { get; set; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var ws = new WorkspaceService();
        ws.Clear(settings.Id);
        ConsoleWriter.Info(CliLocalizationString.msg_cleared);
        return 0;
    }
}