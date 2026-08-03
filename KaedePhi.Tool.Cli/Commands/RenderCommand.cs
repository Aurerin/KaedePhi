using System.Globalization;
using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Render.KaedePhi;

namespace KaedePhi.Tool.Cli.Commands;

public static class RenderCommand
{
    private static string L(string key) =>
        CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? CliLocalizationString.ResourceManager.GetString(key, CultureInfo.CurrentCulture)
        ?? key;

    private static readonly Option<string?> InputOpt = SharedOptions.CreateInputRpeOption();
    private static readonly Option<string?> OutputOpt = SharedOptions.CreateOutputAutoOption();
    private static readonly Option<string?> WorkspaceOpt = SharedOptions.CreateWorkspaceRpeOption();

    private static readonly Option<float> PixelsPerBeatOpt = new("--pixels-per-beat", "-r")
    {
        Description = L("render_opt_pixels_per_beat"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<int> ChannelWidthOpt = new("--channel-width")
    {
        Description = L("render_opt_channel_width"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<int> SamplesPerEventOpt = new("--samples")
    {
        Description = L("render_opt_samples"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<int> BeatSubdivisionsOpt = new("--beat-subdivisions", "-b")
    {
        Description = L("render_opt_beat_subdivisions"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<int> LineIndexOpt = new("--line")
    {
        Description = L("render_opt_line"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<int> LayerIndexOpt = new("--layer")
    {
        Description = L("render_opt_layer"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<double> RangePaddingRatioOpt = new("--range-padding-ratio")
    {
        Description = L("render_opt_range_padding_ratio"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<int> RangeSamplesOpt = new("--range-samples")
    {
        Description = L("render_opt_range_samples"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<double> SegmentToleranceOpt = new("--segment-tolerance")
    {
        Description = L("render_opt_segment_tolerance"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<double> MinRangeHalfOpt = new("--min-range-half")
    {
        Description = L("render_opt_min_range_half"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<double> MinRangeHalfRatioOpt = new("--min-range-half-ratio")
    {
        Description = L("render_opt_min_range_half_ratio"),
        Arity = ArgumentArity.ZeroOrOne,
    };

    public static Command Create()
    {
        var cmd = new Command("render-event", L("render_command_desc"));
        cmd.Aliases.Add("render");
        cmd.Add(InputOpt);
        cmd.Add(OutputOpt);
        cmd.Add(WorkspaceOpt);
        cmd.Add(PixelsPerBeatOpt);
        cmd.Add(ChannelWidthOpt);
        cmd.Add(SamplesPerEventOpt);
        cmd.Add(BeatSubdivisionsOpt);
        cmd.Add(LineIndexOpt);
        cmd.Add(LayerIndexOpt);
        cmd.Add(RangePaddingRatioOpt);
        cmd.Add(RangeSamplesOpt);
        cmd.Add(SegmentToleranceOpt);
        cmd.Add(MinRangeHalfOpt);
        cmd.Add(MinRangeHalfRatioOpt);

        cmd.SetAction(
            async (result, ct) =>
            {
                var input = result.GetValue(InputOpt);
                var workspace = result.GetValue(WorkspaceOpt);
                if (string.IsNullOrWhiteSpace(input) && string.IsNullOrWhiteSpace(workspace))
                {
                    ConsoleWriter.Error(CliLocalizationString.err_input_required);
                    return 1;
                }

                var config = AppConfigHelper.Load();
                var c = config.RenderConfig;

                var svc = new ChartService();
                var nrc = await svc.LoadKpcAsync(input, workspace, ct);
                if (nrc == null)
                {
                    ConsoleWriter.Error(CliLocalizationString.render_err_load_failed);
                    return 1;
                }

                string? outputDir;
                var outputValue = result.GetValue(OutputOpt);
                if (!string.IsNullOrWhiteSpace(outputValue))
                    outputDir = outputValue;
                else
                    outputDir = !string.IsNullOrWhiteSpace(input)
                        ? Path.Combine(Path.GetDirectoryName(input) ?? ".", "render_output")
                        : Path.Combine(Directory.GetCurrentDirectory(), "render_output");

                ConsoleWriter.Info(
                    string.Format(CliLocalizationString.render_msg_start, outputDir)
                );

                var opts = new KpcRenderOptions
                {
                    PixelsPerBeat =
                        SharedOptions.GetIfSpecified(result, PixelsPerBeatOpt) ?? c.PixelsPerBeat,
                    ChannelWidth =
                        SharedOptions.GetIfSpecified(result, ChannelWidthOpt) ?? c.ChannelWidth,
                    SamplesPerEvent =
                        SharedOptions.GetIfSpecified(result, SamplesPerEventOpt)
                        ?? c.SamplesPerEvent,
                    BeatSubdivisions =
                        SharedOptions.GetIfSpecified(result, BeatSubdivisionsOpt)
                        ?? c.BeatSubdivisions,
                    RangePaddingRatio =
                        SharedOptions.GetIfSpecified(result, RangePaddingRatioOpt)
                        ?? c.RangePaddingRatio,
                    RangeSamplesPerEvent =
                        SharedOptions.GetIfSpecified(result, RangeSamplesOpt)
                        ?? c.RangeSamplesPerEvent,
                    SegmentGroupTolerance =
                        SharedOptions.GetIfSpecified(result, SegmentToleranceOpt)
                        ?? c.SegmentGroupTolerance,
                    MinValueRangeHalf =
                        SharedOptions.GetIfSpecified(result, MinRangeHalfOpt)
                        ?? c.MinValueRangeHalf,
                    MinValueRangeHalfRatio =
                        SharedOptions.GetIfSpecified(result, MinRangeHalfRatioOpt)
                        ?? c.MinValueRangeHalfRatio,
                };

                var exporter = new KpcChartRenderExporter();
                exporter.SubscribeLog(ConsoleWriter.Info, ConsoleWriter.Warn, ConsoleWriter.Error);

                try
                {
                    var lineIndex = SharedOptions.GetIfSpecified(result, LineIndexOpt);
                    var layerIndex = SharedOptions.GetIfSpecified(result, LayerIndexOpt);
                    var files = exporter.ExportChart(nrc, outputDir, opts, lineIndex, layerIndex);
                    if (files.Count == 0)
                        ConsoleWriter.Warn(CliLocalizationString.render_warn_nothing);
                    else
                        ConsoleWriter.Info(
                            string.Format(
                                CliLocalizationString.render_msg_done,
                                files.Count,
                                outputDir
                            )
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
        );

        return cmd;
    }
}
