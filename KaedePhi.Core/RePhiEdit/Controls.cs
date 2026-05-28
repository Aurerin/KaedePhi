using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit
{
    public abstract class ControlBase
    {
        [JsonProperty("easing")] public Easing Easing { get; set; } = new(1);

        [JsonProperty("x")] public float X { get; set; }

        public abstract ControlBase Clone();
    }

    public class AlphaControl : ControlBase
    {
        [JsonProperty("alpha")] public float Alpha { get; set; } = 1.0f;

        private static readonly List<AlphaControl> DefaultInstance = new()
        {
            new() { Easing = new Easing(1), Alpha = 1.0f, X = 0.0f },
            new() { Easing = new Easing(1), Alpha = 1.0f, X = 9999999.0f }
        };

        [JsonIgnore]
        public static List<AlphaControl> Default
            => DefaultInstance.ConvertAll(input => input.Clone() as AlphaControl);

        public override ControlBase Clone()
        {
            return new AlphaControl { Easing = new Easing(Easing), X = X, Alpha = Alpha };
        }
    }

    public class XControl : ControlBase
    {
        [JsonProperty("pos")] public float Pos { get; set; } = 1.0f;

        private static readonly List<XControl> DefaultInstance = new()
        {
            new() { Easing = new Easing(1), Pos = 1.0f, X = 0.0f },
            new() { Easing = new Easing(1), Pos = 1.0f, X = 9999999.0f }
        };

        [JsonIgnore]
        public static List<XControl> Default
            => DefaultInstance.ConvertAll(input => input.Clone() as XControl);

        public override ControlBase Clone()
        {
            return new XControl { Easing = new Easing(Easing), X = X, Pos = Pos };
        }
    }

    public class SizeControl : ControlBase
    {
        [JsonProperty("size")] public float Size { get; set; } = 1.0f;

        private static readonly List<SizeControl> DefaultInstance = new()
        {
            new() { Easing = new Easing(1), Size = 1.0f, X = 0.0f },
            new() { Easing = new Easing(1), Size = 1.0f, X = 9999999.0f }
        };

        [JsonIgnore]
        public static List<SizeControl> Default
            => DefaultInstance.ConvertAll(input => input.Clone() as SizeControl);

        public override ControlBase Clone()
        {
            return new SizeControl { Easing = new Easing(Easing), X = X, Size = Size };
        }
    }

    public class SkewControl : ControlBase
    {
        [JsonProperty("skew")] public float Skew { get; set; } = 1.0f;

        private static readonly List<SkewControl> DefaultInstance = new()
        {
            new() { Easing = new Easing(1), Skew = 0.0f, X = 0.0f },
            new() { Easing = new Easing(1), Skew = 0.0f, X = 9999999.0f }
        };

        [JsonIgnore]
        public static List<SkewControl> Default
            => DefaultInstance.ConvertAll(input => input.Clone() as SkewControl);

        public override ControlBase Clone()
        {
            return new SkewControl { Easing = new Easing(Easing), X = X, Skew = Skew };
        }
    }

    public class YControl : ControlBase
    {
        [JsonProperty("y")] public float Y { get; set; } = 1.0f;

        private static readonly List<YControl> DefaultInstance = new()
        {
            new() { Easing = new Easing(1), Y = 1.0f, X = 0.0f },
            new() { Easing = new Easing(1), Y = 1.0f, X = 9999999.0f }
        };

        [JsonIgnore]
        public static List<YControl> Default
            => DefaultInstance.ConvertAll(input => input.Clone() as YControl);

        public override ControlBase Clone()
        {
            return new YControl { Easing = new Easing(Easing), X = X, Y = Y };
        }
    }
}