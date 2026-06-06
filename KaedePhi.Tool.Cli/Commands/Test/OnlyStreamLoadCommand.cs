using System.ComponentModel;
using KaedePhi.Core.PhiEdit;
using KaedePhi.Tool.Cli.Infrastructure;

namespace KaedePhi.Tool.Cli.Commands.Test;

public class OnlyStreamLoadCommand : AsyncCommand<GetTypeTestCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--input <PATH>")]
        [Description("需要推算的文件")]
        public string? Input { get; set; }
    }

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        GetTypeTestCommand.Settings settings,
        CancellationToken cancellationToken
    )
    {
#if Debug
        var input = settings.Input;
        if (string.IsNullOrWhiteSpace(input))
        {
            ConsoleWriter.Error("Input file path cannot be null or whitespace.");
            return 1;
        }

        // 创建文件流
        using var stream = File.OpenRead(input);
        // 测试pec
        var chart = await Chart.LoadStreamAsync(stream);
        ConsoleWriter.Info(chart.Offset.ToString());
        Console.ReadLine();
#else
        ConsoleWriter.Warn("This command can only be executed on Debug builds.");
        await Task.CompletedTask;
#endif

        return 0;
    }
}
