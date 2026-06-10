using KaedePhi.Core.Common;
using KaedePhi.Core.PhiChain.v6;
using KaedePhi.Tool.Converter.PhiChain.Model;
using KaedePhi.Tool.JudgeLines.KaedePhi;
using KaedePhi.Tool.Layer.KaedePhi;
using PhichainEasingKind = KaedePhi.Core.PhiChain.v6.EasingKind;

namespace KaedePhi.Tool.Converter.PhiChain.Utils;

/// <summary>
/// PhiChain 与 KPC 判定线之间的双向转换工具。
/// 处理树形线结构与扁平索引结构之间的转换。
/// </summary>
public static class JudgeLineBuilder
{
    /// <summary>
    /// 将 PhiChain 树形线结构展开为 KPC 扁平判定线列表。
    /// </summary>
    /// <param name="line">PhiChain 序列化线</param>
    /// <param name="fatherIndex">父级线索引，-1 表示无父级</param>
    /// <param name="result">结果列表</param>
    /// <param name="currentIndex">当前线索引（引用传递）</param>
    /// <param name="options">转换选项</param>
    public static void FlattenLine(
        SerializedLine line,
        int fatherIndex,
        List<Kpc.JudgeLine> result,
        ref int currentIndex,
        PhichainToKpcConvertOptions options)
    {
        var kpcLine = ConvertLine(line, fatherIndex, currentIndex, options);
        result.Add(kpcLine);

        var currentIdx = currentIndex;
        currentIndex++;

        // 递归处理子线
        foreach (var child in line.Children)
        {
            FlattenLine(child, currentIdx, result, ref currentIndex, options);
        }
    }

    /// <summary>
    /// 将 PhiChain 序列化线转换为 KPC 判定线。
    /// </summary>
    /// <param name="src">PhiChain 序列化线</param>
    /// <param name="fatherIndex">父级线索引</param>
    /// <param name="lineIndex">当前线索引</param>
    /// <param name="options">转换选项</param>
    /// <returns>KPC 判定线</returns>
    public static Kpc.JudgeLine ConvertLine(SerializedLine src, int fatherIndex, int lineIndex, PhichainToKpcConvertOptions options)
    {
        var kpcLine = new Kpc.JudgeLine
        {
            Name = src.Name,
            Father = fatherIndex,
            RotateWithFather = true,
        };

        // 转换事件，处理不支持的缓动
        var allEvents = new List<LineEvent>();
        foreach (var evt in src.Events)
        {
            if (EasingConverter.NeedsLinearSlicing(evt.Value.Easing))
            {
                allEvents.AddRange(EventBuilder.SliceUnsupportedEasing(evt, options.UnsupportedEasingPrecision));
            }
            else
            {
                allEvents.Add(evt);
            }
        }

        var eventLayer = EventBuilder.ConvertEvents(allEvents);
        kpcLine.EventLayers.Add(eventLayer);

        // 转换普通音符
        var notes = src.Notes.ConvertAll(NoteBuilder.ConvertNote);

        // 展开 CurveNoteTrack（链式音符）
        if (src.CurveNoteTracks.Count > 0)
        {
            var expandedNotes = ExpandCurveNoteTracks(src);
            notes.AddRange(expandedNotes);
        }

        kpcLine.Notes = notes;

        return kpcLine;
    }

    /// <summary>
    /// 展开所有 CurveNoteTrack 为普通音符。
    /// </summary>
    /// <param name="src">PhiChain 序列化线</param>
    /// <returns>展开后的音符列表</returns>
    private static List<Kpc.Note> ExpandCurveNoteTracks(SerializedLine src)
    {
        var notes = new List<Kpc.Note>();

        foreach (var track in src.CurveNoteTracks)
        {
            // 验证索引范围
            if (track.From < 0 || track.From >= src.Notes.Count ||
                track.To < 0 || track.To >= src.Notes.Count)
            {
                continue;
            }

            var fromNote = src.Notes[track.From];
            var toNote = src.Notes[track.To];
            var expanded = NoteBuilder.ExpandCurveNoteTrack(track, fromNote, toNote);
            notes.AddRange(expanded);
        }

        return notes;
    }

    /// <summary>
    /// 将 KPC 扁平判定线列表构建为 PhiChain 树形线结构。
    /// </summary>
    /// <param name="kpcLines">KPC 判定线列表</param>
    /// <param name="options">转换选项</param>
    /// <returns>PhiChain 序列化线列表</returns>
    public static List<SerializedLine> BuildLineTree(
        List<Kpc.JudgeLine> kpcLines,
        KpcToPhichainConvertOptions options)
    {
        // 预处理：解绑 rotateWithFather 为 false 的子线
        var processedLines = PreprocessLines(kpcLines, options);

        var result = new List<SerializedLine>();
        var childMap = new Dictionary<int, List<int>>();

        // 构建父子关系映射
        for (var i = 0; i < processedLines.Count; i++)
        {
            var father = processedLines[i].Father;
            if (father >= 0)
            {
                if (!childMap.ContainsKey(father))
                    childMap[father] = new List<int>();
                childMap[father].Add(i);
            }
        }

        // 从根节点开始构建树
        for (var i = 0; i < processedLines.Count; i++)
        {
            if (processedLines[i].Father < 0)
            {
                result.Add(BuildLineSubtree(processedLines, i, childMap, options));
            }
        }

        return result;
    }

    /// <summary>
    /// 预处理判定线列表：解绑 rotateWithFather 为 false 的子线。
    /// </summary>
    private static List<Kpc.JudgeLine> PreprocessLines(
        List<Kpc.JudgeLine> kpcLines,
        KpcToPhichainConvertOptions options)
    {
        if (!options.UnbindNonRotatingChildren)
            return kpcLines;

        var unbinder = new JudgeLineUnbinder();
        var result = kpcLines.Select(l => l.Clone()).ToList();

        // 找出所有需要解绑的子线（rotateWithFather 为 false）
        var linesToUnbind = new List<int>();
        for (var i = 0; i < result.Count; i++)
        {
            if (result[i].Father >= 0 && !result[i].RotateWithFather)
            {
                linesToUnbind.Add(i);
            }
        }

        // 使用 JudgeLineUnbinder 解绑
        foreach (var lineIndex in linesToUnbind)
        {
            if (options.UnbindClassicMode)
            {
                result[lineIndex] = unbinder.FatherUnbind(lineIndex, result, options.UnbindPrecision);
            }
            else
            {
                result[lineIndex] = unbinder.FatherUnbind(lineIndex, result, options.UnbindPrecision, options.UnbindTolerance);
            }
        }

        return result;
    }

    /// <summary>
    /// 递归构建子树。
    /// </summary>
    private static SerializedLine BuildLineSubtree(
        List<Kpc.JudgeLine> kpcLines,
        int lineIndex,
        Dictionary<int, List<int>> childMap,
        KpcToPhichainConvertOptions options)
    {
        var kpcLine = kpcLines[lineIndex];
        var serializedLine = ConvertLineToPhichain(kpcLine, options);

        // 递归处理子线
        if (childMap.TryGetValue(lineIndex, out var children))
        {
            foreach (var childIndex in children)
            {
                serializedLine.Children.Add(
                    BuildLineSubtree(kpcLines, childIndex, childMap, options)
                );
            }
        }

        return serializedLine;
    }

    /// <summary>
    /// 将 KPC 判定线转换为 PhiChain 序列化线。
    /// </summary>
    /// <param name="src">KPC 判定线</param>
    /// <param name="options">转换选项</param>
    /// <returns>PhiChain 序列化线</returns>
    private static SerializedLine ConvertLineToPhichain(Kpc.JudgeLine src, KpcToPhichainConvertOptions options)
    {
        var line = new SerializedLine
        {
            Name = src.Name,
            Notes = src.Notes.ConvertAll(NoteBuilder.ConvertNote),
        };

        // 使用 LayerProcessor 合并多个事件层（phichain 不支持多层级）
        if (src.EventLayers.Count > 0)
        {
            var processor = new LayerProcessor();
            KpcEvents.EventLayer mergedLayer;

            if (options.MultiLayerMergeClassicMode)
            {
                mergedLayer = processor.LayerMerge(src.EventLayers, options.MultiLayerMergePrecision);
            }
            else
            {
                mergedLayer = processor.LayerMergePlus(src.EventLayers, options.MultiLayerMergePrecision, options.MultiLayerMergeTolerance);
            }

            // 转换合并后的事件层为 PhiChain 事件列表
            line.Events = EventBuilder.ConvertEventLayer(mergedLayer, options);
        }
        else
        {
            line.Events = new List<LineEvent>();
        }

        return line;
    }
}
