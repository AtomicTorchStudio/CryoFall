namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class CharacterCraftingQueue : CraftingQueue
    {
        public ICharacter Character => (ICharacter)this.GameObject;

        public override IItemsContainer ContainerInput => throw new Exception(
                                                              "Character contains more than one input items container for crafting queue.");

        public override IItemsContainer ContainerOutput => this.Character.SharedGetPlayerContainerInventory();

        protected override IItemsContainer[] CreateInputContainersArray()
        {
            return new[]
            {
                this.Character.SharedGetPlayerContainerInventory(),
                this.Character.SharedGetPlayerContainerHotbar(),
                this.Character.SharedGetPlayerContainerEquipment()
            };
        }
    }
}