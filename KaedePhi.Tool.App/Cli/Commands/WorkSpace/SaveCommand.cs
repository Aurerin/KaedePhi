using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Shared;

namespace KaedePhi.Tool.App.Cli.Commands.WorkSpace;

public static class SaveCommand
{
    private static readonly Option<string?> OutputOpt = SharedOptions.CreateOutputPathOption();
    private static readonly Option<string> WorkspaceOpt = new("--workspace", "-w")
    {
        Description = CliHelper.L("cli_opt_workspace_default_desc"),
        Arity = ArgumentArity.ExactlyOne,
    };

    public static Command Create()
    {
        var cmd = new Command("save", CliHelper.L("cmd_save_desc"));
        cmd.Add(OutputOpt);
        cmd.Add(WorkspaceOpt);

        cmd.SetAction(
            async (result, ct) =>
            {
                var output = result.GetValue(OutputOpt);
                if (string.IsNullOrWhiteSpace(output))
                {
                    ConsoleWriter.Error(CliLocalizationString.err_output_required);
                    return 1;
                }

                var workspaceId = result.GetValue(WorkspaceOpt);
                if (string.IsNullOrWhiteSpace(workspaceId))
                    workspaceId = "default";

                var ws = new WorkspaceService();
                await ws.SaveAsync(workspaceId, output);
                ConsoleWriter.Info(
                    string.Format(CliLocalizationString.msg_saved, workspaceId, output)
                );
                return 0;
            }
        );

        return cmd;
    }
}
