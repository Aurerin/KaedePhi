using KaedePhi.Core.Common;
using KaedePhi.Core.KaedePhi;
using KaedePhi.Tool.Event.KaedePhi;
using KpcEvents = KaedePhi.Core.KaedePhi.Events;

namespace KaedePhi.Tests.Event;

/// <summary>
/// 验证 EventListMergerPlus 在自适应采样路径中正确处理同起始拍事件（后来者至上）。
/// 修复前：MaxBy 返回同 StartBeat 中第一个事件（index 最小），导致采样值错误。
/// 修复后：LastOrDefault 返回同 StartBeat 中最后一个事件（index 最大），符合重叠规则。
/// </summary>
public class EventListMergerPlusOverlapTests
{
    private readonly EventListMergerPlus<double> _merger = new();

    [Fact]
    public void Merge_SameStartBeat_LaterIndexWins_InAdaptiveSampling()
    {
        // toEvents: [A: 0-10, value 0→100]
        // fromEvents: [B: 0-5, value 0→200] (same StartBeat as A, but higher index in merged context)
        //
        // The merger adds two tracks. In the overlap region [0, 5]:
        // - toEvents has A active (0→100 over [0,10])
        // - fromEvents has B active (0→200 over [0,5])
        // With the fix, GetActiveEventAtBeat correctly picks the last event with the same StartBeat.
        var toEvents = new List<KpcEvents.Event<double>> { CreateEvent(0, 10, 0, 100) };
        var fromEvents = new List<KpcEvents.Event<double>> { CreateEvent(0, 5, 0, 200) };

        var result = _merger.EventListMerge(toEvents, fromEvents, 4, 1.0);

        result.Should().NotBeEmpty();

        // At beat 0: A(0) + B(0) = 0 + 0 = 0
        var valueAt0 = QueryResult(result, Beat(0));
        valueAt0.Should().BeApproximately(0.0, 1e-6);

        // At beat 2.5: A(25) + B(100) = 125
        var valueAt2_5 = QueryResult(result, Beat(2.5));
        valueAt2_5.Should().BeApproximately(125.0, 1.0); // tolerance for sampling

        // At beat 5: A(50) + B(200) = 250
        var valueAt5 = QueryResult(result, Beat(5));
        valueAt5.Should().BeApproximately(250.0, 1.0);
    }

    [Fact]
    public void Merge_NoOverlap_SimpleAddition()
    {
        var toEvents = new List<KpcEvents.Event<double>> { CreateEvent(0, 5, 0, 100) };
        var fromEvents = new List<KpcEvents.Event<double>> { CreateEvent(5, 10, 50, 150) };

        var result = _merger.EventListMerge(toEvents, fromEvents, 4, 1.0);

        result.Should().NotBeEmpty();

        // toEvents [0,5]: value 0→100, offset from prevFrom = 0 → [0, 100]
        // fromEvents [5,10]: value 50→150, offset from prevTo = 100 → [150, 250]
        var valueAt2_5 = QueryResult(result, Beat(2.5));
        valueAt2_5.Should().BeApproximately(50.0, 1.0);

        var valueAt7_5 = QueryResult(result, Beat(7.5));
        valueAt7_5.Should().BeApproximately(200.0, 1.0);
    }

    [Fact]
    public void Merge_PartialOverlap_LaterTruncatesCorrectly()
    {
        // toEvents: [A: 0-5, 0→100]
        // fromEvents: [B: 3-8, 0→200]
        // Overlap: [3, 5]
        var toEvents = new List<KpcEvents.Event<double>> { CreateEvent(0, 5, 0, 100) };
        var fromEvents = new List<KpcEvents.Event<double>> { CreateEvent(3, 8, 0, 200) };

        var result = _merger.EventListMerge(toEvents, fromEvents, 4, 1.0);

        result.Should().NotBeEmpty();

        // Before overlap (beat 1): A only, offset 0 → ~20
        var valueAt1 = QueryResult(result, Beat(1));
        valueAt1.Should().BeApproximately(20.0, 1.0);

        // In overlap (beat 4): A(80) + B(40) = 120
        var valueAt4 = QueryResult(result, Beat(4));
        valueAt4.Should().BeApproximately(120.0, 2.0);

        // After A ends (beat 6): B only, offset from A.EndValue=100 → B(120)+100 = 220
        var valueAt6 = QueryResult(result, Beat(6));
        valueAt6.Should().BeApproximately(220.0, 2.0);
    }

    #region Helper Methods

    private static KpcEvents.Event<double> CreateEvent(
        double startBeat,
        double endBeat,
        double startValue,
        double endValue
    )
    {
        return new KpcEvents.Event<double>
        {
            StartBeat = new Beat(startBeat),
            EndBeat = new Beat(endBeat),
            StartValue = startValue,
            EndValue = endValue,
            Easing = Easing.Linear,
        };
    }

    private static Beat Beat(double value) => new(value);

    /// <summary>
    /// Query the merged result at the given beat using EventLayer.GetValueAtBeat.
    /// </summary>
    private static double QueryResult(List<KpcEvents.Event<double>> events, Beat beat)
    {
        return KpcEvents.EventLayer.GetValueAtBeat(events, beat);
    }

    #endregion
}
