namespace AtomicTorch.CBND.CoreMod.CraftRecipes.Sprinkler
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class RecipeSprinklerEmptyBottleFromWaterBottle : Recipe.RecipeForManufacturing
    {
        private ItemBottleWater inputItem;

        public override bool IsAutoUnlocked => true;

        [NotLocalizable]
        public override string Name => "Add water from bottle to sprinkler";

        public override bool CanBeCrafted(
            IWorldObject objectManufacturer,
            CraftingQueue craftingQueue,
            ushort countToCraft)
        {
            if (!base.CanBeCrafted(objectManufacturer, craftingQueue, countToCraft))
            {
                return false;
            }

            var liquidCapacity = GetLiquidCapacity(objectManufacturer);
            var privateState = GetPrivateState(objectManufacturer);

            if (privateState.WaterAmount + this.inputItem.Capacity
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

        public override void ServerOnManufacturingCompleted(
            IStaticWorldObject objectManufacturer,
            CraftingQueue craftingQueue)
        {
            var liquidCapacity = GetLiquidCapacity(objectManufacturer);

            // let's add liquid amount of the input item into the object liquid amount
            var privateState = GetPrivateState(objectManufacturer);
            var amount = privateState.WaterAmount;

            amount += this.inputItem.Capacity;
            if (amount >= liquidCapacity)
            {
                // cannot exceed capacity
                amount = liquidCapacity;
            }

            privateState.SetWaterAmount(amount, liquidCapacity, GetPublicState(objectManufacturer));
        }

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.AddAll<ProtoObjectSprinkler>();

            duration = CraftingDuration.Instant;

            this.inputItem = GetProtoEntity<ItemBottleWater>();
            inputItems.Add(this.inputItem);

            outputItems.Add<ItemBottleEmpty>();
        }

        private static double GetLiquidCapacity(IWorldObject objectManufacturer)
        {
            return ((IProtoObjectSprinkler)objectManufacturer.ProtoWorldObject).WaterCapacity;
        }

        private static ProtoObjectSprinkler.PrivateState GetPrivateState(IWorldObject objectManufacturer)
        {
            return ProtoObjectSprinkler.GetPrivateState((IStaticWorldObject)objectManufacturer);
        }

        private static ProtoObjectSprinkler.PublicState GetPublicState(IWorldObject objectManufacturer)
        {
            return ProtoObjectSprinkler.GetPublicState((IStaticWorldObject)objectManufacturer);
        }
    }
}