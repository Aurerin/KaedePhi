using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KaedePhi.Tool.Gui.Services;
using KaedePhi.Tool.Gui.ViewModels;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.Gui;

public class App : Application
{
    internal static ConfigService ConfigService { get; } = new();
    internal static LogService LogService { get; } = new(ConfigService.Config.MaxLogFiles);
    internal static GuiChartService ChartService { get; } = new(LogService);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            LogService.StartSession();
            LogService.Info(log_app_starting);

            var mainVm = new MainViewModel();
            var mainWindow = new MainWindow { DataContext = mainVm };

            var controller = new AppController(mainVm, ChartService, LogService, ConfigService, mainWindow);
            controller.Initialize();

            desktop.MainWindow = mainWindow;
            desktop.Exit += (_, _) =>
            {
                LogService.Info(log_shutdown);
                ChartService.Clear();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
