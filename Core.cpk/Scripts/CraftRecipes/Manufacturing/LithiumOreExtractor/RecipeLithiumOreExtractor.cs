namespace AtomicTorch.CBND.CoreMod.CraftRecipes.LithiumOreExtractor
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class RecipeLithiumOreExtractor : Recipe.RecipeForManufacturing
    {
        private const double LiquidAmountToProduceOneOutputItem = 100;

        private ItemOreLithium outputItem;

        public override bool IsAutoUnlocked => true;

        [NotLocalizable]
        public override string Name => "Lithium salts from lithium ore extractor";

        protected TimeSpan CraftDuration => CraftingDuration.Instant;

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
            if (state.Amount < LiquidAmountToProduceOneOutputItem)
            {
                // not enough amount
                return false;
            }

            if (craftingQueue.ContainerOutput.OccupiedSlotsCount > 0
                && craftingQueue.ContainerOutput.Items.Any(
                    i => i.ProtoItem != this.outputItem))
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
            amount -= LiquidAmountToProduceOneOutputItem;

            liquidState.Amount = amount;

            this.ServerOnLiquidAmountChanged(objectManufacturer);
        }

        protected LiquidContainerState GetLiquidState(IStaticWorldObject staticWorldObject)
        {
            return ProtoObjectExtractor.GetPrivateState(staticWorldObject)
                                       .LiquidContainerState;
        }

        protected void ServerOnLiquidAmountChanged(IStaticWorldObject objectManufacturer)
        {
            // do nothing
        }

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.AddAll<ProtoObjectLithiumOreExtractor>();

            duration = this.CraftDuration;

            this.outputItem = GetProtoEntity<ItemOreLithium>();
            outputItems.Add(this.outputItem);
        }
    }
}