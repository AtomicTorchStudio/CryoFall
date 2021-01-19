namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelItemFuel : BaseViewModel
    {
        private IItem item;

        private ItemWithFuelPrivateState itemWithFuelPrivateState;

        public double FuelAmount { get; set; }

        public Brush FuelBrush { get; private set; }

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

                var fuelType = itemFuelConfig.FuelType;
                Color fuelColor;
                if (fuelType is not null)
                {
                    (_, fuelColor) = ProtoItemFuelIconColorHelper.GetIconAndColor(fuelType);
                }
                else
                {
                    fuelColor = Api.Client.UI.GetApplicationResource<Color>("ColorFuelRefined");
                }

                var mult = 0.72f;
                fuelColor = Color.FromArgb(fuelColor.A,
                                           (byte)(fuelColor.R * mult),
                                           (byte)(fuelColor.G * mult),
                                           (byte)(fuelColor.B * mult));

                this.FuelBrush = new SolidColorBrush(fuelColor);

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