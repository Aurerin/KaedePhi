using System.Reflection;
using KaedePhi.Tool.Cli.Commands;
using KaedePhi.Tool.Cli.Commands.Test;
using KaedePhi.Tool.Cli.Commands.WorkSpace;
using KaedePhi.Tool.Cli.Infrastructure;

#if !Release
ConsoleWriter.Warn(string.Format(CliLocalizationString.warn_unstable_version,CliLocalizationString.project_link));
#endif

var app = new CommandApp();
app.SetDefaultCommand<VersionCommand>();
app.Configure(config =>
{
    config.SetApplicationName(CliLocalizationString.app_title);
    config.SetApplicationVersion(
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown");

    // 未知命令/参数时立即报错，而非静默跳过
    config.UseStrictParsing();

    // 统一异常处理，保持与原先相同的错误输出风格
    // TODO: 放宽命令检查，或提供更多辅助修正命令的提示
    config.SetExceptionHandler((ex, _) =>
    {
        // 未知命令/参数：引导用户使用 --help
        if (ex is CommandParseException)
        {
            ConsoleWriter.Error(CliLocalizationString.err_unknown);
            ConsoleWriter.Info(CliLocalizationString.hit_help);
            return 1;
        }

        // 如果是out of memory这种错误，应该提示使用--stream选项，而不是让其反馈
        if (ex is OutOfMemoryException)
        {
            ConsoleWriter.Error(string.Format(CliLocalizationString.err_out_of_memory, ex));
            return 1;
        }

        ConsoleWriter.Error(string.Format(CliLocalizationString.err_ukerr, ex));
        return 1;
    });

    config.AddCommand<VersionCommand>("version")
        .WithDescription(CliLocalizationString.cmd_version_desc)
        .WithAlias("ver");
    config.AddCommand<GetTypeTestCommand>("test")
        .IsHidden();
    config.AddCommand<OnlyStreamLoadCommand>("pestream")
        .IsHidden();

    config.AddCommand<LoadCommand>("load")
        .WithDescription(CliLocalizationString.cmd_load_desc);

    config.AddCommand<SaveCommand>("save")
        .WithDescription(CliLocalizationString.cmd_save_desc);
    config.AddCommand<UnbindFatherCommand>("unbind-father")
        .WithAlias("unbind")
        .WithDescription(CliLocalizationString.cmd_rpe_unbind_father_desc);
    config.AddCommand<FitEventCommand>("fit")
        .WithAlias("fit-event")
        .WithDescription(CliLocalizationString.fit_command_desc);
    config.AddCommand<ConvertCommand>("convert")
        .WithDescription(CliLocalizationString.convert_command_desc);
    config.AddCommand<LayerMergeCommand>("layer-merge")
        .WithDescription(CliLocalizationString.cmd_rpe_layer_merge_desc);
    config.AddCommand<CutEventCommand>("cut")
        .WithAlias("cut-event")
        .WithAlias("cut-all")
        .WithDescription(CliLocalizationString.cmd_rpe_cut_event_desc);
    config.AddCommand<RenderCommand>("render-event")
        .WithAlias("render")
        .WithDescription(CliLocalizationString.render_command_desc);

    config.AddBranch("workspace", ws =>
    {
        ws.SetDescription(CliLocalizationString.branch_workspace_desc);
        ws.AddCommand<WorkspaceListCommand>("list")
            .WithDescription(CliLocalizationString.cmd_workspace_list_desc);
        ws.AddCommand<WorkspaceClearCommand>("clear")
            .WithDescription(CliLocalizationString.cmd_workspace_clear_desc);
        ws.AddCommand<LoadCommand>("load")
            .WithDescription(CliLocalizationString.cmd_load_desc);
        ws.AddCommand<SaveCommand>("save")
            .WithDescription(CliLocalizationString.cmd_save_desc);
    });
});

return await app.RunAsync(args);