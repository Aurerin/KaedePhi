using System.Reflection;
using Avalonia.Controls;
using Avalonia.Media;

namespace KaedePhi.Tool.Gui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if !Release
        var ver = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";
        VersionLabel.Text = $"v{ver}";
        VersionLabel.Foreground = new SolidColorBrush(Colors.Yellow);
        VersionLabel.Opacity = 0.85;
#else
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionLabel.Text = version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v?";
#endif
    }
}
