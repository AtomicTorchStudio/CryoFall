namespace AtomicTorch.CBND.CoreMod.CraftRecipes.OilCrackingPlant
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

    public class RecipeOilCrackingPlantEmptyCanisterFromMineralOilCanister : Recipe.RecipeForManufacturing
    {
        private ItemCanisterMineralOil inputItem;

        public override bool IsAutoUnlocked => true;

        [NotLocalizable]
        public override string Name => "Add mineral oil from canister to mineral oil processor";

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
            privateState.IsLiquidStatesChanged = true;
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
                    i => i.ProtoItem is not ItemCanisterEmpty))
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
            stations.AddAll<ProtoObjectOilCrackingPlant>();

            duration = CraftingDuration.Instant;

            this.inputItem = GetProtoEntity<ItemCanisterMineralOil>();
            inputItems.Add(this.inputItem);

            outputItems.Add<ItemCanisterEmpty>();
        }

        private static double GetLiquidCapacity(IWorldObject objectManufacturer)
        {
            return ((ProtoObjectOilCrackingPlant)objectManufacturer.ProtoWorldObject).LiquidCapacityMineralOil;
        }

        private static ProtoObjectOilCrackingPlant.PrivateState GetPrivateState(
            IWorldObject objectManufacturer)
        {
            return ProtoObjectOilCrackingPlant.GetPrivateState(
                (IStaticWorldObject)objectManufacturer);
        }

        private LiquidContainerState GetLiquidState(IWorldObject objectManufacturer)
        {
            return this.GetLiquidState(GetPrivateState(objectManufacturer));
        }

        private LiquidContainerState GetLiquidState(ProtoObjectOilCrackingPlant.PrivateState privateState)
        {
            return privateState.LiquidStateMineralOil;
        }
    }
}