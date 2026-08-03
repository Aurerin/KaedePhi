using KaedePhi.Tool.Cli.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KaedePhi.Tool.Cli.Infrastructure;

public static class AppConfigHelper
{
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static AppConfig? _cached;
    private static readonly object _lock = new();

    public static AppConfig Load()
    {
        if (_cached is not null)
            return _cached;

        lock (_lock)
        {
            if (_cached is not null)
                return _cached;

            if (File.Exists("config.yaml"))
            {
                var text = File.ReadAllText("config.yaml");
                _cached = YamlDeserializer.Deserialize<AppConfig>(text);
            }
            else
            {
                _cached = new AppConfig();
                var yaml = YamlSerializer.Serialize(_cached);
                File.WriteAllText("config.yaml", yaml);
            }

            ConsoleWriter.LogLevel = _cached.LogLevel;
            return _cached;
        }
    }
}
