using System.Collections.Generic;
using System.Linq;
using KaedePhi.Core.RePhiEdit.JsonConverter;
using Newtonsoft.Json;

namespace KaedePhi.Core.RePhiEdit
{
    public class JudgeLine
    {
        /// <summary>
        /// 判定线名称
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; } = "KaedePhi_RePhiEditJudgeLine";

        /// <summary>
        /// 判定线纹理相对路径，默认值为line.png
        /// </summary>
        [JsonProperty("Texture")]
        public string Texture { get; set; } = "line.png"; // 判定线纹理路径

        /// <summary>
        /// 判定线纹理锚点(0~1之间)，默认值为中心点(0.5, 0.5)
        /// </summary>
        [JsonProperty("anchor")]
        public float[] Anchor { get; set; } = { 0.5f, 0.5f }; // 判定线纹理锚点

        /// <summary>
        /// 判定线事件层列表
        /// </summary>
        [JsonProperty("eventLayers", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<Events.EventLayer> EventLayers { get; set; } = new(); // 事件层

        /// <summary>
        /// 父级判定线索引，-1表示无父级
        /// </summary>
        [JsonProperty("father")]
        public int Father { get; set; } = -1; // 父级

        /// <summary>
        /// 是否遮罩越过判定线的音符（已被打击的除外）
        /// </summary>
        [JsonProperty("isCover")]
        [JsonConverter(typeof(BoolConverter))]
        public bool IsCover { get; set; } = true; // 是否遮罩

        /// <summary>
        /// 判定线音符列表
        /// </summary>
        [JsonProperty("notes", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore)]
        public List<Note>? Notes
        {
            get => _notes;
            set => _notes = value ?? new List<Note>();
        } // note列表

        private List<Note>? _notes = new();

        /// <summary>
        /// Note总数量(包含 FakeNote，不包含Hold)。
        /// 为什么？RePhiEdit就是这样设计的。。。
        /// 用户绝对不要访问此值。
        /// </summary>
        [JsonProperty("numOfNotes")]
        private int TotalNumberOfNotes
        {
            get
            {
                // Note总数量(包含 FakeNote，不包含任何形式的Hold)
                return Notes?.Count(note => note.Type != NoteType.Hold) ?? 0;
            }
        }

        /// <summary>
        /// 特殊事件层（故事板）
        /// </summary>
        [JsonProperty("extended", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore)]
        public Events.ExtendLayer Extended { get; set; } = new Events.ExtendLayer();

        /// <summary>
        /// 判定线的Z轴顺序
        /// </summary>
        [JsonProperty("zOrder")]
        public int ZOrder { get; set; } // Z轴顺序

        /// <summary>
        /// 判定线是否绑定UI
        /// </summary>
        [JsonProperty("attachUI", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(AttachUiConverter))]
        public AttachUi? AttachUi { get; set; } // 绑定UI名，当不绑定时为null

        /// <summary>
        /// 判定线纹理是否为GIF
        /// </summary>
        [JsonProperty("isGif")]
        public bool IsGif { get; set; } // 纹理是否为GIF

        /// <summary>
        /// 所属组
        /// </summary>
        [JsonProperty("Group")]
        public int Group { get; set; } // 绑定组

        /// <summary>
        /// 当前判定线相对于当前BPM的因子。判定线BPM = 当前BPM / BpmFactor
        /// </summary>
        [JsonProperty("bpmfactor")]
        public float BpmFactor { get; set; } = 1.0f; // BPM因子

        /// <summary>
        /// 是否跟随父线旋转
        /// </summary>
        [JsonProperty("rotateWithFather")]
        public bool RotateWithFather { get; set; } // 是否随父级旋转

        /// <summary>
        /// Position（X） Control 控制点列表
        /// </summary>
        [JsonProperty("posControl")]
        public List<Controls.XControl> PositionControls
        {
            get
            {
                _positionControls ??= new List<Controls.XControl>();

                return _positionControls;
            }
            set => _positionControls = value;
        }

        [JsonIgnore] private List<Controls.XControl> _positionControls;

        /// <summary>
        /// Alpha Control 控制点列表
        /// </summary>
        [JsonProperty("alphaControl")]
        public List<Controls.AlphaControl> AlphaControls
        {
            get
            {
                _alphaControls ??= new List<Controls.AlphaControl>();

                return _alphaControls;
            }
            set => _alphaControls = value;
        }

        [JsonIgnore] private List<Controls.AlphaControl> _alphaControls;

        /// <summary>
        /// Size Control 控制点列表
        /// </summary>
        [JsonProperty("sizeControl")]
        public List<Controls.SizeControl> SizeControls
        {
            get
            {
                _sizeControls ??= new List<Controls.SizeControl>();

                return _sizeControls;
            }
            set => _sizeControls = value;
        }

        [JsonIgnore] private List<Controls.SizeControl> _sizeControls;

        /// <summary>
        /// Skew Control 控制点列表
        /// </summary>
        [JsonProperty("skewControl")]
        public List<Controls.SkewControl> SkewControls
        {
            get
            {
                _skewControls ??= new List<Controls.SkewControl>();

                return _skewControls;
            }
            set => _skewControls = value;
        }

        [JsonIgnore] private List<Controls.SkewControl> _skewControls;

        /// <summary>
        /// Y Control 控制点列表
        /// </summary>
        [JsonProperty("yControl")]
        public List<Controls.YControl> YControls
        {
            get
            {
                _yControls ??= new List<Controls.YControl>();

                return _yControls;
            }
            set => _yControls = value;
        }

        [JsonIgnore] private List<Controls.YControl> _yControls;

        public JudgeLine Clone()
        {
            var clone = new JudgeLine
            {
                Name = Name,
                Texture = Texture,
                Anchor = (float[])Anchor.Clone(),
                Father = Father,
                IsCover = IsCover,
                ZOrder = ZOrder,
                IsGif = IsGif,
                Group = Group,
                BpmFactor = BpmFactor,
                RotateWithFather = RotateWithFather,
                AttachUi = AttachUi,
                EventLayers = new List<Events.EventLayer>(),
                Notes = new List<Note>(),
                Extended = Extended?.Clone(),
                PositionControls = new List<Controls.XControl>(),
                AlphaControls = new List<Controls.AlphaControl>(),
                SizeControls = new List<Controls.SizeControl>(),
                SkewControls = new List<Controls.SkewControl>(),
                YControls = new List<Controls.YControl>()
            };

            // 深拷贝列表
            foreach (var eventLayer in EventLayers)
                clone.EventLayers.Add(eventLayer.Clone());
            foreach (var note in Notes)
                clone.Notes.Add(note.Clone());
            foreach (var control in PositionControls)
                clone.PositionControls.Add(control.Clone() as Controls.XControl);
            foreach (var control in AlphaControls)
                clone.AlphaControls.Add(control.Clone() as Controls.AlphaControl);
            foreach (var control in SizeControls)
                clone.SizeControls.Add(control.Clone() as Controls.SizeControl);
            foreach (var control in SkewControls)
                clone.SkewControls.Add(control.Clone() as Controls.SkewControl);
            foreach (var control in YControls)
                clone.YControls.Add(control.Clone() as Controls.YControl);

            return clone;
        }
    }
}
