namespace KaedePhi.Core.KaedePhi
{
    public partial class Chart
    {
        public Chart Clone()
        {
            return new Chart
            {
                BpmList = BpmList.ConvertAll(bpm => bpm.Clone()),
                Meta = Meta.Clone(),
                JudgeLineList = JudgeLineList.ConvertAll(judgeLine => judgeLine.Clone()),
            };
        }
    }
}