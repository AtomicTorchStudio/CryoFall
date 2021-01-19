namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.ItemsNotifications.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelHUDItemNotificationControl : BaseViewModel
    {
        private static readonly SolidColorBrush BrushBackgroundGreen
            = Api.Client.UI.GetApplicationResource<SolidColorBrush>("BrushColorGreen6");

        private static readonly SolidColorBrush BrushBackgroundRed
            = Api.Client.UI.GetApplicationResource<SolidColorBrush>("BrushColorRed6");

        private int deltaCount;

        private string deltaCountText;

        private float requiredHeight;

        public ViewModelHUDItemNotificationControl(IProtoItem protoItem, int deltaCount)
        {
            this.DeltaCount = deltaCount;
            this.ProtoItem = protoItem;
            this.Icon = Client.UI.GetTextureBrush(this.ProtoItem.Icon);
        }

        public int DeltaCount
        {
            get => this.deltaCount;
            set
            {
                this.deltaCount = value;
                var isPositive = this.deltaCount > 0;
                this.deltaCountText = isPositive ? '+' + this.deltaCount.ToString() : this.deltaCount.ToString();
                this.ForegroundBrush = isPositive
                                           ? BrushBackgroundGreen
                                           : BrushBackgroundRed;
            }
        }

        public string DeltaCountText => this.deltaCountText;

        public SolidColorBrush ForegroundBrush { get; private set; }

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

        public string TooltipText => this.ProtoItem.Name;
    }
}