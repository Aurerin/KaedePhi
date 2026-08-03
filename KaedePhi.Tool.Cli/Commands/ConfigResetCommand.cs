using System.Globalization;
using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KaedePhi.Tool.Cli.Commands;

public static class ConfigResetCommand
{
    private static string L(string key) =>
        CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentCulture)
        ?? key;

    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public static Command Create()
    {
        var cmd = new Command("reset", L("cmd_config_reset_desc"));
        cmd.SetAction((_) =>
        {
            var configPath = "config.yaml";
            var defaults = new AppConfig();
            var yaml = YamlSerializer.Serialize(defaults);
            File.WriteAllText(configPath, yaml);
            ConsoleWriter.Info(string.Format(CliLocalizationString.msg_config_reset_done, configPath));
            return 0;
        });
        return cmd;
    }
}
