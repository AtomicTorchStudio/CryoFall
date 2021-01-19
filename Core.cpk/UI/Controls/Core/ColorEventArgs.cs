namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows.Media;

    public class ColorEventArgs : EventArgs
    {
        public ColorEventArgs(Color color)
        {
            this.Color = color;
        }

        public Color Color { get; }

        public new Color Empty { get; protected set; }
    }
}