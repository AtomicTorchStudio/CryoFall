namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.ItemsNotifications.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Axes;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelHUDItemNotificationControl : BaseViewModel
    {
        public static readonly SolidColorBrush BrushBackgroundGreen
            = new SolidColorBrush(Color.FromArgb(0x70, 0x20, 0x90, 0x20));

        public static readonly SolidColorBrush BrushBackgroundRed
            = new SolidColorBrush(Color.FromArgb(0x70, 0xA0, 0x10, 0x10));

        public static readonly SolidColorBrush BrushTextWhite
            = new SolidColorBrush(Color.FromArgb(0xF3, 0xFF, 0xFF, 0xFF));

        private SolidColorBrush backgroundBrush;

        private int deltaCount;

        private string deltaCountText;

        private float requiredHeight;

        private SolidColorBrush textBrush;

        public ViewModelHUDItemNotificationControl(IProtoItem protoItem, int deltaCount)
        {
            this.DeltaCount = deltaCount;
            this.ProtoItem = protoItem;

            if (IsDesignTime)
            {
                this.Icon = Brushes.BlueViolet;
                return;
            }

            this.Icon = Client.UI.GetTextureBrush(this.ProtoItem.Icon);
        }

        public ViewModelHUDItemNotificationControl()
        {
            if (!IsDesignTime)
            {
                throw new Exception("This is design-time only constructor.");
            }

            this.DeltaCount = Api.Random.Next(1, 100) * (Api.Random.Next(0, 2) == 0 ? -1 : 1);
            this.ProtoItem = new ItemAxeIron();

            if (IsDesignTime)
            {
                this.Icon = Brushes.BlueViolet;
                return;
            }

            var iconToLoad = this.ProtoItem.Icon;
            this.Icon = Client.UI.GetTextureBrush(iconToLoad);
        }

        public SolidColorBrush BackgroundBrush => this.backgroundBrush;

        public int DeltaCount
        {
            get => this.deltaCount;
            set
            {
                this.deltaCount = value;
                var isPositive = this.deltaCount > 0;
                this.deltaCountText = isPositive ? '+' + this.deltaCount.ToString() : this.deltaCount.ToString();
                this.textBrush = BrushTextWhite; //isPositive ? BrushTextGreen : BrushTextRed;
                this.backgroundBrush = isPositive ? BrushBackgroundGreen : BrushBackgroundRed;
            }
        }

        public string DeltaCountText => this.deltaCountText;

        public Brush Icon { get; }

        public IProtoItem ProtoItem { get; }

        public float RequiredHeight
        {
            get => this.requiredHeight;
            set
            {
                this.requiredHeight = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public SolidColorBrush TextBrush => this.textBrush;

        public string TooltipText => this.ProtoItem.Name;
    }
}