using System.Globalization;

namespace KaedePhi.Tool.App.Shared;

public static class CliHelper
{
    public static string L(string key) =>
        CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentCulture)
        ?? key;
}
