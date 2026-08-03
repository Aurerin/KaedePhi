using System.Globalization;
using KaedePhi.Tool.Cli.Infrastructure;

namespace KaedePhi.Tool.Cli.Commands.WorkSpace;

public static class WorkspaceListCommand
{
    private static string L(string key) =>
        CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentCulture)
        ?? key;

    public static Command Create()
    {
        var cmd = new Command("list", L("cmd_workspace_list_desc"));
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
