using System.Globalization;
using KaedePhi.Tool.Cli.Infrastructure;

namespace KaedePhi.Tool.Cli.Commands.WorkSpace;

public static class WorkspaceClearCommand
{
    private static string L(string key) =>
        CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentCulture)
        ?? key;

    private static readonly Option<string?> IdOpt = new("--id")
    {
        Description = L("cli_opt_workspace_clear_id_desc"),
        Arity = ArgumentArity.ZeroOrOne
    };

    public static Command Create()
    {
        var cmd = new Command("clear", L("cmd_workspace_clear_desc"));
        cmd.Add(IdOpt);
        cmd.SetAction((result) =>
        {
            var ws = new WorkspaceService();
            ws.Clear(result.GetValue(IdOpt));
            ConsoleWriter.Info(CliLocalizationString.msg_cleared);
            return 0;
        });
        return cmd;
    }
}
