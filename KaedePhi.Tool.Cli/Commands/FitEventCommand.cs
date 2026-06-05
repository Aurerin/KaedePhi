using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Settings;
using KaedePhi.Tool.Event.KaedePhi;

namespace KaedePhi.Tool.Cli.Commands;

public sealed class FitEventCommand : AsyncCommand<FitEventCommand.Settings>
{
    public sealed class Settings : OperationSettings { }

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings s,
        CancellationToken cancellationToken
    )
    {
        var c = s.AppConfig.FitConfig;
        s.Tolerance ??= c.Tolerance;
        s.DryRun ??= c.DryRun;

        var svc = new ChartService();
        var nrc = await svc.LoadKpcAsync(s.Input, s.Workspace, cancellationToken);
        if (nrc == null)
        {
            ConsoleWriter.Error(CliLocalizationString.err_unimplemented);
            return 1;
        }

        var nrcCopy = nrc.Clone();

        var mxFitter = new EventFit<double>();
        var myFitter = new EventFit<double>();
        var alFitter = new EventFit<int>();
        var roFitter = new EventFit<double>();
        var spFitter = new EventFit<float>();
        mxFitter.SubscribeLog(
            info: ConsoleWriter.Info,
            warning: ConsoleWriter.Warn,
            error: ConsoleWriter.Error,
            debug: ConsoleWriter.Debug
        );
        myFitter.SubscribeLog(
            info: ConsoleWriter.Info,
            warning: ConsoleWriter.Warn,
            error: ConsoleWriter.Error,
            debug: ConsoleWriter.Debug
        );
        alFitter.SubscribeLog(
            info: ConsoleWriter.Info,
            warning: ConsoleWriter.Warn,
            error: ConsoleWriter.Error,
            debug: ConsoleWriter.Debug
        );
        roFitter.SubscribeLog(
            info: ConsoleWriter.Info,
            warning: ConsoleWriter.Warn,
            error: ConsoleWriter.Error,
            debug: ConsoleWriter.Debug
        );
        spFitter.SubscribeLog(
            info: ConsoleWriter.Info,
            warning: ConsoleWriter.Warn,
            error: ConsoleWriter.Error,
            debug: ConsoleWriter.Debug
        );

        var tolerance = s.Tolerance ?? 0.1d;

        for (var i = 0; i < nrc.JudgeLineList.Count; i++)
        {
            for (var j = 0; j < nrc.JudgeLineList[i].EventLayers.Count; j++)
            {
                var el = nrc.JudgeLineList[i].EventLayers[j];
                if (el == null)
                    continue;
                cancellationToken.ThrowIfCancellationRequested();

                var mxResult = mxFitter.FitEvents(el.MoveXEvents, tolerance);
                var myResult = myFitter.FitEvents(el.MoveYEvents, tolerance);
                var alResult = alFitter.FitEvents(el.AlphaEvents, tolerance);
                var roResult = roFitter.FitEvents(el.RotateEvents, tolerance);
                var spResult = spFitter.FitEvents(el.SpeedEvents, tolerance);

                nrcCopy.JudgeLineList[i].EventLayers[j].MoveXEvents = mxResult;
                nrcCopy.JudgeLineList[i].EventLayers[j].MoveYEvents = myResult;
                nrcCopy.JudgeLineList[i].EventLayers[j].AlphaEvents = alResult;
                nrcCopy.JudgeLineList[i].EventLayers[j].RotateEvents = roResult;
                nrcCopy.JudgeLineList[i].EventLayers[j].SpeedEvents = spResult;
            }
        }

        var output = await ChartService.SaveAsRpeAsync(
            nrcCopy,
            svc.ResolveOutputPath(s.Input, s.Output, s.Workspace),
            s.DryRun ?? false,
            cancellationToken
        );
        ConsoleWriter.Info(string.Format(CliLocalizationString.msg_written, output));
        return 0;
    }
}
