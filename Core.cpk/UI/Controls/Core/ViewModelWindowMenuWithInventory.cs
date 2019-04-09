namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelWindowMenuWithInventory : BaseViewModel
    {
        public ViewModelWindowMenuWithInventory()
        {
            if (IsDesignTime)
            {
                return;
            }

            var currentPlayerCharacter = ClientCurrentCharacterHelper.Character;
            ;
            var containerInventory = (IClientItemsContainer)currentPlayerCharacter.SharedGetPlayerContainerInventory();
            this.ContainerInventory = containerInventory;

            //ClientContainersExchangeManager.Register(this, containerInventory);
            //ClientContainersExchangeManager.Register(this, currentPlayerCharacter.SharedGetPlayerContainerHotbar());
        }

        public IClientItemsContainer ContainerInventory { get; }
    }
}