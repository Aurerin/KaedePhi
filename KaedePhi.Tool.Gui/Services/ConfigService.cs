using System.IO;
using KaedePhi.Tool.Gui.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KaedePhi.Tool.Gui.Services;

public sealed class ConfigService
{
    private readonly string _configPath;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public GuiAppConfig Config { get; set; }

    public ConfigService()
    {
        var configDir = AppPaths.GetDirectory("config");
        _configPath = Path.Combine(configDir, "config.yaml");

        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        Config = Load();
    }

    private GuiAppConfig Load()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var yaml = File.ReadAllText(_configPath);
                return _deserializer.Deserialize<GuiAppConfig>(yaml) ?? new GuiAppConfig();
            }
        }
        catch
        {
            // Config parse error, fall back to defaults
        }

        var defaults = new GuiAppConfig();
        Save(defaults);
        return defaults;
    }

    public void Save()
    {
        Save(Config);
    }

    private void Save(GuiAppConfig config)
    {
        try
        {
            var yaml = _serializer.Serialize(config);
            File.WriteAllText(_configPath, yaml);
        }
        catch
        {
            // Save failure should not crash the app
        }
    }
}
