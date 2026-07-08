namespace KaedePhi.Samples;

/// <summary>
/// 示例用最小谱面数据，运行 Sample 无需准备外部文件。
/// </summary>
internal static class SampleChartData
{
    internal const string MinimalRePhiEditJson =
        """
        {
            "BPMList": [{"bpm": 120, "startTime": [0, 0, 1]}],
            "META": {
                "name": "KaedePhi Sample",
                "composer": "Sample Composer",
                "charter": "Sample Charter",
                "level": "HD"
            },
            "judgeLineList": [{
                "Group": 0,
                "Name": "",
                "Texture": "line.png",
                "isCover": 1,
                "eventLayers": [{
                    "moveXEvents": [
                        {"startTime": [0, 0, 1], "endTime": [0, 1, 1], "start": 0.0, "end": 1.0, "easing": 1},
                        {"startTime": [0, 1, 1], "endTime": [0, 2, 1], "start": 1.0, "end": 0.5, "easing": 1}
                    ],
                    "moveYEvents": [],
                    "rotateEvents": [],
                    "alphaEvents": [],
                    "speedEvents": []
                }],
                "father": -1,
                "zOrder": 0
            }],
            "chartTime": 60,
            "judgeLineGroup": ["Default"],
            "multiLineString": "1",
            "multiScale": 1.0,
            "timeTags": [],
            "xybind": true
        }
        """;
}
