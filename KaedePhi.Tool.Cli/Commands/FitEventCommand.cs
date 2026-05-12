using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Settings;
using KaedePhi.Tool.Event.KaedePhi;

namespace KaedePhi.Tool.Cli.Commands;

public sealed class FitEventCommand : AsyncCommand<FitEventCommand.Settings>
{
    public sealed class Settings : OperationSettings
    {
        [CommandOption("--segment-penalty <N>")]
        [LocalizedDescription("fit_opt_segment_penalty")]
        public double? SegmentPenalty { get; set; }

        [CommandOption("--keep-original-penalty <N>")]
        [LocalizedDescription("fit_opt_keep_original_penalty")]
        public double? KeepOriginalPenalty { get; set; }

        [CommandOption("--full-search-threshold <N>")]
        [LocalizedDescription("fit_opt_full_search_threshold")]
        public int? FullSearchRunLengthThreshold { get; set; }

        [CommandOption("--search-window <N>")]
        [LocalizedDescription("fit_opt_search_window")]
        public int? LongRunSearchWindow { get; set; }

        [CommandOption("--phase-epsilon <N>")]
        [LocalizedDescription("fit_opt_phase_epsilon")]
        public double? PhaseDetectionEpsilon { get; set; }
    }

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
            writer.Error(CliLocalizationString.err_unimplemented);
            return 1;
        }

        var nrcCopy = nrc.Clone();

        var fitOptions = new EventFitOptions
        {
            SegmentPenalty = s.SegmentPenalty ?? c.SegmentPenalty,
            KeepOriginalPenalty = s.KeepOriginalPenalty ?? c.KeepOriginalPenalty,
            FullSearchRunLengthThreshold = s.FullSearchRunLengthThreshold ?? c.FullSearchRunLengthThreshold,
            LongRunSearchWindow = s.LongRunSearchWindow ?? c.LongRunSearchWindow,
            PhaseDetectionEpsilon = s.PhaseDetectionEpsilon ?? c.PhaseDetectionEpsilon
        };

        var mxFitter = new EventFit<double>(fitOptions);
        var myFitter = new EventFit<double>(fitOptions);
        var alFitter = new EventFit<int>(fitOptions);
        var roFitter = new EventFit<double>(fitOptions);
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
        writer.Info(string.Format(CliLocalizationString.msg_written, output));
        return 0;
    }
}