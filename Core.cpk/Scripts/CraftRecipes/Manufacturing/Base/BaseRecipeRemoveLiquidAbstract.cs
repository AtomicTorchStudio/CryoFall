namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class BaseRecipeRemoveLiquidAbstract
        <TInputProtoItem,
         TOutputProtoItem>
        : Recipe.RecipeForManufacturing
        where TOutputProtoItem : class, IProtoItem, new()
        where TInputProtoItem : class, IProtoItem, new()
    {
        protected abstract TimeSpan CraftDuration { get; }

        protected TOutputProtoItem OutputItem { get; private set; }

        protected abstract double OutputItemLiquidCapacity { get; }

        public override bool CanBeCrafted(
            IWorldObject objectManufacturer,
            CraftingQueue craftingQueue,
            ushort countToCraft)
        {
            if (!base.CanBeCrafted(objectManufacturer, craftingQueue, countToCraft))
            {
                return false;
            }

            var state = this.GetLiquidState((IStaticWorldObject)objectManufacturer);
            if (state.Amount < this.OutputItemLiquidCapacity)
            {
                // not enough amount
                return false;
            }

            if (craftingQueue.ContainerOutput.OccupiedSlotsCount > 0
                && craftingQueue.ContainerOutput.Items.Any(
                    i => !(i.ProtoItem is TOutputProtoItem)))
            {
                // contains something other in the output container
                return false;
            }

            return true;
        }

        public sealed override void ServerOnManufacturingCompleted(
            IStaticWorldObject objectManufacturer,
            CraftingQueue craftingQueue)
        {
            // let's remove the liquid amount of the output item from the refinery liquid state
            var liquidState = this.GetLiquidState(objectManufacturer);
            var amount = liquidState.Amount;
            amount -= this.OutputItemLiquidCapacity;

            liquidState.Amount = amount;

            this.ServerOnLiquidAmountChanged(objectManufacturer);
        }

        protected abstract LiquidContainerState GetLiquidState(IStaticWorldObject staticWorldObject);

        protected abstract void ServerOnLiquidAmountChanged(IStaticWorldObject objectManufacturer);

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            this.SetupRecipeStations(stations);

            duration = this.CraftDuration;

            inputItems.Add<TInputProtoItem>();

            this.OutputItem = GetProtoEntity<TOutputProtoItem>();
            outputItems.Add(this.OutputItem);
        }

        protected abstract void SetupRecipeStations(StationsList stations);
    }
}