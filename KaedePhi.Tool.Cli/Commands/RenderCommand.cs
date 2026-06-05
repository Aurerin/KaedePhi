using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Settings;
using KaedePhi.Tool.Render.KaedePhi;

namespace KaedePhi.Tool.Cli.Commands;

public sealed class RenderCommand : AsyncCommand<RenderCommand.Settings>
{
    public sealed class Settings : OperationSettings
    {
        [CommandOption("-r|--pixels-per-beat <N>")]
        [LocalizedDescription("render_opt_pixels_per_beat")]
        public float? PixelsPerBeat { get; set; }

        [CommandOption("--channel-width <N>")]
        [LocalizedDescription("render_opt_channel_width")]
        public int? ChannelWidth { get; set; }

        [CommandOption("--samples <N>")]
        [LocalizedDescription("render_opt_samples")]
        public int? SamplesPerEvent { get; set; }

        [CommandOption("-b|--beat-subdivisions <N>")]
        [LocalizedDescription("render_opt_beat_subdivisions")]
        public int? BeatSubdivisions { get; set; }

        [CommandOption("--line <INDEX>")]
        [LocalizedDescription("render_opt_line")]
        public int? LineIndex { get; set; }

        [CommandOption("--layer <INDEX>")]
        [LocalizedDescription("render_opt_layer")]
        public int? LayerIndex { get; set; }

        [CommandOption("--range-padding-ratio <N>")]
        [LocalizedDescription("render_opt_range_padding_ratio")]
        public double? RangePaddingRatio { get; set; }

        [CommandOption("--range-samples <N>")]
        [LocalizedDescription("render_opt_range_samples")]
        public int? RangeSamplesPerEvent { get; set; }

        [CommandOption("--segment-tolerance <N>")]
        [LocalizedDescription("render_opt_segment_tolerance")]
        public double? SegmentGroupTolerance { get; set; }

        [CommandOption("--min-range-half <N>")]
        [LocalizedDescription("render_opt_min_range_half")]
        public double? MinValueRangeHalf { get; set; }

        [CommandOption("--min-range-half-ratio <N>")]
        [LocalizedDescription("render_opt_min_range_half_ratio")]
        public double? MinValueRangeHalfRatio { get; set; }
    }

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings s,
        CancellationToken cancellationToken
    )
    {
        var c = s.AppConfig.RenderConfig;
        s.PixelsPerBeat ??= c.PixelsPerBeat;
        s.ChannelWidth ??= c.ChannelWidth;
        s.SamplesPerEvent ??= c.SamplesPerEvent;
        s.BeatSubdivisions ??= c.BeatSubdivisions;

        var svc = new ChartService();
        var nrc = await svc.LoadKpcAsync(s.Input, s.Workspace, cancellationToken);
        if (nrc == null)
        {
            ConsoleWriter.Error(CliLocalizationString.render_err_load_failed);
            return 1;
        }

        string? outputDir;
        if (!string.IsNullOrWhiteSpace(s.Output))
            outputDir = s.Output;
        else
            outputDir = !string.IsNullOrWhiteSpace(s.Input)
                ? Path.Combine(Path.GetDirectoryName(s.Input) ?? ".", "render_output")
                : Path.Combine(Directory.GetCurrentDirectory(), "render_output");

        ConsoleWriter.Info(string.Format(CliLocalizationString.render_msg_start, outputDir));

        var opts = new KpcRenderOptions
        {
            PixelsPerBeat = s.PixelsPerBeat ?? 100f,
            ChannelWidth = s.ChannelWidth ?? 150,
            SamplesPerEvent = s.SamplesPerEvent ?? 64,
            BeatSubdivisions = s.BeatSubdivisions ?? 2,
            RangePaddingRatio = s.RangePaddingRatio ?? c.RangePaddingRatio,
            RangeSamplesPerEvent = s.RangeSamplesPerEvent ?? c.RangeSamplesPerEvent,
            SegmentGroupTolerance = s.SegmentGroupTolerance ?? c.SegmentGroupTolerance,
            MinValueRangeHalf = s.MinValueRangeHalf ?? c.MinValueRangeHalf,
            MinValueRangeHalfRatio = s.MinValueRangeHalfRatio ?? c.MinValueRangeHalfRatio,
        };

        var exporter = new KpcChartRenderExporter();
        exporter.SubscribeLog(
            info: ConsoleWriter.Info,
            warning: ConsoleWriter.Warn,
            error: ConsoleWriter.Error
        );

        try
        {
            var files = exporter.ExportChart(nrc, outputDir, opts, s.LineIndex, s.LayerIndex);
            if (files.Count == 0)
                ConsoleWriter.Warn(CliLocalizationString.render_warn_nothing);
            else
                ConsoleWriter.Info(
                    string.Format(CliLocalizationString.render_msg_done, files.Count, outputDir)
                );
        }
        catch (Exception ex)
        {
            ConsoleWriter.Error(
                string.Format(CliLocalizationString.render_err_render_failed, ex.Message)
            );
            return 2;
        }

        return 0;
    }
}
