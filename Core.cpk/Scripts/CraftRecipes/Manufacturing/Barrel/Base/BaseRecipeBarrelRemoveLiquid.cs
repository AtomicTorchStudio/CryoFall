namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class BaseRecipeBarrelRemoveLiquid
        <TInputItem, TOutputItem> : Recipe.RecipeForManufacturing, IRecipeBarrelRemoveLiquid
        where TInputItem : class, IProtoItem, new()
        where TOutputItem : class, IProtoItemLiquidStorage, new()
    {
        private TOutputItem outputItem;

        public sealed override bool IsAutoUnlocked => true;

        protected virtual TimeSpan CraftDuration => TimeSpan.FromSeconds(0.025);

        public override void ServerOnManufacturingCompleted(
            IStaticWorldObject objectManufacturer,
            CraftingQueue craftingQueue)
        {
            // let's remove the liquid amount of the output item from the barrel amount
            var output = this.outputItem;
            var protoBarrel = (IProtoObjectBarrel)objectManufacturer.ProtoWorldObject;
            var state = protoBarrel.GetBarrelPrivateState(objectManufacturer);
            int amount = state.LiquidAmount;

            amount -= output.Capacity;
            if (amount < output.Capacity)
            {
                // consume all the remaining amount
                // that should not happens but still
                amount = 0;
                Logger.Warning("Consumed all the remaining amount of liquid in " + objectManufacturer);
            }

            state.LiquidAmount = (ushort)amount;
            state.LiquidType = amount > 0 ? (LiquidType?)output.LiquidType : null;
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

            var output = this.outputItem;
            var protoBarrel = (IProtoObjectBarrel)objectManufacturer.ProtoWorldObject;
            var state = protoBarrel.GetBarrelPrivateState(objectManufacturer);

            if (state.LiquidAmount < output.Capacity
                || state.LiquidType != output.LiquidType)
            {
                // contains liquid of other type or not enough amount
                return false;
            }

            if (craftingQueue.ContainerOutput.OccupiedSlotsCount > 0
                && craftingQueue.ContainerOutput.Items.Any(
                    i => !(i.ProtoItem is TOutputItem)))
            {
                // contains something other in the output container
                return false;
            }

            return true;
        }

        protected sealed override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            // all barrels
            stations.AddAll<IProtoObjectBarrel>();

            duration = this.CraftDuration;

            inputItems.Add<TInputItem>();

            this.outputItem = GetProtoEntity<TOutputItem>();
            outputItems.Add(this.outputItem);
        }
    }
}