using System.Collections.Generic;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit.Controls
{
    public class AlphaControl : ControlBase
    {
        [JsonProperty("alpha")]
        public float Alpha { get; set; } = 1.0f;

        private static readonly List<AlphaControl> DefaultInstance = new()
        {
            new()
            {
                Easing = new Easing(1),
                Alpha = 1.0f,
                X = 0.0f,
            },
            new()
            {
                Easing = new Easing(1),
                Alpha = 1.0f,
                X = 9999999.0f,
            },
        };

        [JsonIgnore]
        public static List<AlphaControl> Default =>
            DefaultInstance.ConvertAll(input => input.Clone() as AlphaControl);

        public override ControlBase Clone()
        {
            return new AlphaControl
            {
                Easing = new Easing(Easing),
                X = X,
                Alpha = Alpha,
            };
        }
    }
}
