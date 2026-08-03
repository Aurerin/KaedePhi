using System.Globalization;
using System.Reflection;
using KaedePhi.Tool.Cli.Infrastructure;

namespace KaedePhi.Tool.Cli.Commands;

public static class VersionCommand
{
    private static string L(string key) =>
        CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentCulture)
        ?? key;

    public static Command Create()
    {
        var cmd = new Command("version", L("cmd_version_desc"));
        cmd.Aliases.Add("ver");
        cmd.SetAction((_) =>
        {
#if PreRelease || Release
            var ver = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
#else
            var ver = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
#endif
            ConsoleWriter.Info($"{CliLocalizationString.app_title} v{ver}");
            return 0;
        });
        return cmd;
    }
}
