namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Systems.ItemFuelRefill;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelHotbarItemWithFuelOverlayControl : BaseViewModel
    {
        private readonly IReadOnlyItemFuelConfig itemFuelConfig;

        private ItemFuelRefillActionState currentAction;

        private IItem item;

        public ViewModelHotbarItemWithFuelOverlayControl(IItem item, IReadOnlyItemFuelConfig itemFuelConfig)
        {
            this.itemFuelConfig = itemFuelConfig;

            var characterState = ClientCurrentCharacterHelper.PrivateState;
            characterState.ClientSubscribe(
                _ => _.CurrentActionState,
                s => { this.CurrentAction = s as ItemFuelRefillActionState; },
                this);

            this.CurrentAction = characterState.CurrentActionState as ItemFuelRefillActionState;
            this.Item = item;
        }

        public byte FuelAmountPercent { get; set; }

        public Brush FuelIcon
            => Api.Client.UI.GetTextureBrush(
                this.itemFuelConfig.ClientGetFuelIcon());

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

                var itemLightPrivateState = this.item.GetPrivateState<ItemWithFuelPrivateState>();
                this.UpdateFuelAmount();

                itemLightPrivateState.ClientSubscribe(
                    _ => _.FuelAmount,
                    _ => this.UpdateFuelAmount(),
                    this);

                this.NotifyPropertyChanged(nameof(this.FuelIcon));
            }
        }

        public double RefillDurationSeconds { get; private set; }

        private ItemFuelRefillActionState CurrentAction
        {
            get => this.currentAction;
            set
            {
                if (this.currentAction == value)
                {
                    return;
                }

                if (value == null
                    || value.Item != this.item)
                {
                    this.RefillDurationSeconds = 0;
                    return;
                }

                this.currentAction = value;
                this.RefillDurationSeconds = this.currentAction.TimeRemainsSeconds;
            }
        }

        private void UpdateFuelAmount()
        {
            var itemLightPrivateState = this.item.GetPrivateState<ItemWithFuelPrivateState>();
            var fuelAmount = itemLightPrivateState.FuelAmount;
            var percent = Math.Round(
                100 * (fuelAmount / this.itemFuelConfig.FuelCapacity),
                MidpointRounding.AwayFromZero);

            if (percent == 0
                && fuelAmount > 0)
            {
                // display 0 only when there is no fuel
                percent = 1;
            }

            this.FuelAmountPercent = (byte)percent;
        }
    }
}