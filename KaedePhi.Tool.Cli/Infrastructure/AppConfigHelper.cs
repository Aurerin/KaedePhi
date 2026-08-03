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

    // ReSharper disable once ChangeFieldTypeToSystemThreadingLock 不适用于.NET 8.0
    private static readonly object Lock = new();

    public static AppConfig Load()
    {
        if (_cached is not null)
            return _cached;

        lock (Lock)
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
