using KaedePhi.Tool.Cli.Infrastructure;
using Spectre.Console;

namespace KaedePhi.Tool.Cli.Commands.WorkSpace;

// Description set via WithDescription(CliLocalizationString.cmd_load_desc) in Program.cs
public sealed class LoadCommand : AsyncCommand<LoadCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-i|--input <PATH>")]
        [LocalizedDescription("cli_opt_input_phiedit_desc")]
        public string? Input { get; set; }

        [CommandOption("-w|--workspace <ID>")]
        [LocalizedDescription("cli_opt_workspace_default_desc")]
        public string Workspace { get; set; } = "default";

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Input))
                return ValidationResult.Error(CliLocalizationString.err_input_required);
            return base.Validate();
        }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        var ws = new WorkspaceService();
        if (settings.Input is not { } input)
            return 1;
        await ws.LoadAsync(settings.Workspace, input);
        ConsoleWriter.Info(string.Format(CliLocalizationString.msg_loaded, settings.Workspace));
        return 0;
    }
}