using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using KaedePhi.Tool.Converter.KaedePhi;
using KaedePhi.Tool.Converter.PhiEdit;
using KaedePhi.Tool.Converter.PhiEdit.Model;
using KaedePhi.Tool.Converter.Phigros.v3;
using KaedePhi.Tool.Converter.RePhiEdit;
using KaedePhi.Tool.Converter.RePhiEdit.Model;
using KaedePhi.Tool.Event.KaedePhi;
using KaedePhi.Tool.JudgeLines.KaedePhi;
using KaedePhi.Tool.Layer.KaedePhi;
using KaedePhi.Tool.Render.KaedePhi;
using Chart = KaedePhi.Core.KaedePhi.Chart;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.Gui.Services;

public sealed class GuiChartService
{
    private readonly LogService _log;
    private const string WorkspaceId = "gui_session";
    private const string WorkspaceFileName = "chart.json";
    private readonly string _workspaceRoot;
    private readonly string _workspaceDir;
    private readonly string _workspaceFilePath;

    public GuiChartService(LogService log)
    {
        _log = log;
        _workspaceRoot = Path.Combine(AppContext.BaseDirectory, "workspaces");
        _workspaceDir = Path.Combine(_workspaceRoot, WorkspaceId);
        _workspaceFilePath = Path.Combine(_workspaceDir, WorkspaceFileName);
        Directory.CreateDirectory(_workspaceDir);
    }

    public string WorkspaceDir => _workspaceDir;
    public string WorkspaceFilePath => _workspaceFilePath;

    public void ClearWorkspace()
    {
        if (Directory.Exists(_workspaceDir))
        {
            Directory.Delete(_workspaceDir, true);
            Directory.CreateDirectory(_workspaceDir);
        }
        _log.Info(log_workspace_cleared);
    }

    public async Task CopyToWorkspaceAsync(string sourceFilePath, bool stream, CancellationToken ct)
    {
        _log.Info(string.Format(log_file_selected, sourceFilePath, stream));
        if (stream)
        {
            await using var src = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read,
                FileShare.Read, 65536, true);
            await using var dst = new FileStream(_workspaceFilePath, FileMode.Create, FileAccess.Write,
                FileShare.None, 65536, true);
            await src.CopyToAsync(dst, ct);
        }
        else
        {
            File.Copy(sourceFilePath, _workspaceFilePath, true);
        }
        _log.Info(log_step_saved);
    }

    public async Task<(string Text, ChartType DetectedType)> LoadAndDetectFromWorkspaceAsync(CancellationToken ct)
    {
        _log.Info(log_step_loading);
        var text = await File.ReadAllTextAsync(_workspaceFilePath, ct);
        var detectedType = ChartGetType.GetType(text);
        _log.Info(string.Format(log_step_detected, detectedType));
        return (text, detectedType);
    }

    public Chart ConvertToKpc(string text, ChartType sourceType)
    {
        _log.Info(log_step_converting);
        switch (sourceType)
        {
            case ChartType.RePhiEdit:
            {
                var rpeChart = Core.RePhiEdit.Chart.LoadFromJsonAsync(text).GetAwaiter().GetResult();
                return ChartPipeline
                    .From(rpeChart, new RePhiEditConverter(), null)
                    .To(new KaedePhiConverter(), null);
            }
            case ChartType.PhiEdit:
            {
                var peChart = Core.PhiEdit.Chart.LoadAsync(text).GetAwaiter().GetResult();
                var peConverter = new PhiEditConverter();
                return ChartPipeline
                    .From(peChart, peConverter, null)
                    .To(new KaedePhiConverter(), null);
            }
            case ChartType.PhigrosV3:
            {
                var v3Chart = Core.Phigros.v3.Chart.LoadFromJsonAsync(text).GetAwaiter().GetResult();
                return ChartPipeline
                    .From(v3Chart, new PhigrosV3Converter(), null)
                    .To(new KaedePhiConverter(), null);
            }
            default:
                throw new NotSupportedException($"Unsupported chart format: {sourceType}");
        }
    }

    public async Task SaveKpcToWorkspaceAsync(Chart chart, ChartType originalType, bool stream, CancellationToken ct)
    {
        await ConvertFromKpcAndSaveAsync(chart, originalType, _workspaceFilePath, stream, ct);
        _log.Info(log_step_saved);
    }

    public async Task<string> ConvertFromKpcAndSaveAsync(
        Chart chart, ChartType targetType, string outputPath, bool stream, CancellationToken ct)
    {
        _log.Info(string.Format(log_exporting_to, outputPath, targetType));
        switch (targetType)
        {
            case ChartType.RePhiEdit:
            {
                var rpeChart = new RePhiEditConverter().FromKpc(chart, new ConvertOption());
                if (stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await rpeChart.ExportToJsonStreamAsync(s, false);
                }
                else
                {
                    await File.WriteAllTextAsync(outputPath, await rpeChart.ExportToJsonAsync(false), ct);
                }
                break;
            }
            case ChartType.PhiEdit:
            {
                var peChart = new PhiEditConverter().FromKpc(chart, new KpcToPhiEditConvertOptions());
                if (stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await peChart.ExportToStreamAsync(s);
                }
                else
                {
                    await File.WriteAllTextAsync(outputPath, await peChart.ExportAsync(), ct);
                }
                break;
            }
            default:
                throw new NotSupportedException($"Cannot export to format: {targetType}");
        }

        _log.Info(log_export_done);
        return outputPath;
    }

    public void RunFatherUnbind(Chart chart, double precision, double tolerance, bool classic, bool disableCompress)
    {
        _log.Info(string.Format(log_running_tool, tool_unbind_name));
        var unbinder = new KpcJudgeLineUnbinder();
        for (var i = 0; i < chart.JudgeLineList.Count; i++)
        {
            if (chart.JudgeLineList[i].Father == -1) continue;
            chart.JudgeLineList[i] = classic
                ? unbinder.FatherUnbind(i, chart.JudgeLineList, precision)
                : unbinder.FatherUnbindPlus(i, chart.JudgeLineList, precision, tolerance);
        }
    }

    public void RunLayerMerge(Chart chart, double precision, double tolerance, bool classic, bool disableCompress)
    {
        _log.Info(string.Format(log_running_tool, tool_layermerge_name));
        var processor = new KpcLayerProcessor();
        foreach (var line in chart.JudgeLineList)
        {
            if (line.EventLayers is not { Count: > 1 }) continue;
            var merged = classic
                ? processor.LayerMerge(line.EventLayers, precision)
                : processor.LayerMergePlus(line.EventLayers, precision, tolerance);
            if (!disableCompress)
                processor.LayerEventsCompress(merged, tolerance);
            line.EventLayers.Clear();
            line.EventLayers.Add(merged);
        }
    }

    public void RunCutEvent(Chart chart, double precision, double tolerance, bool disableCompress)
    {
        _log.Info(string.Format(log_running_tool, tool_cut_name));
        var processor = new KpcLayerProcessor();
        foreach (var line in chart.JudgeLineList)
        {
            if (line.EventLayers is not { Count: > 0 }) continue;
            line.EventLayers = processor.CutLayerEvents(line.EventLayers, precision);
            if (!disableCompress)
            {
                foreach (var layer in line.EventLayers)
                    processor.LayerEventsCompress(layer, tolerance);
            }
        }
    }

    public void RunFitEvent(Chart chart, double tolerance)
    {
        _log.Info(string.Format(log_running_tool, tool_fit_name));
        var degree = Environment.ProcessorCount;
        foreach (var line in chart.JudgeLineList)
        {
            if (line.EventLayers is not { Count: > 0 }) continue;
            foreach (var layer in line.EventLayers)
            {
                var doubleFit = new EventFit<double>();
                var intFit = new EventFit<int>();
                var floatFit = new EventFit<float>();

                if (layer.MoveXEvents is { Count: > 0 })
                    layer.MoveXEvents = doubleFit.EventListFit(layer.MoveXEvents, tolerance, degree);
                if (layer.MoveYEvents is { Count: > 0 })
                    layer.MoveYEvents = doubleFit.EventListFit(layer.MoveYEvents, tolerance, degree);
                if (layer.RotateEvents is { Count: > 0 })
                    layer.RotateEvents = doubleFit.EventListFit(layer.RotateEvents, tolerance, degree);
                if (layer.AlphaEvents is { Count: > 0 })
                    layer.AlphaEvents = intFit.EventListFit(layer.AlphaEvents, tolerance, degree);
                if (layer.SpeedEvents is { Count: > 0 })
                    layer.SpeedEvents = floatFit.EventListFit(layer.SpeedEvents, tolerance, degree);
            }
        }
    }

    public IReadOnlyList<string> RunRender(Chart chart, int pixelsPerBeat, int channelWidth, int samples, int beatSubdivisions)
    {
        _log.Info(string.Format(log_running_tool, tool_render_name));
        var outputDir = Path.Combine(_workspaceDir, "render_output");
        var options = new KpcRenderOptions
        {
            PixelsPerBeat = pixelsPerBeat,
            ChannelWidth = channelWidth,
            SamplesPerEvent = samples,
            BeatSubdivisions = beatSubdivisions
        };
        var exporter = new KpcChartRenderExporter();
        return exporter.ExportChart(chart, outputDir, options);
    }
}
