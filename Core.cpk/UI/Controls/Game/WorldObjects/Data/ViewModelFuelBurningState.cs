namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelFuelBurningState : BaseViewModel
    {
        public readonly FuelBurningState FuelBurningState;

        public ViewModelFuelBurningState(
            FuelBurningState fuelBurningState)
        {
            this.FuelBurningState = fuelBurningState;

            this.ContainerFuel = (IClientItemsContainer)fuelBurningState?.ContainerFuel;

            this.FuelBurningState.ClientSubscribe(
                _ => _.FuelUseTimeRemainsSeconds,
                secondsToBurn => this.RefreshFuelUsage(),
                this);

            this.FuelBurningState.ClientSubscribe(
                _ => _.CurrentFuelItemType,
                newFuelItemType => this.RefreshFuelUsage(),
                this);

            this.RefreshFuelUsage();

            var character = ClientCurrentCharacterHelper.Character;
            ClientContainersExchangeManager.Register(this,
                                                     this.ContainerFuel,
                                                     allowedTargets: new[]
                                                     {
                                                         character.SharedGetPlayerContainerInventory(),
                                                         character.SharedGetPlayerContainerHotbar()
                                                     });
        }

        public IClientItemsContainer ContainerFuel { get; }

        public float FuelUsageCurrentValue { get; set; } = 50;

        public float FuelUsageMaxValue { get; set; } = 100;

        private void RefreshFuelUsage()
        {
            if (this.FuelBurningState == null)
            {
                return;
            }

            var max = (float)(this.FuelBurningState.CurrentFuelItemType?.FuelAmount ?? 0);
            if (max <= 0)
            {
                this.FuelUsageMaxValue = 1;
                this.FuelUsageCurrentValue = 0;
                return;
            }

            this.FuelUsageMaxValue = max;
            this.FuelUsageCurrentValue = (float)this.FuelBurningState.FuelUseTimeRemainsSeconds;
        }
    }
}