namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelServerColumnSortOrderControl : BaseViewModel
    {
        private static readonly Brush BrushAvailable
            = new SolidColorBrush(Color.FromArgb(0xFF, 0x77, 0x77, 0x77));

        private static readonly Brush BrushSelected
            = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        public Brush BrushArrowDown { get; set; } = BrushAvailable;

        public Brush BrushArrowUp { get; set; } = BrushAvailable;

        public bool IsArrowDownVisible { get; set; } = true;

        public bool IsArrowUpVisible { get; set; } = true;

        public void Refresh(bool isSelected, bool isReversed)
        {
            this.IsArrowDownVisible = this.IsArrowUpVisible = true;

            if (!isSelected)
            {
                this.BrushArrowDown = this.BrushArrowUp = BrushAvailable;
                return;
            }

            if (isReversed)
            {
                this.BrushArrowDown = BrushAvailable;
                this.BrushArrowUp = BrushSelected;
            }
            else
            {
                this.BrushArrowDown = BrushSelected;
                this.BrushArrowUp = BrushAvailable;
            }
        }
    }
}