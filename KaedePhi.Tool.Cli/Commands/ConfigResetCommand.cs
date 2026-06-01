using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KaedePhi.Tool.Cli.Commands;

public sealed class ConfigResetCommand : Command
{
    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var configPath = "config.yaml";
        var defaults = new AppConfig();
        var yaml = YamlSerializer.Serialize(defaults);
        File.WriteAllText(configPath, yaml);
        ConsoleWriter.Info(string.Format(CliLocalizationString.msg_config_reset_done, configPath));
        return 0;
    }
}
