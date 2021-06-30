namespace AtomicTorch.CBND.CoreMod.CraftRecipes.GeneratorBurner
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class RecipeGeneratorSteamEmptyBottleFromFullBottle : Recipe.RecipeForManufacturing
    {
        private ItemBottleWater inputItem;

        public override bool IsAutoUnlocked => true;

        public override string Name => "Fill steam generator with water";

        public override void ServerOnManufacturingCompleted(
            IStaticWorldObject objectManufacturer,
            CraftingQueue craftingQueue)
        {
            var liquidCapacity = GetLiquidCapacity(objectManufacturer);

            // let's add liquid amount of the input item into the object liquid amount
            var privateState = GetPrivateState(objectManufacturer);
            var liquidState = this.GetLiquidState(privateState);
            var amount = liquidState.Amount;

            amount += this.inputItem.Capacity;
            if (amount >= liquidCapacity)
            {
                // cannot exceed capacity
                amount = liquidCapacity;
            }

            liquidState.Amount = amount;
        }

        protected override bool CanBeCrafted(
            IStaticWorldObject objectManufacturer,
            CraftingQueue craftingQueue,
            ushort countToCraft)
        {
            if (!base.CanBeCrafted(objectManufacturer, craftingQueue, countToCraft))
            {
                return false;
            }

            var liquidCapacity = GetLiquidCapacity(objectManufacturer);
            var state = this.GetLiquidState(objectManufacturer);

            if (state.Amount + this.inputItem.Capacity
                > liquidCapacity)
            {
                // capacity will exceeded - cannot craft
                return false;
            }

            if (craftingQueue.ContainerOutput.OccupiedSlotsCount > 0
                && craftingQueue.ContainerOutput.Items.Any(
                    i => !(i.ProtoItem is ItemBottleEmpty)))
            {
                // contains something other in the output container
                return false;
            }

            return true;
        }

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectGeneratorSteam>();

            duration = CraftingDuration.Instant;

            this.inputItem = GetProtoEntity<ItemBottleWater>();
            inputItems.Add(this.inputItem);

            outputItems.Add<ItemBottleEmpty>();
        }

        private static double GetLiquidCapacity(IWorldObject objectManufacturer)
        {
            return ((IProtoObjectGeneratorWithFuel)objectManufacturer.ProtoWorldObject).LiquidCapacity;
        }

        private static ObjectGeneratorWithFuelPrivateState GetPrivateState(
            IWorldObject objectManufacturer)
        {
            return objectManufacturer.GetPrivateState<ObjectGeneratorWithFuelPrivateState>();
        }

        private LiquidContainerState GetLiquidState(IWorldObject objectManufacturer)
        {
            return this.GetLiquidState(GetPrivateState(objectManufacturer));
        }

        private LiquidContainerState GetLiquidState(ObjectGeneratorWithFuelPrivateState privateState)
        {
            return privateState.LiquidState;
        }
    }
}