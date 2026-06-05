using KaedePhi.Core.Common;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit
{
    public class BeatTag
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 标签所在拍
        /// </summary>
        [JsonProperty("time")]
        public Beat Time { get; set; }
    }
}
