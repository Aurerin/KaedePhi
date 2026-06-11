using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KaedePhi.Core.KaedePhi.Events;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using KaedePhi.Tool.Converter.KaedePhi;
using KaedePhi.Tool.Converter.PhiChain;
using KaedePhi.Tool.Converter.PhiChain.Model;
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
using Serilog;
using static KaedePhi.Tool.Localization.GuiLocalizationString;
using Chart = KaedePhi.Core.KaedePhi.Chart;

namespace KaedePhi.Tool.Gui.Services;

public sealed class GuiChartService
{
    private readonly ILogger _log;

    public GuiChartService(LogService logService)
    {
        _log = logService.ForContext<GuiChartService>();
    }

    /// <summary>
    /// 当前加载的 KPC 图表（内存中）
    /// </summary>
    public Chart? CurrentChart { get; private set; }

    /// <summary>
    /// 源文件的格式类型
    /// </summary>
    public ChartType SourceFormat { get; private set; }

    /// <summary>
    /// 源文件路径
    /// </summary>
    public string? SourceFilePath { get; private set; }

    /// <summary>
    /// 是否已加载图表
    /// </summary>
    public bool IsLoaded => CurrentChart != null;

    /// <summary>
    /// 从文件加载图表并转换为 KPC 格式存储在内存中
    /// </summary>
    public async Task<(Chart Chart, ChartType DetectedType)> LoadChartAsync(
        string filePath,
        bool stream,
        CancellationToken ct
    )
    {
        _log.Information(log_file_selected, filePath, stream);

        string text;
        if (stream)
        {
            using var reader = new StreamReader(filePath);
            text = await reader.ReadToEndAsync(ct);
        }
        else
        {
            text = await File.ReadAllTextAsync(filePath, ct);
        }

        var detectedType = ChartGetType.GetType(text);
        _log.Information(log_step_detected, detectedType);

        // 在后台线程执行耗时的格式转换
        var kpcChart = await Task.Run(() => ConvertToKpc(text, detectedType), ct);

        CurrentChart = kpcChart;
        SourceFormat = detectedType;
        SourceFilePath = filePath;

        return (kpcChart, detectedType);
    }

    /// <summary>
    /// 将当前 KPC 图表导出到指定格式和路径
    /// </summary>
    public async Task ExportChartAsync(
        ChartType targetType,
        string outputPath,
        bool stream,
        bool indented,
        KpcToPhigrosV3ConvertOptions? phigrosOptions = null,
        CancellationToken ct = default
    )
    {
        if (CurrentChart == null)
            throw new InvalidOperationException("No chart loaded");

        _log.Information(log_exporting_to, outputPath, targetType);
        await ConvertFromKpcAndSaveAsync(
            CurrentChart,
            targetType,
            outputPath,
            stream,
            indented,
            phigrosOptions,
            ct
        );
        _log.Information(log_export_done);
    }

    /// <summary>
    /// 清除当前加载的图表
    /// </summary>
    public void Clear()
    {
        CurrentChart = null;
        SourceFormat = default;
        SourceFilePath = null;
    }

    private Chart ConvertToKpc(string text, ChartType sourceType)
    {
        _log.Information(log_step_converting);
        switch (sourceType)
        {
            case ChartType.RePhiEdit:
            {
                var rpeChart = Core
                    .RePhiEdit.Chart.LoadFromJsonAsync(text)
                    .GetAwaiter()
                    .GetResult();
                var rpeConverter = new RePhiEditConverter();
                rpeConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                var kpcConverter = new KaedePhiConverter();
                kpcConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                return ChartPipeline.From(rpeChart, rpeConverter, null).To(kpcConverter, null);
            }
            case ChartType.PhiEdit:
            {
                var peChart = Core.PhiEdit.Chart.LoadAsync(text).GetAwaiter().GetResult();
                var peConverter = new PhiEditConverter();
                peConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                var kpcConverter = new KaedePhiConverter();
                kpcConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                return ChartPipeline
                    .From(peChart, peConverter, new PhiEditToKpcConvertOptions())
                    .To(kpcConverter, null);
            }
            case ChartType.PhigrosV3:
            {
                var v3Chart = Core
                    .Phigros.v3.Chart.LoadFromJsonAsync(text)
                    .GetAwaiter()
                    .GetResult();
                var v3Converter = new PhigrosV3Converter();
                v3Converter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                var kpcConverter = new KaedePhiConverter();
                kpcConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                return ChartPipeline.From(v3Chart, v3Converter, null).To(kpcConverter, null);
            }
            case ChartType.PhiChain:
            {
                var pcChart = Core.PhiChain.v6.Chart.LoadFromJson(text);
                var pcConverter = new PhiChainConverter();
                pcConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                var kpcConverter = new KaedePhiConverter();
                kpcConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                return ChartPipeline
                    .From(pcChart, pcConverter, new PhiChainToKpcConvertOptions())
                    .To(kpcConverter, null);
            }
            default:
                throw new NotSupportedException($"Unsupported chart format: {sourceType}");
        }
    }

    private async Task ConvertFromKpcAndSaveAsync(
        Chart chart,
        ChartType targetType,
        string outputPath,
        bool stream,
        bool indented,
        KpcToPhigrosV3ConvertOptions? phigrosOptions,
        CancellationToken ct
    )
    {
        _log.Information(log_exporting_to, outputPath, targetType);
        switch (targetType)
        {
            case ChartType.RePhiEdit:
            {
                var rpeConverter = new RePhiEditConverter();
                rpeConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                var rpeChart = rpeConverter.FromKpc(chart, new ConvertOption());
                if (stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await rpeChart.ExportToJsonStreamAsync(s, indented);
                }
                else
                {
                    await File.WriteAllTextAsync(
                        outputPath,
                        await rpeChart.ExportToJsonAsync(indented),
                        ct
                    );
                }
                break;
            }
            case ChartType.PhiEdit:
            {
                var peConverter = new PhiEditConverter();
                peConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
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
                v3Converter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                var phigrosChart = v3Converter.FromKpc(
                    chart,
                    phigrosOptions ?? new KpcToPhigrosV3ConvertOptions()
                );
                if (stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await phigrosChart.ExportToJsonStreamAsync(s, indented);
                }
                else
                {
                    await File.WriteAllTextAsync(
                        outputPath,
                        await phigrosChart.ExportToJsonAsync(indented),
                        ct
                    );
                }
                break;
            }
            case ChartType.PhiChain:
            {
                var pcConverter = new PhiChainConverter();
                pcConverter.SubscribeLog(
                    info: msg => _log.Information(msg),
                    warning: msg => _log.Warning(msg),
                    error: msg => _log.Error(msg),
                    debug: msg => _log.Debug(msg)
                );
                var pcChart = pcConverter.FromKpc(chart, new KpcToPhiChainConvertOptions());
                if (stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await pcChart.ExportToJsonStreamAsync(s, indented);
                }
                else
                {
                    await File.WriteAllTextAsync(
                        outputPath,
                        await pcChart.ExportToJsonAsync(indented),
                        ct
                    );
                }
                break;
            }
            default:
                throw new NotSupportedException($"Cannot export to format: {targetType}");
        }

        _log.Information(log_export_done);
    }

    public void RunFatherUnbind(
        Chart chart,
        double precision,
        double tolerance,
        bool classic,
        bool disableCompress,
        IProgress<ToolProgress>? progress = null
    )
    {
        _log.Information(log_running_tool, tool_unbind_name);
        var unbinder = new JudgeLineUnbinder();
        unbinder.SubscribeLog(
            info: msg => _log.Information(msg),
            warning: msg => _log.Warning(msg),
            error: msg => _log.Error(msg),
            debug: msg => _log.Debug(msg)
        );
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
                : unbinder.FatherUnbind(i, chart.JudgeLineList, precision, tolerance, lineProgress);
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    public void RunLayerMerge(
        Chart chart,
        double precision,
        double tolerance,
        bool classic,
        bool disableCompress,
        IProgress<ToolProgress>? progress = null
    )
    {
        _log.Information(log_running_tool, tool_layermerge_name);
        var processor = new LayerProcessor();
        processor.SubscribeLog(
            info: msg => _log.Information(msg),
            warning: msg => _log.Warning(msg),
            error: msg => _log.Error(msg),
            debug: msg => _log.Debug(msg)
        );
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

    public void RunCutEvent(
        Chart chart,
        double precision,
        double tolerance,
        bool disableCompress,
        IProgress<ToolProgress>? progress = null
    )
    {
        _log.Information(log_running_tool, tool_cut_name);
        var processor = new LayerProcessor();
        processor.SubscribeLog(
            info: msg => _log.Information(msg),
            warning: msg => _log.Warning(msg),
            error: msg => _log.Error(msg),
            debug: msg => _log.Debug(msg)
        );
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

    public void RunFitEvent(Chart chart, double tolerance, IProgress<ToolProgress>? progress = null)
    {
        _log.Information(log_running_tool, tool_fit_name);
        var totalLines = chart.JudgeLineList.Count;
        var doubleFit = new EventFit<double>();
        var intFit = new EventFit<int>();
        var floatFit = new EventFit<float>();
        doubleFit.SubscribeLog(
            info: msg => _log.Information(msg),
            warning: msg => _log.Warning(msg),
            error: msg => _log.Error(msg),
            debug: msg => _log.Debug(msg)
        );
        intFit.SubscribeLog(
            info: msg => _log.Information(msg),
            warning: msg => _log.Warning(msg),
            error: msg => _log.Error(msg),
            debug: msg => _log.Debug(msg)
        );
        floatFit.SubscribeLog(
            info: msg => _log.Information(msg),
            warning: msg => _log.Warning(msg),
            error: msg => _log.Error(msg),
            debug: msg => _log.Debug(msg)
        );

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
                FitLayer(
                    line.EventLayers[ei],
                    doubleFit,
                    intFit,
                    floatFit,
                    tolerance,
                    layerProgress
                );
            }
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    private static void FitLayer(
        EventLayer layer,
        EventFit<double> doubleFit,
        EventFit<int> intFit,
        EventFit<float> floatFit,
        double tolerance,
        IProgress<ToolProgress>? progress
    )
    {
        if (layer.MoveXEvents is { Count: > 0 })
            layer.MoveXEvents = doubleFit.FitEvents(layer.MoveXEvents, tolerance);
        if (layer.MoveYEvents is { Count: > 0 })
            layer.MoveYEvents = doubleFit.FitEvents(layer.MoveYEvents, tolerance);
        if (layer.RotateEvents is { Count: > 0 })
            layer.RotateEvents = doubleFit.FitEvents(layer.RotateEvents, tolerance);
        if (layer.AlphaEvents is { Count: > 0 })
            layer.AlphaEvents = intFit.FitEvents(layer.AlphaEvents, tolerance);
        if (layer.SpeedEvents is { Count: > 0 })
            layer.SpeedEvents = floatFit.FitEvents(layer.SpeedEvents, tolerance);
        progress?.Report(new ToolProgress(1.0));
    }

    public IReadOnlyList<string> RunRender(
        Chart chart,
        int pixelsPerBeat,
        int channelWidth,
        int samples,
        int beatSubdivisions,
        IProgress<ToolProgress>? progress = null
    )
    {
        _log.Information(log_running_tool, tool_render_name);
        var outputDir = Path.Combine(
            Path.GetTempPath(),
            "kaedephi_render_" + Guid.NewGuid().ToString("N")[..8]
        );
        Directory.CreateDirectory(outputDir);
        var options = new KpcRenderOptions
        {
            PixelsPerBeat = pixelsPerBeat,
            ChannelWidth = channelWidth,
            SamplesPerEvent = samples,
            BeatSubdivisions = beatSubdivisions,
        };
        var exporter = new KpcChartRenderExporter();
        exporter.SubscribeLog(
            info: msg => _log.Information(msg),
            warning: msg => _log.Warning(msg),
            error: msg => _log.Error(msg)
        );
        return exporter.ExportChart(chart, outputDir, options, progress: progress);
    }
}
