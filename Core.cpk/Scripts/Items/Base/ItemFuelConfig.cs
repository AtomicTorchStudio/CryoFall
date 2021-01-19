namespace AtomicTorch.CBND.CoreMod.Items
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.ItemFuelRefill;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using JetBrains.Annotations;

    public class ItemFuelConfig : IReadOnlyItemFuelConfig
    {
        public double FuelAmountInitial { get; set; }

        public double FuelCapacity { get; set; }

        public ITextureResource FuelCustomIcon { get; set; }

        public FuelProtoItemsList FuelProtoItemsList { get; } = new();

        public string FuelTitle => this.IsElectricity
                                       ? CoreStrings.TitleEnergyCharge
                                       : CoreStrings.TitleFuel;

        [CanBeNull]
        public Type FuelType => this.FuelProtoItemsList.FirstOrDefault()?.GetType();

        public double FuelUsePerSecond { get; set; }

        public bool IsElectricity => this.FuelProtoItemsList.Count > 0
                                     && this.FuelProtoItemsList[0] is IProtoItemFuelElectricity;

        public double RefillDuration { get; set; } = 2;

        IReadOnlyList<IProtoItem> IReadOnlyItemFuelConfig.FuelProtoItemsList => this.FuelProtoItemsList;

        public virtual ITextureResource ClientGetFuelIcon()
        {
            if (this.FuelCustomIcon is not null)
            {
                return this.FuelCustomIcon;
            }

            var fuelType = this.FuelType;
            if (fuelType is null)
            {
                Api.Logger.Error("No fuel icon is overridden for " + this);
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

        public void SharedOnRefilled(
            IItem item,
            double newFuelAmount,
            bool serverNotifyClients)
        {
            var privateState = GetItemPrivateState(item);
            privateState.FuelAmount = MathHelper.Clamp(newFuelAmount, 0, this.FuelCapacity);

            if (Api.IsServer && serverNotifyClients)
            {
                ItemFuelRefillSystem.ServerNotifyItemRefilled(item);
            }
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