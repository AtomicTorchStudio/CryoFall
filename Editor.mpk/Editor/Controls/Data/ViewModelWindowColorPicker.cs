namespace AtomicTorch.CBND.CoreMod.Editor.Controls.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelWindowColorPicker : BaseViewModel
    {
        private byte b;

        private Color color;

        private byte g;

        private byte r;

        public ViewModelWindowColorPicker(Color color)
            : base(isAutoDisposeFields: false)
        {
            this.Color = color;
        }

        public byte B
        {
            get => this.b;
            set
            {
                if (this.b == value)
                {
                    return;
                }

                this.b = value;
                this.NotifyThisPropertyChanged();

                this.RefreshColor();
            }
        }

        public Color Color
        {
            get => this.color;
            set
            {
                if (this.color == value)
                {
                    return;
                }

                this.color = value;
                this.NotifyThisPropertyChanged();

                this.R = value.R;
                this.G = value.G;
                this.B = value.B;
            }
        }

        public byte G
        {
            get => this.g;
            set
            {
                if (this.g == value)
                {
                    return;
                }

                this.g = value;
                this.NotifyThisPropertyChanged();

                this.RefreshColor();
            }
        }

        public byte R
        {
            get => this.r;
            set
            {
                if (this.r == value)
                {
                    return;
                }

                this.r = value;
                this.NotifyThisPropertyChanged();

                this.RefreshColor();
            }
        }

        private void RefreshColor()
        {
            this.Color = Color.FromRgb(this.R, this.G, this.B);
        }
    }
}