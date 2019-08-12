namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;

    public class ViewModelWindowRechargingStation : BaseViewModel
    {
        public ViewModelWindowRechargingStation(ProtoObjectRechargingStation.PrivateState privateState)
        {
            this.PrivateState = privateState;

            this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(
                    privateState.ItemsContainer,
                    callbackTakeAllItemsSuccess: null)
                {
                    IsContainerTitleVisible = false,
                    IsManagementButtonsVisible = false
                };
        }

        public ProtoObjectRechargingStation.PrivateState PrivateState { get; }

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }
    }
}