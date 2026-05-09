using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Settings;
using KaedePhi.Tool.Event.KaedePhi;

namespace KaedePhi.Tool.Cli.Commands;

public sealed class FitEventCommand : AsyncCommand<FitEventCommand.Settings>
{
    public sealed class Settings : OperationSettings;

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings s, CancellationToken cancellationToken)
    {
        var c = s.AppConfig.FitConfig;
        s.Tolerance ??= c.Tolerance;
        s.DryRun ??= c.DryRun;

        var writer = new ConsoleWriter();
        var svc = new ChartService();
        var nrc = await svc.LoadKpcAsync(s.Input, s.Workspace, cancellationToken);
        if (nrc == null)
        {
            writer.Error(Strings.cli_err_unimplemented);
            return 1;
        }

        var nrcCopy = nrc.Clone();

        var mxFitter = new EventFit<double>();
        var myFitter = new EventFit<double>();
        var alFitter = new EventFit<int>();
        var roFitter = new EventFit<double>();
        mxFitter.SubscribeLog(info: writer.Info, warning: writer.Warn, error: writer.Error, debug: writer.Info);
        myFitter.SubscribeLog(info: writer.Info, warning: writer.Warn, error: writer.Error, debug: writer.Info);
        alFitter.SubscribeLog(info: writer.Info, warning: writer.Warn, error: writer.Error, debug: writer.Info);
        roFitter.SubscribeLog(info: writer.Info, warning: writer.Warn, error: writer.Error, debug: writer.Info);

        var degree = Math.Max(1, Environment.ProcessorCount);
        var tol = s.Tolerance ?? 0.5d;

        for (var i = 0; i < nrc.JudgeLineList.Count; i++)
        {
            for (var j = 0; j < nrc.JudgeLineList[i].EventLayers.Count; j++)
            {
                var el = nrc.JudgeLineList[i].EventLayers[j];
                if (el == null) continue;
                cancellationToken.ThrowIfCancellationRequested();

                // 使用顺序执行避免嵌套并行导致线程池抖动
                var mxResult = mxFitter.EventListFit(el.MoveXEvents, tol, degree);
                var myResult = myFitter.EventListFit(el.MoveYEvents, tol, degree);
                var alResult = alFitter.EventListFit(el.AlphaEvents, tol, degree);
                var roResult = roFitter.EventListFit(el.RotateEvents, tol, degree);

                nrcCopy.JudgeLineList[i].EventLayers[j].MoveXEvents = mxResult;
                nrcCopy.JudgeLineList[i].EventLayers[j].MoveYEvents = myResult;
                nrcCopy.JudgeLineList[i].EventLayers[j].AlphaEvents = alResult;
                nrcCopy.JudgeLineList[i].EventLayers[j].RotateEvents = roResult;
            }
        }

        var output = await ChartService.SaveAsRpeAsync(nrcCopy, svc.ResolveOutputPath(s.Input, s.Output, s.Workspace),
            s.DryRun ?? false, cancellationToken);
        writer.Info(string.Format(Strings.cli_msg_written, output));
        return 0;
    }
}