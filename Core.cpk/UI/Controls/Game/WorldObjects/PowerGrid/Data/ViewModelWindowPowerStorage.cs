namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelWindowPowerStorage : BaseViewModel
    {
        private readonly ObjectPowerGridPrivateState privateState;

        public ViewModelWindowPowerStorage(ObjectPowerGridPrivateState privateState)
        {
            this.privateState = privateState;
            this.privateState.ClientSubscribe(_ => _.PowerGrid,
                                              _ => this.Refresh(),
                                              this);

            this.Refresh();
        }

        public ViewModelPowerGridState ViewModelPowerGridState { get; set; }

        private void Refresh()
        {
            var oldViewModel = this.ViewModelPowerGridState;

            var powerGrid = this.privateState.PowerGrid;

            this.ViewModelPowerGridState = powerGrid is null
                                               ? null
                                               : new ViewModelPowerGridState(PowerGrid.GetPublicState(powerGrid));

            oldViewModel?.Dispose();
        }
    }
}