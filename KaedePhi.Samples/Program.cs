using KaedePhi.Core.Common;
using KaedePhi.Core.KaedePhi.Events;
using KaedePhi.Samples;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using KaedePhi.Tool.Converter.KaedePhi;
using KaedePhi.Tool.Converter.RePhiEdit;
using KaedePhi.Tool.Converter.RePhiEdit.Model;
using KaedePhi.Tool.Event.KaedePhi;
using KpcChart = KaedePhi.Core.KaedePhi.Chart;
using RpeChart = KaedePhi.Core.RePhiEdit.Chart;

Console.WriteLine("KaedePhi.Samples — Core / Tool 快速入门");
Console.WriteLine(new string('=', 48));

await RunCoreSamplesAsync();
await RunToolSamplesAsync();

Console.WriteLine();
Console.WriteLine("全部示例运行完成。");
Console.WriteLine("下一步：把 SampleChartData.MinimalRePhiEditJson 换成你的谱面文本，");
Console.WriteLine("或参考 KaedePhi.Tool.Cli / KaedePhi.Tool.Gui 中的完整工作流。");

static async Task RunCoreSamplesAsync()
{
    PrintSection("Core：读取、检查与序列化 RePhiEdit 谱面");

    // 1. 从 JSON 文本反序列化为强类型模型
    var rpeChart = RpeChart.LoadFromJson(SampleChartData.MinimalRePhiEditJson);
    Console.WriteLine($"谱面名称：{rpeChart.Meta.Name}");
    Console.WriteLine($"BPM 数量：{rpeChart.BpmList.Count}，首项 BPM：{rpeChart.BpmList[0].Bpm}");
    Console.WriteLine($"判定线数量：{rpeChart.JudgeLineList.Count}");

    // 2. 使用 Core 通用类型 Beat 表示拍号
    var startBeat = new Beat([0, 0, 1]);
    var endBeat = new Beat([0, 2, 1]);
    Console.WriteLine($"拍号 {startBeat} 到 {endBeat}，换算为小数拍：{(double)startBeat:F3} → {(double)endBeat:F3}");

    // 3. 序列化后再读回，验证往返一致性
    var exportedJson = await rpeChart.ExportToJsonAsync(format: false);
    var roundTrip = await RpeChart.LoadFromJsonAsync(exportedJson);
    Console.WriteLine($"往返序列化后谱面名称一致：{roundTrip.Meta.Name == rpeChart.Meta.Name}");
}

static async Task RunToolSamplesAsync()
{
    PrintSection("Tool：格式识别、转换管线与事件压缩");

    var chartText = SampleChartData.MinimalRePhiEditJson;

    // 1. 自动识别谱面格式
    var chartType = ChartGetType.GetType(chartText);
    Console.WriteLine($"识别到的谱面格式：{chartType}");

    // 2. 通过 ChartPipeline 走 KPC 中间格式完成转换
    var rpeChart = await RpeChart.LoadFromJsonAsync(chartText);
    var kpcChart = ChartPipeline
        .From(rpeChart, new RePhiEditConverter(), inOptions: null)
        .To(new KaedePhiConverter(), outOptions: null);

    Console.WriteLine($"转换到 KPC 后判定线数量：{kpcChart.JudgeLineList.Count}");

    var convertedBack = ChartPipeline
        .From(kpcChart, new KaedePhiConverter(), inOptions: null)
        .To(new RePhiEditConverter(), new ConvertOption());

    var outputPath = Path.Combine(Path.GetTempPath(), "kaedephi-sample-output.json");
    await File.WriteAllTextAsync(outputPath, await convertedBack.ExportToJsonAsync(format: true));
    Console.WriteLine($"已导出转换结果：{outputPath}");

    // 3. 在 KPC 上运行 Tool 提供的事件压缩
    var eventCountBefore = CountMoveXEvents(kpcChart);
    CompressMoveXEvents(kpcChart, tolerance: 5d);
    var eventCountAfter = CountMoveXEvents(kpcChart);
    Console.WriteLine($"MoveX 事件压缩：{eventCountBefore} → {eventCountAfter}");
}

static void CompressMoveXEvents(KpcChart chart, double tolerance)
{
    var compressor = new EventCompressor<double>();

    foreach (var line in chart.JudgeLineList)
    {
        foreach (var layer in line.EventLayers.OfType<EventLayer>())
        {
            layer.MoveXEvents = compressor.EventListCompressSqrt(
                layer.MoveXEvents ?? [],
                tolerance
            );
        }
    }
}

static int CountMoveXEvents(KpcChart chart)
{
    return chart.JudgeLineList.Sum(line =>
        line.EventLayers.OfType<EventLayer>().Sum(layer => layer.MoveXEvents?.Count ?? 0)
    );
}

static void PrintSection(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}
