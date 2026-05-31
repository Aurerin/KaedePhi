using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KaedePhi.Tool.Gui.Services;
using KaedePhi.Tool.Gui.ViewModels;
using Serilog;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.Gui;

public class App : Application
{
    internal static ConfigService ConfigService { get; } = new();
    internal static LogService LogService { get; } = new(ConfigService.Config.MaxLogFiles);

    // 延迟初始化：必须在 StartSession() 之后构造，否则 ForContext<T>() 返回空 logger，
    // 导致工具运行期间产生的日志全部静默丢失。
    internal static GuiChartService ChartService { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 1. 先启动日志会话，配置好 _rootLogger
            LogService.StartSession();

            // 2. 再构造依赖 ForContext<T>() 的服务，确保拿到真实 logger
            ChartService = new GuiChartService(LogService);

            Log.ForContext<App>().Information(log_app_starting);

            var mainVm = new MainViewModel();
            var mainWindow = new MainWindow { DataContext = mainVm };

            var controller = new AppController(mainVm, ChartService, LogService, ConfigService, mainWindow);
            controller.Initialize();

            desktop.MainWindow = mainWindow;
            desktop.Exit += (_, _) =>
            {
                Log.ForContext<App>().Information(log_shutdown);
                ChartService.Clear();
                LogService.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
