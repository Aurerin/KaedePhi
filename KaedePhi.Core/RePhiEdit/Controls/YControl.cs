using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit.Controls
{
    public class YControl : ControlBase
    {
        [JsonProperty("y")]
        public float Y { get; set; } = 1.0f;

        private static readonly List<YControl> DefaultInstance = new()
        {
            new()
            {
                Easing = new Easing(1),
                Y = 1.0f,
                X = 0.0f,
            },
            new()
            {
                Easing = new Easing(1),
                Y = 1.0f,
                X = 9999999.0f,
            },
        };

        [JsonIgnore]
        public static List<YControl> Default =>
            DefaultInstance.ConvertAll(input => input.Clone() as YControl);

        public override ControlBase Clone()
        {
            return new YControl
            {
                Easing = new Easing(Easing),
                X = X,
                Y = Y,
            };
        }
    }
}
