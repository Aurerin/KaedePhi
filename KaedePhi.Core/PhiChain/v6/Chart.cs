using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace KaedePhi.Core.PhiChain.v6
{
    public sealed partial class Chart
    {
        [JsonProperty("format")]
        public ulong Format { get; set; } = Constants.CurrentFormat;

        [JsonProperty("offset")]
        public float Offset { get; set; }

        [JsonProperty("bpm_list")]
        public BpmList BpmList { get; set; } = new();

        [JsonProperty("lines")]
        public List<SerializedLine> Lines { get; set; } = new() { SerializedLine.CreateDefault() };

        public static Chart Empty()
        {
            return new Chart { Lines = new List<SerializedLine>() };
        }

        /// <summary>
        /// 深克隆当前 Chart 对象
        /// </summary>
        public Chart Clone()
        {
            return new Chart
            {
                Format = Format,
                Offset = Offset,
                BpmList = BpmList.Clone(),
                Lines = Lines.Select(l => l.Clone()).ToList(),
            };
        }
    }
}
