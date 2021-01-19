namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;

    public class ViewModelWindowTrashCan : BaseViewModel
    {
        public ViewModelWindowTrashCan(ObjectTrashCan.PrivateState privateState)
        {
            this.PrivateState = privateState;

            this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(
                    privateState.ItemsContainer)
                {
                    IsContainerTitleVisible = false,
                    IsManagementButtonsVisible = false
                };
        }

        public ObjectTrashCan.PrivateState PrivateState { get; }

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }
    }
}