using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using KaedePhi.Core.Common;

namespace KaedePhi.Core.PhiEdit
{
    public partial class Chart
    {
        /// <summary>
        /// 将 PhiEditChart 格式的文本字符串反序列化为 <see cref="Chart"/> 对象。
        /// <para>
        /// 第一行必须为整数偏移量；随后每行为一条指令（<c>bp</c>、判定线指令或 Note 指令）。
        /// Note 指令若未内联速度/宽度信息，则紧跟的两行分别为速度行（<c># value</c>）和宽度行（<c>&amp; value</c>）。
        /// 解析完成后所有集合按拍数升序排序。
        /// </para>
        /// </summary>
        /// <param name="pec">符合 PhiEditChart 规范的文本字符串。</param>
        /// <returns>已完整反序列化并排序的 <see cref="Chart"/> 实例。</returns>
        /// <exception cref="FormatException">首行不是合法整数偏移量，或任意指令字段数不足。</exception>
        [PublicAPI]
        public static Chart Load(string pec)
        {
            var lines = pec.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            if (!int.TryParse(lines[0], out var offset))
                throw new FormatException(
                    "Malformed chart file: first line is not a valid integer offset."
                );

            var chart = new Chart { Offset = offset };
            var judgeDict = new Dictionary<int, JudgeLine>();

            var lineIndex = 1;
            while (lineIndex < lines.Length)
                lineIndex = ProcessLine(lines, lineIndex, chart, judgeDict);

            SortAndBuild(chart, judgeDict);
            return chart;
        }

        /// <summary>
        /// 处理 <paramref name="lines"/> 中索引为 <paramref name="i"/> 的单行，并返回下一行的索引。
        /// <para>
        /// 空白行直接跳过（返回 <c>i + 1</c>）；其余行交由
        /// <see cref="ParseChartLineCore(string,string[],int,Chart,Dictionary{int,JudgeLine})"/> 处理。
        /// </para>
        /// </summary>
        /// <param name="lines">谱面全部文本行数组。</param>
        /// <param name="i">当前待处理行的索引（从 1 开始，第 0 行已作为偏移量消耗）。</param>
        /// <param name="chart">正在构建的谱面对象，BPM 列表等数据将就地写入。</param>
        /// <param name="judgeDict">判定线暂存字典，键为判定线索引，值为对应 <see cref="JudgeLine"/>。</param>
        /// <returns>下一次应处理的行索引。</returns>
        /// <exception cref="FormatException">指令字段数不足或格式不合法。</exception>
        private static int ProcessLine(
            string[] lines,
            int i,
            Chart chart,
            Dictionary<int, JudgeLine> judgeDict
        )
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                return i + 1;

            return i + ParseChartLineCore(line, lines, i, chart, judgeDict);
        }

        /// <summary>
        /// 从 <paramref name="stream"/> 流式读取 PhiEditChart 并反序列化为 <see cref="Chart"/> 对象。
        /// <para>
        /// 使用 <see cref="StreamReader"/> 逐行读取，内存占用低于 <see cref="Load"/>；
        /// 流读取完毕后不会关闭 <paramref name="stream"/>（<c>leaveOpen: true</c>）。
        /// 解析完成后所有集合按拍数升序排序。
        /// </para>
        /// </summary>
        /// <param name="stream">可读的 PhiEditChart 文件流；调用方负责其生命周期管理。</param>
        /// <returns>已完整反序列化并排序的 <see cref="Chart"/> 实例。</returns>
        /// <exception cref="FormatException">首行不是合法整数偏移量，或任意指令字段数不足。</exception>
        [PublicAPI]
        public static Chart LoadStream(Stream stream)
        {
            using var reader = CreateStreamReader(stream);
            var (chart, judgeDict) = InitializeChart(reader.ReadLine);

            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    ParseChartLineCore(line, reader.ReadLine, chart, judgeDict);
            }

            SortAndBuild(chart, judgeDict);
            return chart;
        }

        /// <summary>
        /// 异步从 <paramref name="stream"/> 流式读取 PhiEditChart 并反序列化为 <see cref="Chart"/> 对象。
        /// <para>
        /// 使用 <see cref="StreamReader"/> 异步逐行读取；
        /// 流读取完毕后不会关闭 <paramref name="stream"/>（<c>leaveOpen: true</c>）。
        /// 解析完成后所有集合按拍数升序排序。
        /// </para>
        /// </summary>
        /// <param name="stream">可读的 PhiEditChart 文件流；调用方负责其生命周期管理。</param>
        /// <returns>已完整反序列化并排序的 <see cref="Chart"/> 实例。</returns>
        /// <exception cref="FormatException">首行不是合法整数偏移量，或任意指令字段数不足。</exception>
        [PublicAPI]
        public static async Task<Chart> LoadStreamAsync(Stream stream)
        {
            using var reader = CreateStreamReader(stream);
            var firstLine = await reader.ReadLineAsync();
            var (chart, judgeDict) = InitializeChart(() => firstLine);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    await ParseChartLineAsync(line, reader, chart, judgeDict);
            }

            SortAndBuild(chart, judgeDict);
            return chart;
        }

        private static async Task ParseChartLineAsync(
            string line,
            StreamReader reader,
            Chart chart,
            Dictionary<int, JudgeLine> judgeDict
        )
        {
            var part = line.Split(' ');
            var judgeLineIndex = part[0] != "bp" && part.Length > 1 ? int.Parse(part[1]) : -1;

            if (part[0] == "bp")
            {
                EnsureMinParts(part, 3, "bp");
                chart.BpmList.Add(
                    new BpmItem { StartBeat = float.Parse(part[1]), Bpm = float.Parse(part[2]) }
                );
            }
            else if (line.StartsWith('n'))
            {
                var (speedPart, widthPart) = GetInlineNoteParts(part);
                if (speedPart is null)
                {
                    speedPart = (await reader.ReadLineAsync())?.Split(' ');
                    widthPart = (await reader.ReadLineAsync())?.Split(' ');
                    if (speedPart == null || widthPart == null)
                        throw new FormatException("Malformed note: missing speed or width lines.");
                }

                AddNoteToDict(BuildNote(part, speedPart, widthPart), judgeLineIndex, judgeDict);
            }
            else
                ParseLineCommand(part, judgeLineIndex, judgeDict);
        }

        private static StreamReader CreateStreamReader(Stream stream) =>
            new(
                stream,
                JsonDefaults.NoBomUtf8,
                detectEncodingFromByteOrderMarks: true,
                1024,
                leaveOpen: true
            );

        private static (Chart chart, Dictionary<int, JudgeLine> judgeDict) InitializeChart(
            Func<string?> readFirstLineFunc
        )
        {
            var chart = new Chart();
            var judgeDict = new Dictionary<int, JudgeLine>();

            var firstLine = readFirstLineFunc();
            if (!int.TryParse(firstLine, out var offset))
                throw new FormatException(
                    "Malformed chart file: first line is not a valid integer offset."
                );
            chart.Offset = offset;

            return (chart, judgeDict);
        }

        /// <summary>
        /// 解析谱面中的一行文本，将结果写入 <paramref name="chart"/> 或 <paramref name="judgeDict"/>。
        /// <para>
        /// 若当前行为 Note 指令且未内联速度/宽度信息，则通过 <paramref name="readNextLineFunc"/> 额外读取紧跟的两行。
        /// </para>
        /// </summary>
        /// <param name="line">当前非空白文本行。</param>
        /// <param name="readNextLineFunc">用于按需读取后续行（Note 多行格式）的函数。</param>
        /// <param name="chart">正在构建的谱面对象。</param>
        /// <param name="judgeDict">判定线暂存字典。</param>
        /// <exception cref="FormatException">指令字段数不足，或 Note 缺失速度/宽度行。</exception>
        private static void ParseChartLineCore(
            string line,
            Func<string?> readNextLineFunc,
            Chart chart,
            Dictionary<int, JudgeLine> judgeDict
        )
        {
            var part = line.Split(' ');
            var judgeLineIndex = part[0] != "bp" && part.Length > 1 ? int.Parse(part[1]) : -1;

            if (part[0] == "bp")
            {
                EnsureMinParts(part, 3, "bp");
                chart.BpmList.Add(
                    new BpmItem { StartBeat = float.Parse(part[1]), Bpm = float.Parse(part[2]) }
                );
            }
            else if (line.StartsWith('n'))
            {
                var (speedPart, widthPart) = GetInlineNoteParts(part);
                if (speedPart is null)
                {
                    speedPart = readNextLineFunc()?.Split(' ');
                    widthPart = readNextLineFunc()?.Split(' ');
                    if (speedPart == null || widthPart == null)
                        throw new FormatException("Malformed note: missing speed or width lines.");
                }

                AddNoteToDict(BuildNote(part, speedPart, widthPart), judgeLineIndex, judgeDict);
            }
            else
                ParseLineCommand(part, judgeLineIndex, judgeDict);
        }

        /// <summary>
        /// 解析谱面中的一行文本，将结果写入 <paramref name="chart"/> 或 <paramref name="judgeDict"/>。
        /// <para>
        /// 若当前行为 Note 指令且未内联速度/宽度信息，则从 <paramref name="lines"/> 额外读取紧跟的两行。
        /// </para>
        /// </summary>
        /// <param name="line">当前非空白文本行。</param>
        /// <param name="lines">谱面全部文本行数组。</param>
        /// <param name="index">当前行在 <paramref name="lines"/> 中的索引。</param>
        /// <param name="chart">正在构建的谱面对象。</param>
        /// <param name="judgeDict">判定线暂存字典。</param>
        /// <returns>消耗的行数（1 为仅当前行，3 为包含后续两行）。</returns>
        /// <exception cref="FormatException">指令字段数不足，或 Note 缺失速度/宽度行。</exception>
        private static int ParseChartLineCore(
            string line,
            string[] lines,
            int index,
            Chart chart,
            Dictionary<int, JudgeLine> judgeDict
        )
        {
            var part = line.Split(' ');
            var judgeLineIndex = part[0] != "bp" && part.Length > 1 ? int.Parse(part[1]) : -1;

            if (part[0] == "bp")
            {
                EnsureMinParts(part, 3, "bp");
                chart.BpmList.Add(
                    new BpmItem { StartBeat = float.Parse(part[1]), Bpm = float.Parse(part[2]) }
                );
            }
            else if (line.StartsWith('n'))
            {
                var (speedPart, widthPart) = GetInlineNoteParts(part);
                if (speedPart is null)
                {
                    if (index + 2 >= lines.Length)
                        throw new FormatException(
                            $"Malformed note at line {index + 1}: missing speed or width lines."
                        );
                    speedPart = lines[index + 1].Split(' ');
                    widthPart = lines[index + 2].Split(' ');
                    AddNoteToDict(BuildNote(part, speedPart, widthPart), judgeLineIndex, judgeDict);
                    return 3;
                }

                AddNoteToDict(BuildNote(part, speedPart, widthPart), judgeLineIndex, judgeDict);
            }
            else
                ParseLineCommand(part, judgeLineIndex, judgeDict);

            return 1;
        }

        /// <summary>
        /// 校验指令的字段数量是否满足最低要求；不满足时抛出包含命令名称和实际/期望字段数的 <see cref="FormatException"/>。
        /// </summary>
        /// <param name="part">已按空格拆分的指令字段数组。</param>
        /// <param name="min">该指令要求的最小字段数（含指令标识符本身）。</param>
        /// <param name="cmd">指令名称，用于生成错误消息（如 <c>"bp"</c>、<c>"cm"</c>）。</param>
        /// <exception cref="FormatException"><paramref name="part"/> 的长度小于 <paramref name="min"/>。</exception>
        private static void EnsureMinParts(string[] part, int min, string cmd)
        {
            if (part.Length < min)
                throw new FormatException(
                    $"Malformed '{cmd}' command: expected at least {min} parts, got {part.Length}."
                );
        }

        /// <summary>
        /// 根据指令类型（<c>cv</c>/<c>cp</c>/<c>cd</c>/<c>ca</c>/<c>cm</c>/<c>cr</c>/<c>cf</c>）
        /// 解析对应的关键帧或事件，追加到 <paramref name="judgeDict"/> 中对应判定线的集合内。
        /// <para>
        /// 未知指令类型将被静默忽略。若指令所对应的判定线尚不存在，会自动创建并注册。
        /// </para>
        /// </summary>
        /// <param name="part">已按空格拆分的指令字段数组，<c>part[0]</c> 为指令标识符，<c>part[1]</c> 为判定线索引。</param>
        /// <param name="judgeLineIndex">当前指令作用的判定线索引。</param>
        /// <param name="judgeDict">判定线暂存字典，解析结果将就地写入。</param>
        /// <exception cref="FormatException">指令字段数不足。</exception>
        private static void ParseLineCommand(
            string[] part,
            int judgeLineIndex,
            Dictionary<int, JudgeLine> judgeDict
        )
        {
            switch (part[0])
            {
                case "cv":
                    EnsureMinParts(part, 4, "cv");
                    Ensure();
                    judgeDict[judgeLineIndex]
                        .SpeedFrames.Add(
                            new Frame { Beat = float.Parse(part[2]), Value = float.Parse(part[3]) }
                        );
                    break;
                case "cp":
                    EnsureMinParts(part, 5, "cp");
                    Ensure();
                    judgeDict[judgeLineIndex]
                        .MoveFrames.Add(
                            new MoveFrame
                            {
                                Beat = float.Parse(part[2]),
                                XValue = float.Parse(part[3]),
                                YValue = float.Parse(part[4]),
                            }
                        );
                    break;
                case "cd":
                    EnsureMinParts(part, 4, "cd");
                    Ensure();
                    judgeDict[judgeLineIndex]
                        .RotateFrames.Add(
                            new Frame { Beat = float.Parse(part[2]), Value = float.Parse(part[3]) }
                        );
                    break;
                case "ca":
                    EnsureMinParts(part, 4, "ca");
                    Ensure();
                    judgeDict[judgeLineIndex]
                        .AlphaFrames.Add(
                            new Frame { Beat = float.Parse(part[2]), Value = float.Parse(part[3]) }
                        );
                    break;
                case "cm":
                    EnsureMinParts(part, 7, "cm");
                    Ensure();
                    judgeDict[judgeLineIndex]
                        .MoveEvents.Add(
                            new MoveEvent
                            {
                                StartBeat = float.Parse(part[2]),
                                EndBeat = float.Parse(part[3]),
                                EndXValue = float.Parse(part[4]),
                                EndYValue = float.Parse(part[5]),
                                EasingType = new Easing(int.Parse(part[6])),
                            }
                        );
                    break;
                case "cr":
                    EnsureMinParts(part, 6, "cr");
                    Ensure();
                    judgeDict[judgeLineIndex]
                        .RotateEvents.Add(
                            new Event
                            {
                                StartBeat = float.Parse(part[2]),
                                EndBeat = float.Parse(part[3]),
                                EndValue = float.Parse(part[4]),
                                EasingType = new Easing(int.Parse(part[5])),
                            }
                        );
                    break;
                case "cf":
                    EnsureMinParts(part, 5, "cf");
                    Ensure();
                    judgeDict[judgeLineIndex]
                        .AlphaEvents.Add(
                            new Event
                            {
                                StartBeat = float.Parse(part[2]),
                                EndBeat = float.Parse(part[3]),
                                EndValue = float.Parse(part[4]),
                                EasingType = Easing.Linear,
                            }
                        );
                    break;
            }

            return;

            void Ensure()
            {
                if (!judgeDict.ContainsKey(judgeLineIndex))
                    judgeDict[judgeLineIndex] = new JudgeLine();
            }
        }

        /// <summary>
        /// 根据已拆分的字段数组构造一个 <see cref="Note"/> 对象。
        /// <para>
        /// <c>part[0]</c> 的第二个字符决定音符类型；Hold 音符（类型 3）会从 <c>part</c> 读取结束拍，
        /// 其余音符的结束拍等于起始拍。速度倍率和宽度比例分别从
        /// <paramref name="noteSpeedMultiplierPart"/>[1] 和 <paramref name="noteWidthRatioPart"/>[1] 读取。
        /// </para>
        /// </summary>
        /// <param name="part">Note 主指令字段数组（至少 4 个元素）。</param>
        /// <param name="noteSpeedMultiplierPart">速度行字段数组，格式为 <c>["#", value]</c>（至少 2 个元素）。</param>
        /// <param name="noteWidthRatioPart">宽度行字段数组，格式为 <c>["&amp;", value]</c>（至少 2 个元素）。</param>
        /// <returns>完整填充的 <see cref="Note"/> 实例。</returns>
        /// <exception cref="FormatException">任意字段数组元素数量不足。</exception>
        private static Note BuildNote(
            string[] part,
            string[] noteSpeedMultiplierPart,
            string[]? noteWidthRatioPart
        )
        {
            if (noteWidthRatioPart is null)
                throw new FormatException("Malformed note: missing width ratio part.");
            if (part.Length < 4)
                throw new FormatException(
                    $"Malformed note command: expected at least 4 parts, got {part.Length}."
                );
            if (noteSpeedMultiplierPart.Length < 2)
                throw new FormatException(
                    "Malformed note speed multiplier part: expected at least 2 elements."
                );
            if (noteWidthRatioPart.Length < 2)
                throw new FormatException(
                    "Malformed note width ratio part: expected at least 2 elements."
                );

            var noteType = (NoteType)int.Parse(part[0].Substring(1, 1));
            var isHold = noteType == NoteType.Hold;
            return new Note
            {
                StartBeat = float.Parse(part[2]),
                EndBeat = isHold ? float.Parse(part[3]) : float.Parse(part[2]),
                PositionX = float.Parse(part[isHold ? 4 : 3]),
                Above = part[isHold ? 5 : 4] == "1",
                IsFake = part[isHold ? 6 : 5] == "1",
                SpeedMultiplier = float.Parse(noteSpeedMultiplierPart[1]),
                WidthRatio = float.Parse(noteWidthRatioPart[1]),
                Type = noteType,
            };
        }

        /// <summary>
        /// 从 Note 主指令字段数组中尝试提取内联的速度行和宽度行。
        /// <para>
        /// 部分不规范谱面允许将速度（<c># value</c>）和宽度（<c>&amp; value</c>）以空格连接内联在同一行中；
        /// 本方法通过查找 <c>#</c> 和 <c>&amp;</c> 标记判断是否为内联格式。
        /// </para>
        /// </summary>
        /// <param name="part">Note 行按空格拆分后的全部字段。</param>
        /// <returns>
        /// 若找到内联的速度和宽度信息，返回对应的两个字段数组元组 <c>(speedPart, widthPart)</c>；
        /// 否则两者均为 <see langword="null"/>，表示需要额外读取后续两行。
        /// </returns>
        private static (string[]? speedPart, string[]? widthPart) GetInlineNoteParts(string[] part)
        {
            var hashIndex = Array.IndexOf(part, "#");
            var ampIndex = Array.IndexOf(part, "&");
            if (
                hashIndex != -1
                && ampIndex != -1
                && hashIndex + 1 < part.Length
                && ampIndex + 1 < part.Length
            )
                return (new[] { "#", part[hashIndex + 1] }, new[] { "&", part[ampIndex + 1] });
            return (null, null);
        }

        /// <summary>
        /// 将 <paramref name="note"/> 追加到 <paramref name="judgeDict"/> 中对应判定线的 <see cref="JudgeLine.NoteList"/>。
        /// 若指定索引的判定线尚不存在，则自动创建并注册。
        /// </summary>
        /// <param name="note">待追加的音符。</param>
        /// <param name="judgeLineIndex">音符所属判定线的索引。</param>
        /// <param name="judgeDict">判定线暂存字典。</param>
        private static void AddNoteToDict(
            Note note,
            int judgeLineIndex,
            Dictionary<int, JudgeLine> judgeDict
        )
        {
            if (!judgeDict.ContainsKey(judgeLineIndex))
                judgeDict[judgeLineIndex] = new JudgeLine();
            judgeDict[judgeLineIndex].NoteList.Add(note);
        }

        /// <summary>
        /// 对 <paramref name="chart"/> 和 <paramref name="judgeDict"/> 执行最终的排序与组装。
        /// <para>
        /// BPM 列表按起始拍升序排序；每条判定线的关键帧列表、事件列表和音符列表分别按拍数/起始拍升序排序；
        /// 最后将 <paramref name="judgeDict"/> 按判定线索引升序转换为 <see cref="Chart.JudgeLineList"/>。
        /// </para>
        /// </summary>
        /// <param name="chart">待完善的谱面对象，<see cref="Chart.JudgeLineList"/> 将在此方法中赋值。</param>
        /// <param name="judgeDict">解析阶段积累的判定线暂存字典。</param>
        private static void SortAndBuild(Chart chart, Dictionary<int, JudgeLine> judgeDict)
        {
            chart.BpmList = chart.BpmList.OrderBy(b => b.StartBeat).ToList();
            foreach (var judgeLine in judgeDict.Values)
            {
                // 排序
                // Frame
                judgeLine.SpeedFrames = judgeLine.SpeedFrames.OrderBy(f => f.Beat).ToList();
                judgeLine.MoveFrames = judgeLine.MoveFrames.OrderBy(f => f.Beat).ToList();
                judgeLine.RotateFrames = judgeLine.RotateFrames.OrderBy(f => f.Beat).ToList();
                judgeLine.AlphaFrames = judgeLine.AlphaFrames.OrderBy(f => f.Beat).ToList();
                // Event
                judgeLine.MoveEvents = judgeLine.MoveEvents.OrderBy(e => e.StartBeat).ToList();
                judgeLine.RotateEvents = judgeLine.RotateEvents.OrderBy(e => e.StartBeat).ToList();
                judgeLine.AlphaEvents = judgeLine.AlphaEvents.OrderBy(e => e.StartBeat).ToList();
                // Note
                judgeLine.NoteList = judgeLine.NoteList.OrderBy(n => n.StartBeat).ToList();
            }

            chart.JudgeLineList = judgeDict.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
        }

        /// <summary>
        /// 将 PhiEditChart 格式的文本字符串异步反序列化为 <see cref="Chart"/> 对象。
        /// <para>内部在线程池上调用同步的 <see cref="Load"/>，适合在 UI 线程等不宜阻塞的上下文中使用。</para>
        /// </summary>
        /// <param name="pec">符合 PhiEditChart 规范的文本字符串。</param>
        /// <returns>已完整反序列化并排序的 <see cref="Chart"/> 实例。</returns>
        /// <exception cref="FormatException">首行不是合法整数偏移量，或任意指令字段数不足。</exception>
        public static async Task<Chart> LoadAsync(string pec) => await Task.Run(() => Load(pec));

        /// <summary>
        /// 以惰性迭代方式枚举单条判定线 <paramref name="judgeLine"/> 的所有 PhiEditChart 导出行。
        /// <para>
        /// 输出顺序为：移动关键帧 → 速度关键帧 → 旋转关键帧 → 不透明度关键帧 →
        /// 移动事件 → 旋转事件 → 不透明度事件 → 音符。
        /// </para>
        /// </summary>
        /// <param name="judgeLine">待导出的判定线。</param>
        /// <param name="index">该判定线在谱面中的索引，用于生成指令中的判定线编号字段。</param>
        /// <returns>按 PhiEditChart 规范格式化的文本行序列。</returns>
        private static IEnumerable<string> GetJudgeLineLines(JudgeLine judgeLine, int index)
        {
            // Frame
            foreach (var frame in judgeLine.MoveFrames)
                yield return frame.ToString(index);
            foreach (var frame in judgeLine.SpeedFrames)
                yield return frame.ToString(index, "cv");
            foreach (var frame in judgeLine.RotateFrames)
                yield return frame.ToString(index, "cd");
            foreach (var frame in judgeLine.AlphaFrames)
                yield return frame.ToString(index, "ca");
            // Event
            foreach (var ev in judgeLine.MoveEvents)
                yield return ev.ToString(index);
            foreach (var ev in judgeLine.RotateEvents)
                yield return ev.ToString(index, "cr");
            foreach (var ev in judgeLine.AlphaEvents)
                yield return ev.ToString(index, "cf");
            // Note
            foreach (var note in judgeLine.NoteList)
                yield return note.ToString(index);
        }

        /// <summary>
        /// 以惰性迭代方式枚举整个谱面的所有 PhiEditChart 导出行。
        /// <para>输出顺序为：偏移量行 → BPM 行 → 各判定线的全部指令行（调用 <see cref="GetJudgeLineLines"/>）。</para>
        /// </summary>
        /// <returns>按 PhiEditChart 规范格式化的完整文本行序列。</returns>
        private IEnumerable<string> GetExportLines()
        {
            yield return Offset.ToString();
            foreach (var bpm in BpmList)
                yield return bpm.ToString();
            for (var i = 0; i < JudgeLineList.Count; i++)
                foreach (var line in GetJudgeLineLines(JudgeLineList[i], i))
                    yield return line;
        }

        /// <summary>
        /// 将谱面序列化为 PhiEditChart 格式的文本字符串，各行以 <see cref="Environment.NewLine"/> 连接。
        /// </summary>
        /// <returns>完整的 PhiEditChart 文本。</returns>
        [PublicAPI]
        public string Export() => string.Join(Environment.NewLine, GetExportLines());

        /// <summary>
        /// 将谱面异步序列化为 PhiEditChart 格式的文本字符串。
        /// <para>内部在线程池上调用同步的 <see cref="Export"/>，适合在 UI 线程等不宜阻塞的上下文中使用。</para>
        /// </summary>
        /// <returns>完整的 PhiEditChart 文本。</returns>
        public async Task<string> ExportAsync() => await Task.Run(Export);

        /// <summary>
        /// 将谱面以 PhiEditChart 格式流式写入 <paramref name="stream"/>，每行结尾使用系统换行符。
        /// <para>写入完毕后不会关闭 <paramref name="stream"/>（<c>leaveOpen: true</c>），调用方负责其生命周期管理。</para>
        /// </summary>
        /// <param name="stream">可写的目标流。</param>
        public void ExportToStream(Stream stream)
        {
            using var writer = CreateStreamWriter(stream);
            WriteExportLines(writer.WriteLine);
        }

        /// <summary>
        /// 将谱面以 PhiEditChart 格式异步流式写入 <paramref name="stream"/>，每行结尾使用系统换行符。
        /// <para>写入完毕后不会关闭 <paramref name="stream"/>（<c>leaveOpen: true</c>），调用方负责其生命周期管理。</para>
        /// </summary>
        /// <param name="stream">可写的目标流。</param>
        public async Task ExportToStreamAsync(Stream stream)
        {
            await using var writer = CreateStreamWriter(stream);
            foreach (var line in GetExportLines())
                await writer.WriteLineAsync(line);
        }

        private static StreamWriter CreateStreamWriter(Stream stream) =>
            new(stream, JsonDefaults.NoBomUtf8, 1024, leaveOpen: true);

        private void WriteExportLines(Action<string> writeLineFunc)
        {
            foreach (var line in GetExportLines())
                writeLineFunc(line);
        }
    }
}
