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
using Chart = KaedePhi.Core.KaedePhi.Chart;

namespace KaedePhi.Tool.Cli.Infrastructure;

/// <summary>
/// <see cref="ChartService.SaveAsAsync"/> 的导出选项。
/// </summary>
public sealed record SaveAsOptions
{
    /// <summary>是否使用流式写入（适合大文件）。</summary>
    public bool Stream { get; init; }

    /// <summary>是否格式化 JSON 输出。</summary>
    public bool Format { get; init; }

    /// <summary>演习模式：不实际写文件，仅返回目标路径。</summary>
    public bool DryRun { get; init; }

    /// <summary>PhiEdit 转换选项（仅 <see cref="ChartType.PhiEdit"/> 时生效）。</summary>
    public KpcToPhiEditConvertOptions? PhiEditOptions { get; init; }

    /// <summary>Phigros v3 转换选项（仅 <see cref="ChartType.PhigrosV3"/> 时生效）。</summary>
    public KpcToPhigrosV3ConvertOptions? PhigrosOptions { get; init; }
}

/// <summary>
/// 谱面加载、格式检测与导出服务。
/// </summary>
public sealed class ChartService
{
    private readonly WorkspaceService _workspace = new();

    /// <summary>从文件路径或工作区加载原始文本。</summary>
    public async Task<string> LoadChartTextAsync(
        string? input,
        string? workspace,
        CancellationToken ct = default
    )
    {
        string path;
        if (!string.IsNullOrWhiteSpace(workspace))
        {
            path =
                _workspace.GetChartPath(workspace)
                ?? throw new InvalidOperationException(
                    string.Format(CliLocalizationString.err_workspace_missing, workspace)
                );
        }
        else
        {
            path =
                input
                ?? throw new InvalidOperationException(CliLocalizationString.err_input_required);
        }

        return await File.ReadAllTextAsync(path, ct);
    }

    /// <summary>将输入谱面统一转换为中间类型。</summary>
    public async Task<Chart?> LoadKpcAsync(
        string? input,
        string? workspace,
        CancellationToken ct = default
    )
    {
        var text = await LoadChartTextAsync(input, workspace, ct);
        var chartType = ChartGetType.GetType(text);

        switch (chartType)
        {
            case ChartType.RePhiEdit:
            {
                var rePhiEditConverter = new RePhiEditConverter();
                var kaedePhiConverter = new KaedePhiConverter();
                var rePhiEditChart = await Core.RePhiEdit.Chart.LoadFromJsonAsync(text);

                return ChartPipeline
                    .From(rePhiEditChart, rePhiEditConverter, null)
                    .To(kaedePhiConverter, null);
            }

            case ChartType.PhiEdit:
            {
                var phiEditConverter = new PhiEditConverter();
                var kaedePhiConverter = new KaedePhiConverter();
                var phiEditChart = await Core.PhiEdit.Chart.LoadAsync(text);
                return ChartPipeline
                    .From(phiEditChart, phiEditConverter, new PhiEditToKpcConvertOptions())
                    .To(kaedePhiConverter, null);
            }

            case ChartType.PhigrosV3:
            {
                var phigrosV3Converter = new PhigrosV3Converter();
                var kaedePhiConverter = new KaedePhiConverter();
                var phigrosV3Chart = await Core.Phigros.v3.Chart.LoadFromJsonAsync(text);
                return ChartPipeline
                    .From(phigrosV3Chart, phigrosV3Converter, null)
                    .To(kaedePhiConverter, null);
            }

            case ChartType.PhiChain:
            {
                var phiChainConverter = new PhiChainConverter();
                var kaedePhiConverter = new KaedePhiConverter();
                var phiChainChart = Core.PhiChain.v6.Chart.LoadFromJson(text);
                return ChartPipeline
                    .From(phiChainChart, phiChainConverter, new PhiChainToKpcConvertOptions())
                    .To(kaedePhiConverter, null);
            }

            default:
                return null;
        }
    }

    /// <summary>根据输入路径或工作区自动计算输出路径。</summary>
    public string ResolveOutputPath(string? input, string? output, string? workspace)
    {
        if (!string.IsNullOrWhiteSpace(output))
            return output;
        if (string.IsNullOrEmpty(input))
            throw new InvalidOperationException(CliLocalizationString.err_input_required);
        if (string.IsNullOrWhiteSpace(workspace))
            return Path.Combine(
                Path.GetDirectoryName(input) ?? ".",
                Path.GetFileNameWithoutExtension(input) + "_PFC.json"
            );
        return Path.Combine(_workspace.Root, workspace, "chart.json");
    }

    /// <summary>将 KPC 谱面导出为 RPE 格式并写入。</summary>
    public static async Task<string> SaveAsRpeAsync(
        Chart chart,
        string outputPath,
        bool dryRun,
        CancellationToken ct = default
    )
    {
        var rpeChart = new RePhiEditConverter().FromKpc(chart, new ConvertOption());
        if (dryRun)
            return outputPath;
        var json = await rpeChart.ExportToJsonAsync(false);
        await File.WriteAllTextAsync(outputPath, json, ct);
        return outputPath;
    }

    /// <summary>将 KPC 谱面导出为目标格式并写入。</summary>
    public static async Task<string?> SaveAsAsync(
        Chart chart,
        string outputPath,
        ChartType target,
        SaveAsOptions options,
        CancellationToken ct = default
    )
    {
        if (options.DryRun)
            return target is ChartType.RePhiEdit or ChartType.PhiEdit or ChartType.PhigrosV3 or ChartType.PhiChain
                ? outputPath
                : null;
        switch (target)
        {
            case ChartType.RePhiEdit:
            {
                var rpeChart = new RePhiEditConverter().FromKpc(chart, new ConvertOption());

                if (options.Stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await rpeChart.ExportToJsonStreamAsync(s, options.Format);
                }
                else
                {
                    await File.WriteAllTextAsync(
                        outputPath,
                        await rpeChart.ExportToJsonAsync(options.Format),
                        ct
                    );
                }

                return outputPath;
            }
            case ChartType.PhiEdit:
            {
                var peChart = new PhiEditConverter().FromKpc(
                    chart,
                    options.PhiEditOptions ?? new KpcToPhiEditConvertOptions()
                );
                if (options.Stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await peChart.ExportToStreamAsync(s);
                }
                else
                {
                    await File.WriteAllTextAsync(outputPath, await peChart.ExportAsync(), ct);
                }

                return outputPath;
            }
            case ChartType.PhigrosV3:
            {
                var phigrosChart = new PhigrosV3Converter().FromKpc(
                    chart,
                    options.PhigrosOptions ?? new KpcToPhigrosV3ConvertOptions()
                );
                if (options.Stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await phigrosChart.ExportToJsonStreamAsync(s, options.Format);
                }
                else
                {
                    await File.WriteAllTextAsync(
                        outputPath,
                        await phigrosChart.ExportToJsonAsync(options.Format),
                        ct
                    );
                }

                return outputPath;
            }
            case ChartType.PhiChain:
            {
                var phiChainChart = new PhiChainConverter().FromKpc(chart, new KpcToPhiChainConvertOptions());
                if (options.Stream)
                {
                    await using var s = new FileStream(outputPath, FileMode.Create);
                    await phiChainChart.ExportToJsonStreamAsync(s, options.Format);
                }
                else
                {
                    await File.WriteAllTextAsync(
                        outputPath,
                        await phiChainChart.ExportToJsonAsync(options.Format),
                        ct
                    );
                }

                return outputPath;
            }
            default:
                return null;
        }
    }
}
