namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ManufacturingState : BaseNetObject
    {
        public ManufacturingState(
            IStaticWorldObject worldObject,
            byte containerInputSlotsCount,
            byte containerOutputSlotsCount)
        {
            var itemsService = Api.Server.Items;
            var containerInput = itemsService.CreateContainer(worldObject, containerInputSlotsCount);
            var containerOutput = itemsService.CreateContainer<ItemsContainerOutput>(worldObject,
                                                                                     containerOutputSlotsCount);
            this.CraftingQueue = new ManufacturingCraftingQueue(containerInput, containerOutput);
        }

        public IItemsContainer ContainerInput => this.CraftingQueue.ContainerInput;

        [TempOnly]
        public ushort? ContainerInputLastStateHash { get; set; }

        public IItemsContainer ContainerOutput => this.CraftingQueue.ContainerOutput;

        // We need this property here also (we cannot reuse the same property from the CraftingQueue).
        [TempOnly]
        public ushort? ContainerOutputLastStateHash { get; set; }

        [SyncToClient]
        public ManufacturingCraftingQueue CraftingQueue { get; }

        public bool HasActiveRecipe => this.CraftingQueue.QueueItems.Count > 0;

        [SyncToClient]
        public Recipe SelectedRecipe { get; set; }

        public void SetSlotsCount(byte input, byte output)
        {
            var itemsService = Api.Server.Items;

            if (this.ContainerInput.SlotsCount < input)
            {
                itemsService.SetSlotsCount(this.ContainerInput, input);
            }

            if (this.ContainerOutput.SlotsCount < output)
            {
                itemsService.SetSlotsCount(this.ContainerOutput, output);
            }
        }
    }
}