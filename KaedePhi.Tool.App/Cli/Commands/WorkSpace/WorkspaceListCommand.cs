using KaedePhi.Tool.App.Cli.Infrastructure;
using KaedePhi.Tool.App.Shared;

namespace KaedePhi.Tool.App.Cli.Commands.WorkSpace;

public static class WorkspaceListCommand
{
    public static Command Create()
    {
        var cmd = new Command("list", CliHelper.L("cmd_workspace_list_desc"));
        cmd.SetAction(_ =>
        {
            var ws = new WorkspaceService();
            foreach (var id in ws.List())
                Console.WriteLine(id);
            return 0;
        });
        return cmd;
    }
}
