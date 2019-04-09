namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelHUDStatBar : BaseViewModel
    {
        private float valueCurrent = 75;

        private float valueMax = 100;

        public ViewModelHUDStatBar(string title, Color foregroundColor, Color backgroundColor, Color fireColor)
        {
            this.Title = title;

            this.FireColor = fireColor;
            this.FireTransparentColor = Color.FromArgb(0, fireColor.R, fireColor.G, fireColor.B);

            this.ForegroundBrush = new SolidColorBrush(foregroundColor);
            this.BackgroundBrush = new SolidColorBrush(backgroundColor);
        }

        public ViewModelHUDStatBar(string title)
        {
            this.Title = title;
        }

        public Brush BackgroundBrush { get; }

        public Color FireColor { get; }

        public Color FireTransparentColor { get; }

        public Brush ForegroundBrush { get; }

        public string Title { get; }

        public float ValueCurrent
        {
            get => this.valueCurrent;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == this.valueCurrent)
                {
                    return;
                }

                this.valueCurrent = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ValueFraction));
            }
        }

        public float ValueFraction => this.ValueCurrent / this.ValueMax;

        public float ValueMax
        {
            get => this.valueMax;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == this.valueMax)
                {
                    return;
                }

                this.valueMax = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ValueFraction));
            }
        }
    }
}