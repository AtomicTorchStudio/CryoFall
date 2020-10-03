namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelRechargingStationItemSlot : BaseViewModel
    {
        private uint? chargeValueCurrent;

        private IItem item;

        public Brush BarBrush { get; private set; }

        public uint ChargeValueCurrent
        {
            get => this.chargeValueCurrent ?? 0;
            private set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (this.chargeValueCurrent == value)
                {
                    return;
                }

                this.chargeValueCurrent = value;
                this.BarBrush = this.GetBarBrush();
                this.NotifyThisPropertyChanged();
            }
        }

        public uint ChargeValueMax { get; private set; }

        public IItem Item
        {
            get => this.item;
            set
            {
                if (this.item == value)
                {
                    return;
                }

                this.ReleaseSubscriptions();
                this.item = value;
                this.NotifyThisPropertyChanged();

                this.chargeValueCurrent = null;

                if (this.item is not null)
                {
                    if (this.TrySetupFuelItem())
                    {
                        return;
                    }

                    if (this.TrySetupPowerBank())
                    {
                        return;
                    }
                }

                // the item is null or doesn't provide charge info
                this.ChargeValueMax = 1;
                this.ChargeValueCurrent = 0;
                this.BarBrush = Brushes.Transparent;
            }
        }

        private Brush GetBarBrush()
        {
            return ViewModelItemEnergyCharge.GetBrush(this.ChargeValueCurrent,
                                                      this.ChargeValueMax);
        }

        private bool TrySetupFuelItem()
        {
            if (!(this.item.ProtoItem is IProtoItemWithFuel protoItemWithFuel))
            {
                return false;
            }

            var fuelConfig = protoItemWithFuel.ItemFuelConfig;
            if (!fuelConfig.IsElectricity)
            {
                return false;
            }

            var capacity = fuelConfig.FuelCapacity;
            if (capacity <= 0)
            {
                return false;
            }

            this.ChargeValueMax = (uint)capacity;

            var privateState = this.item.GetPrivateState<ItemWithFuelPrivateState>();
            privateState.ClientSubscribe(_ => _.FuelAmount,
                                         value => this.ChargeValueCurrent = (uint)value,
                                         this);

            this.ChargeValueCurrent = (uint)privateState.FuelAmount;
            return true;
        }

        private bool TrySetupPowerBank()
        {
            if (!(this.item.ProtoItem is IProtoItemPowerBank protoPowerBank))
            {
                return false;
            }

            var capacity = protoPowerBank.EnergyCapacity;
            this.ChargeValueMax = capacity;
            var privateState = this.item.GetPrivateState<ItemPowerBankPrivateState>();
            privateState.ClientSubscribe(_ => _.EnergyCharge,
                                         value => this.ChargeValueCurrent = (uint)value,
                                         this);

            this.ChargeValueCurrent = (uint)privateState.EnergyCharge;
            return true;
        }
    }
}