namespace KaedePhi.Core.KaedePhi.Controls
{
    public abstract class ControlBase
    {
        public Easing Easing { get; set; } = new(1);

        public float X { get; set; }

        public abstract ControlBase Clone();
    }
}
