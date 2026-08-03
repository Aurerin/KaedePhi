using System.Globalization;
using System.Reflection;
using KaedePhi.Tool.Cli.Commands;
using KaedePhi.Tool.Cli.Commands.Test;
using KaedePhi.Tool.Cli.Commands.WorkSpace;
using KaedePhi.Tool.Cli.Infrastructure;

#if !Release
ConsoleWriter.Warn(
    string.Format(CliLocalizationString.warn_unstable_version, CliLocalizationString.project_link)
);
#endif

var root = new RootCommand(CliLocalizationString.app_title);

var ver = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
root.SetAction((_) =>
{
    ConsoleWriter.Info($"{CliLocalizationString.app_title} v{ver}");
    return 0;
});

root.Add(VersionCommand.Create());

var testCmd = GetTypeTestCommand.Create();
testCmd.Hidden = true;
root.Add(testCmd);

var peStreamCmd = OnlyStreamLoadCommand.Create();
peStreamCmd.Hidden = true;
root.Add(peStreamCmd);

root.Add(LoadCommand.Create());
root.Add(SaveCommand.Create());
root.Add(ConvertCommand.Create());
root.Add(FitEventCommand.Create());
root.Add(CutEventCommand.Create());
root.Add(LayerMergeCommand.Create());
root.Add(UnbindFatherCommand.Create());
root.Add(RenderCommand.Create());

var configBranch = new Command("config", L("branch_config_desc"));
configBranch.Add(ConfigResetCommand.Create());
root.Add(configBranch);

var workspaceBranch = new Command("workspace", L("branch_workspace_desc"));
workspaceBranch.Add(WorkspaceListCommand.Create());
workspaceBranch.Add(WorkspaceClearCommand.Create());
workspaceBranch.Add(LoadCommand.Create());
workspaceBranch.Add(SaveCommand.Create());
root.Add(workspaceBranch);

try
{
    return await root.Parse(args).InvokeAsync();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    if (ex is OutOfMemoryException)
    {
        ConsoleWriter.Error(string.Format(CliLocalizationString.err_out_of_memory, ex));
        return 1;
    }
    ConsoleWriter.Error(string.Format(CliLocalizationString.err_ukerr, ex));
    return 1;
}

static string L(string key) =>
    CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
    ?? CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentCulture)
    ?? key;
