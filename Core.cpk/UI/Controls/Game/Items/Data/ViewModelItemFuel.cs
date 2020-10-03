namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelItemFuel : BaseViewModel
    {
        private IItem item;

        private ItemWithFuelPrivateState itemWithFuelPrivateState;

        public double FuelAmount { get; set; }

        public double FuelCapacity { get; private set; }

        public string FuelTitle { get; private set; }

        public IItem Item
        {
            get => this.item;
            set
            {
                if (this.item == value)
                {
                    return;
                }

                if (this.item is not null)
                {
                    this.ReleaseSubscriptions();
                }

                this.item = value;
                if (this.item is null)
                {
                    return;
                }

                var itemFuelConfig = ((IProtoItemWithFuel)this.item.ProtoItem).ItemFuelConfig;
                this.FuelCapacity = itemFuelConfig.FuelCapacity;
                this.FuelTitle = itemFuelConfig.FuelTitle;

                this.itemWithFuelPrivateState = this.item.GetPrivateState<ItemWithFuelPrivateState>();
                this.itemWithFuelPrivateState.ClientSubscribe(_ => _.FuelAmount,
                                                              _ => this.Refresh(),
                                                              this);

                this.Refresh();
            }
        }

        private void Refresh()
        {
            this.FuelAmount = this.itemWithFuelPrivateState.FuelAmount;
        }
    }
}