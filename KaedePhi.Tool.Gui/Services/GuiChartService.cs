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
using KaedePhi.Tool.Converter.Phigros.v3.Model;
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
    private readonly string _workspaceDir;
    private readonly string _workspaceFilePath;

    public GuiChartService(LogService log)
    {
        _log = log;
        var workspaceRoot = AppPaths.GetDirectory("workspaces");
        _workspaceDir = Path.Combine(workspaceRoot, WorkspaceId);
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
                var rpeConverter = new RePhiEditConverter();
                rpeConverter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
                var kpcConverter = new KaedePhiConverter();
                kpcConverter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
                return ChartPipeline
                    .From(rpeChart, rpeConverter, null)
                    .To(kpcConverter, null);
            }
            case ChartType.PhiEdit:
            {
                var peChart = Core.PhiEdit.Chart.LoadAsync(text).GetAwaiter().GetResult();
                var peConverter = new PhiEditConverter();
                peConverter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
                var kpcConverter = new KaedePhiConverter();
                kpcConverter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
                return ChartPipeline
                    .From(peChart, peConverter, new PhiEditToKpcConvertOptions())
                    .To(kpcConverter, null);
            }
            case ChartType.PhigrosV3:
            {
                var v3Chart = Core.Phigros.v3.Chart.LoadFromJsonAsync(text).GetAwaiter().GetResult();
                var v3Converter = new PhigrosV3Converter();
                v3Converter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
                var kpcConverter = new KaedePhiConverter();
                kpcConverter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
                return ChartPipeline
                    .From(v3Chart, v3Converter, null)
                    .To(kpcConverter, null);
            }
            default:
                throw new NotSupportedException($"Unsupported chart format: {sourceType}");
        }
    }

    public async Task SaveKpcToWorkspaceAsync(Chart chart, ChartType originalType, bool stream, CancellationToken ct)
    {
        await ConvertFromKpcAndSaveAsync(chart, originalType, _workspaceFilePath, stream, false, ct);
        _log.Info(log_step_saved);
    }

    public async Task<string> ConvertFromKpcAndSaveAsync(
        Chart chart, ChartType targetType, string outputPath, bool stream, bool indented, CancellationToken ct)
    {
        _log.Info(string.Format(log_exporting_to, outputPath, targetType));
        switch (targetType)
        {
            case ChartType.RePhiEdit:
            {
                var rpeConverter = new RePhiEditConverter();
                rpeConverter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
                var rpeChart = rpeConverter.FromKpc(chart, new ConvertOption());
                if (stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await rpeChart.ExportToJsonStreamAsync(s, indented);
                }
                else
                {
                    await File.WriteAllTextAsync(outputPath, await rpeChart.ExportToJsonAsync(indented), ct);
                }
                break;
            }
            case ChartType.PhiEdit:
            {
                var peConverter = new PhiEditConverter();
                peConverter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
                var peChart = peConverter.FromKpc(chart, new KpcToPhiEditConvertOptions());
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
            case ChartType.PhigrosV3:
            {
                var v3Converter = new PhigrosV3Converter();
                v3Converter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
                var phigrosChart = v3Converter.FromKpc(chart, new KpcToPhigrosV3ConvertOptions());
                if (stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await phigrosChart.ExportToJsonStreamAsync(s, indented);
                }
                else
                {
                    await File.WriteAllTextAsync(outputPath, await phigrosChart.ExportToJsonAsync(indented), ct);
                }
                break;
            }
            default:
                throw new NotSupportedException($"Cannot export to format: {targetType}");
        }

        _log.Info(log_export_done);
        return outputPath;
    }

    public void RunFatherUnbind(Chart chart, double precision, double tolerance, bool classic, bool disableCompress,
        IProgress<ToolProgress>? progress = null)
    {
        _log.Info(string.Format(log_running_tool, tool_unbind_name));
        var unbinder = new KpcJudgeLineUnbinder();
        unbinder.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
        var linesToProcess = new List<int>();
        for (var i = 0; i < chart.JudgeLineList.Count; i++)
        {
            if (chart.JudgeLineList[i].Father != -1)
                linesToProcess.Add(i);
        }

        var totalLines = linesToProcess.Count;
        for (var idx = 0; idx < totalLines; idx++)
        {
            var i = linesToProcess[idx];
            var capturedIdx = idx;
            var lineProgress = new Progress<ToolProgress>(p =>
            {
                var overall = (double)capturedIdx / totalLines;
                progress?.Report(new ToolProgress(p.Percentage, overall, p.Detail));
            });
            chart.JudgeLineList[i] = classic
                ? unbinder.FatherUnbind(i, chart.JudgeLineList, precision, lineProgress)
                : unbinder.FatherUnbindPlus(i, chart.JudgeLineList, precision, tolerance, lineProgress);
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    public void RunLayerMerge(Chart chart, double precision, double tolerance, bool classic, bool disableCompress,
        IProgress<ToolProgress>? progress = null)
    {
        _log.Info(string.Format(log_running_tool, tool_layermerge_name));
        var processor = new KpcLayerProcessor();
        processor.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
        var totalLines = chart.JudgeLineList.Count;
        for (var li = 0; li < totalLines; li++)
        {
            var line = chart.JudgeLineList[li];
            if (line.EventLayers is not { Count: > 1 })
            {
                progress?.Report(new ToolProgress(1.0, (double)(li + 1) / totalLines));
                continue;
            }

            var capturedLi = li;
            var lineProgress = new Progress<ToolProgress>(p =>
            {
                var overall = (double)capturedLi / totalLines;
                progress?.Report(new ToolProgress(p.Percentage, overall, p.Detail));
            });
            var merged = classic
                ? processor.LayerMerge(line.EventLayers, precision, lineProgress)
                : processor.LayerMergePlus(line.EventLayers, precision, tolerance, lineProgress);
            if (!disableCompress)
                processor.LayerEventsCompress(merged, tolerance, lineProgress);
            line.EventLayers.Clear();
            line.EventLayers.Add(merged);
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    public void RunCutEvent(Chart chart, double precision, double tolerance, bool disableCompress,
        IProgress<ToolProgress>? progress = null)
    {
        _log.Info(string.Format(log_running_tool, tool_cut_name));
        var processor = new KpcLayerProcessor();
        processor.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
        var totalLines = chart.JudgeLineList.Count;
        for (var li = 0; li < totalLines; li++)
        {
            var line = chart.JudgeLineList[li];
            if (line.EventLayers is not { Count: > 0 })
            {
                progress?.Report(new ToolProgress(1.0, (double)(li + 1) / totalLines));
                continue;
            }

            var capturedLi = li;
            var lineProgress = new Progress<ToolProgress>(p =>
            {
                var overall = (double)capturedLi / totalLines;
                progress?.Report(new ToolProgress(p.Percentage, overall, p.Detail));
            });
            line.EventLayers = processor.CutLayerEvents(line.EventLayers, precision, lineProgress);
            if (!disableCompress)
            {
                foreach (var layer in line.EventLayers)
                    processor.LayerEventsCompress(layer, tolerance, lineProgress);
            }
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    public void RunFitEvent(Chart chart, double tolerance, EventFitOptions? fitOptions = null, IProgress<ToolProgress>? progress = null)
    {
        _log.Info(string.Format(log_running_tool, tool_fit_name));
        var degree = Environment.ProcessorCount;
        var totalLines = chart.JudgeLineList.Count;
        var doubleFit = new EventFit<double>(fitOptions);
        var intFit = new EventFit<int>(fitOptions);
        var floatFit = new EventFit<float>(fitOptions);
        doubleFit.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
        intFit.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);
        floatFit.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s), debug: _log.Info);

        for (var li = 0; li < totalLines; li++)
        {
            var line = chart.JudgeLineList[li];
            if (line.EventLayers is not { Count: > 0 })
            {
                progress?.Report(new ToolProgress(1.0, (double)(li + 1) / totalLines));
                continue;
            }

            var totalLayers = line.EventLayers.Count;
            for (var ei = 0; ei < totalLayers; ei++)
            {
                var capturedLi = li;
                var capturedEi = ei;
                var layerProgress = new Progress<ToolProgress>(p =>
                {
                    var overall = (capturedLi + (double)capturedEi / totalLayers) / totalLines;
                    progress?.Report(new ToolProgress(p.Percentage, overall, p.Detail));
                });
                FitLayer(line.EventLayers[ei], doubleFit, intFit, floatFit, tolerance, degree, layerProgress);
            }
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    private static void FitLayer(Core.KaedePhi.EventLayer layer,
        EventFit<double> doubleFit, EventFit<int> intFit, EventFit<float> floatFit,
        double tolerance, int degree,
        IProgress<ToolProgress>? progress)
    {
        if (layer.MoveXEvents is { Count: > 0 })
            layer.MoveXEvents = doubleFit.EventListFit(layer.MoveXEvents, tolerance, degree, progress);
        if (layer.MoveYEvents is { Count: > 0 })
            layer.MoveYEvents = doubleFit.EventListFit(layer.MoveYEvents, tolerance, degree, progress);
        if (layer.RotateEvents is { Count: > 0 })
            layer.RotateEvents = doubleFit.EventListFit(layer.RotateEvents, tolerance, degree, progress);
        if (layer.AlphaEvents is { Count: > 0 })
            layer.AlphaEvents = intFit.EventListFit(layer.AlphaEvents, tolerance, degree, progress);
        if (layer.SpeedEvents is { Count: > 0 })
            layer.SpeedEvents = floatFit.EventListFit(layer.SpeedEvents, tolerance, degree, progress);
    }

    public IReadOnlyList<string> RunRender(Chart chart, int pixelsPerBeat, int channelWidth, int samples, int beatSubdivisions,
        IProgress<ToolProgress>? progress = null)
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
        exporter.SubscribeLog(info: _log.Info, warning: _log.Warn, error: s => _log.Error(s));
        return exporter.ExportChart(chart, outputDir, options, progress: progress);
    }
}
