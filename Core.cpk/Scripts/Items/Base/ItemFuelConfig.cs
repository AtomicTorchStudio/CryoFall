namespace AtomicTorch.CBND.CoreMod.Items
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemFuelConfig : IReadOnlyItemFuelConfig
    {
        public ItemFuelConfig()
        {
        }

        public double FuelAmountInitial { get; set; }

        public double FuelCapacity { get; set; }

        public ITextureResource FuelCustomIcon { get; set; }

        public FuelProtoItemsList FuelProtoItemsList { get; } = new FuelProtoItemsList();

        public double FuelUsePerSecond { get; set; }

        public double RefillDuration { get; set; } = 2;

        public string FuelTitle { get; set; } = CoreStrings.TitleFuel;

        IReadOnlyList<IProtoItem> IReadOnlyItemFuelConfig.FuelProtoItemsList => this.FuelProtoItemsList;

        public virtual ITextureResource ClientGetFuelIcon()
        {
            if (this.FuelCustomIcon != null)
            {
                return this.FuelCustomIcon;
            }

            var fuelType = this.FuelProtoItemsList.FirstOrDefault()?.GetType();
            if (fuelType == null)
            {
                Api.Logger.Error("No fuel icon is overriden for " + this);
                return TextureResource.NoTexture;
            }

            return ProtoItemFuelIconColorHelper.GetIconAndColor(fuelType).Item1;
        }

        public void ServerInitialize(ItemWithFuelPrivateState privateState, bool isFirstTimeInit)
        {
            if (isFirstTimeInit)
            {
                // set initial fuel amount
                privateState.FuelAmount = this.FuelAmountInitial;
            }
            else
            {
                // clamp current fuel amount
                privateState.FuelAmount = MathHelper.Clamp(privateState.FuelAmount, 0, this.FuelCapacity);
            }
        }

        public double SharedGetFuelAmount(IItem item)
        {
            var privateState = GetItemPrivateState(item);
            return privateState.FuelAmount;
        }

        public void SharedOnRefilled(IItem item, double fuelAmount)
        {
            var privateState = GetItemPrivateState(item);
            privateState.FuelAmount = fuelAmount;
        }

        public void SharedTryConsumeFuel(
            IItem item,
            ItemWithFuelPrivateState privateState,
            double deltaTime,
            out bool isFuelRanOut)
        {
            var fuelAmount = privateState.FuelAmount;
            fuelAmount -= this.FuelUsePerSecond * deltaTime;

            if (fuelAmount <= 0)
            {
                fuelAmount = 0;
                Api.Logger.Info(item + " - fuel has ran out - make it inactive");
                isFuelRanOut = true;
            }
            else
            {
                isFuelRanOut = false;
            }

            privateState.FuelAmount = fuelAmount;
        }

        public IReadOnlyItemFuelConfig ToReadOnly()
        {
            Api.Assert(this.FuelCapacity >= 0,      "Fuel capacity must be >= 0");
            Api.Assert(this.FuelAmountInitial >= 0, "Initial fuel amount must be >= 0");
            Api.Assert(this.FuelUsePerSecond >= 0,  "Fuel use per second must be >= 0");

            if (this.FuelAmountInitial > this.FuelCapacity)
            {
                throw new Exception(
                    $"{nameof(this.FuelAmountInitial)} ({this.FuelAmountInitial:0.##}) is bigger than {nameof(this.FuelCapacity)} ({this.FuelCapacity:0.##})");
            }

            return this;
        }

        private static ItemWithFuelPrivateState GetItemPrivateState(IItem item)
        {
            return item.GetPrivateState<ItemWithFuelPrivateState>();
        }
    }
}