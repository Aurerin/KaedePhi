using KaedePhi.Core.Common;
using KaedePhi.Core.PhiChain.v6;
using KaedePhi.Tool.Converter.PhiChain.Model;
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

        var result = new List<Kpc.JudgeLine>();

        for (var i = 0; i < kpcLines.Count; i++)
        {
            var line = kpcLines[i];

            // 如果是子线且 rotateWithFather 为 false，将其提升为根线
            if (line.Father >= 0 && !line.RotateWithFather)
            {
                // 合并父线的位置事件到子线
                var mergedLine = MergeFatherEvents(line, kpcLines);
                result.Add(mergedLine);
            }
            else
            {
                result.Add(line);
            }
        }

        // 更新父子关系索引
        UpdateFatherIndices(result);

        return result;
    }

    /// <summary>
    /// 合并父线的 X/Y 事件到子线（不包含旋转）。
    /// </summary>
    private static Kpc.JudgeLine MergeFatherEvents(
        Kpc.JudgeLine childLine,
        List<Kpc.JudgeLine> allLines)
    {
        if (childLine.Father < 0 || childLine.Father >= allLines.Count)
            return childLine;

        var fatherLine = allLines[childLine.Father];
        var mergedLine = childLine.Clone();

        // 获取父线的所有事件层
        var fatherEvents = new List<LineEvent>();
        foreach (var layer in fatherLine.EventLayers)
        {
            fatherEvents.AddRange(EventBuilder.ConvertEventLayer(layer));
        }

        // 只取 X 和 Y 事件（不取旋转）
        var fatherXYEvents = fatherEvents
            .Where(e => e.Type == LineEventType.X || e.Type == LineEventType.Y)
            .ToList();

        if (fatherXYEvents.Count == 0)
            return mergedLine;

        // 合并事件
        var childEvents = new List<LineEvent>();
        foreach (var layer in mergedLine.EventLayers)
        {
            childEvents.AddRange(EventBuilder.ConvertEventLayer(layer));
        }

        // 合并父线事件
        childEvents.AddRange(fatherXYEvents);

        // 重建事件层
        mergedLine.EventLayers.Clear();
        mergedLine.EventLayers.Add(EventBuilder.ConvertEvents(childEvents));

        return mergedLine;
    }

    /// <summary>
    /// 更新父子关系索引（解绑后需要重新映射）。
    /// </summary>
    private static void UpdateFatherIndices(List<Kpc.JudgeLine> lines)
    {
        // 简单实现：保持原有索引
        // 更复杂的实现需要重新映射索引
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

        // 合并所有事件层为一个事件列表（phichain 不支持多层级）
        var allEvents = new List<LineEvent>();
        foreach (var layer in src.EventLayers)
        {
            allEvents.AddRange(EventBuilder.ConvertEventLayer(layer));
        }

        line.Events = allEvents;

        return line;
    }
}
