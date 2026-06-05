using KaedePhi.Tool.Common;
using SkiaSharp;
using Chart = KaedePhi.Core.KaedePhi.Chart;

namespace KaedePhi.Tool.Render.KaedePhi;

/// <summary>
/// KPC 谱面渲染导出器：将谱面各判定线、各事件层渲染为 PNG 图片并写入目录。
/// </summary>
public class KpcChartRenderExporter : LoggableBase, IChartRenderExporter<Chart, KpcRenderOptions>
{
    /// <inheritdoc/>
    public IReadOnlyList<string> ExportChart(
        Chart chart,
        string outputDir,
        KpcRenderOptions opts,
        int? lineIndex = null,
        int? layerIndex = null,
        IProgress<ToolProgress>? progress = null
    )
    {
        Directory.CreateDirectory(outputDir);
        var written = new List<string>();

        int lineStart = lineIndex ?? 0;
        int lineEnd = lineIndex.HasValue ? lineIndex.Value + 1 : chart.JudgeLineList.Count;
        int totalLines = lineEnd - lineStart;
        int completedLines = 0;

        for (int li = lineStart; li < lineEnd; li++)
        {
            if (li >= chart.JudgeLineList.Count)
                break;
            var line = chart.JudgeLineList[li];
            if (line.EventLayers == null || line.EventLayers.Count == 0)
            {
                completedLines++;
                continue;
            }

            string safeName = SanitizeFileName(line.Name ?? $"line{li}");

            int layerStart = layerIndex ?? 0;
            int layerEnd = layerIndex.HasValue ? layerIndex.Value + 1 : line.EventLayers.Count;
            int totalLayers = layerEnd - layerStart;
            int completedLayers = 0;

            for (int ei = layerStart; ei < layerEnd; ei++)
            {
                if (ei >= line.EventLayers.Count)
                    break;
                var eventLayer = line.EventLayers[ei];
                if (eventLayer == null)
                {
                    completedLayers++;
                    continue;
                }

                LogInfo($"渲染 [{li}]{safeName} 第 {ei} 层...");

                using var bitmap = KpcEventChannelRenderer.RenderEventLayer(eventLayer, opts);
                string filename = $"{safeName}_L{li}_layer{ei}.png";
                string filePath = Path.Combine(outputDir, filename);

                SaveBitmap(bitmap, filePath);
                written.Add(filePath);
                LogInfo($"  已写入: {filePath}");

                completedLayers++;
                var lineProgress = (double)completedLines / totalLines;
                var layerProgress = (double)completedLayers / totalLayers / totalLines;
                progress?.Report(
                    new ToolProgress(lineProgress + layerProgress, $"[{li}]{safeName} layer{ei}")
                );
            }

            completedLines++;
        }

        progress?.Report(new ToolProgress(1.0));
        return written;
    }

    private static void SaveBitmap(SKBitmap bitmap, string filePath)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars).Trim('.', ' ');
    }
}
