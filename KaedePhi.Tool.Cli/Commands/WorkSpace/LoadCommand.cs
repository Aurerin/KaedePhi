using System.Globalization;
using KaedePhi.Tool.Cli.Infrastructure;

namespace KaedePhi.Tool.Cli.Commands.WorkSpace;

public static class LoadCommand
{
    private static string L(string key) =>
        CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentCulture)
        ?? key;

    private static readonly Option<string?> InputOpt = SharedOptions.CreateInputPhieditOption();
    private static readonly Option<string> WorkspaceOpt = new("--workspace", "-w")
    {
        Description = L("cli_opt_workspace_default_desc"),
        Arity = ArgumentArity.ExactlyOne
    };

    public static Command Create()
    {
        var cmd = new Command("load", L("cmd_load_desc"));
        cmd.Add(InputOpt);
        cmd.Add(WorkspaceOpt);

        cmd.SetAction(async (result, ct) =>
        {
            var input = result.GetValue(InputOpt);
            if (string.IsNullOrWhiteSpace(input))
            {
                ConsoleWriter.Error(CliLocalizationString.err_input_required);
                return 1;
            }

            var workspaceId = result.GetValue(WorkspaceOpt);
            if (string.IsNullOrWhiteSpace(workspaceId))
                workspaceId = "default";

            var ws = new WorkspaceService();
            await ws.LoadAsync(workspaceId, input);
            ConsoleWriter.Info(string.Format(CliLocalizationString.msg_loaded, workspaceId));
            return 0;
        });

        return cmd;
    }
}
