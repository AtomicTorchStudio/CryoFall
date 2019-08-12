namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelItemEnergyCharge : BaseViewModel
    {
        private static readonly Brush BrushGreen = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0xC0, 0x20));

        private static readonly Brush BrushRed = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0x10, 0x10));

        private static readonly Brush BrushYellow = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xE0, 0x10));

        private IItem item;

        private ItemPowerBankPrivateState itemPrivateState;

        public Brush Brush { get; private set; }

        public uint Capacity { get; private set; }

        public uint Charge { get; set; }

        public IItem Item
        {
            get => this.item;
            set
            {
                if (this.item == value)
                {
                    return;
                }

                if (this.item != null)
                {
                    this.ReleaseSubscriptions();
                }

                this.item = value;
                if (this.item == null)
                {
                    return;
                }

                this.Capacity = ((IProtoItemPowerBank)this.item.ProtoItem).EnergyCapacity;

                this.itemPrivateState = this.item.GetPrivateState<ItemPowerBankPrivateState>();
                this.itemPrivateState.ClientSubscribe(_ => _.EnergyCharge,
                                                      _ => this.Refresh(),
                                                      this);

                this.Refresh();
            }
        }

        public string LabelFormat => "{0:F0}/{1:F0} " + CoreStrings.EnergyUnitAbbreviation;

        public static Brush GetBrush(double charge, double capacity)
        {
            var fraction = charge / capacity;
            if (fraction > 0.5)
            {
                return BrushGreen;
            }

            if (fraction > 0.2)
            {
                return BrushYellow;
            }

            return BrushRed;
        }

        private void Refresh()
        {
            this.Charge = (uint)this.itemPrivateState.EnergyCharge;
            this.Brush = GetBrush(this.Charge, this.Capacity);
        }
    }
}