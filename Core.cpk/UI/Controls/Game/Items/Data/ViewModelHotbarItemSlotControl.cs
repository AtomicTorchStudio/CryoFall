namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelHotbarItemSlotControl : BaseViewModel
    {
        private IItem item;

        public IItem Item
        {
            get => this.item;
            set
            {
                if (this.item == value)
                {
                    return;
                }

                this.item = value;
                this.OverlayControl = this.item is not null
                                          ? CreateOverlayControl(this.item)
                                          : null;
            }
        }

        public FrameworkElement OverlayControl { get; private set; }

        public Visibility SelectedVisibility { get; set; }
            = IsDesignTime ? Visibility.Visible : Visibility.Collapsed;

        public string ShortcutKey { get; set; }

        private static FrameworkElement CreateOverlayControl(IItem item)
        {
            if (item.ProtoGameObject is IProtoItemWithHotbarOverlay protoItemWithHotbarOverlay)
            {
                return protoItemWithHotbarOverlay.ClientCreateHotbarOverlayControl(item);
            }

            return null;
        }
    }
}