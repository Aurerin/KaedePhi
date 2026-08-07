using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Shared;

namespace KaedePhi.Tool.App.Cli.Commands.WorkSpace;

public static class WorkspaceClearCommand
{
    private static readonly Option<string?> IdOpt = new("--id")
    {
        Description = CliHelper.L("cli_opt_workspace_clear_id_desc"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    public static Command Create()
    {
        var cmd = new Command("clear", CliHelper.L("cmd_workspace_clear_desc")) { IdOpt };
        cmd.SetAction(result =>
        {
            var ws = new WorkspaceService();
            ws.Clear(result.GetValue(IdOpt));
            ConsoleWriter.Info(CliLocalizationString.msg_cleared);
            return 0;
        });
        return cmd;
    }
}
