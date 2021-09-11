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

    public abstract class BaseRecipeBarrelAddLiquid
        <TInputItem,
         TOutputItem>
        : Recipe.RecipeForManufacturing,
          IRecipeBarrelAddLiquid
        where TInputItem : class, IProtoItemLiquidStorage, new()
        where TOutputItem : class, IProtoItem, new()
    {
        private TInputItem inputItem;

        public sealed override bool IsAutoUnlocked => true;

        protected virtual TimeSpan CraftDuration => TimeSpan.FromSeconds(0.025);

        public override void ServerOnManufacturingCompleted(
            IStaticWorldObject objectManufacturer,
            CraftingQueue craftingQueue)
        {
            // let's add liquid amount of the input item into the barrel amount
            var input = this.inputItem;
            var protoBarrel = (IProtoObjectBarrel)objectManufacturer.ProtoWorldObject;
            var state = protoBarrel.GetBarrelPrivateState(objectManufacturer);
            int amount = state.LiquidAmount;

            state.LiquidType = input.LiquidType;
            amount += input.Capacity;
            if (amount >= protoBarrel.LiquidCapacity)
            {
                // cannot exceed barrel capacity
                amount = protoBarrel.LiquidCapacity;
            }

            state.LiquidAmount = (ushort)amount;
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

            var input = this.inputItem;
            var protoBarrel = (IProtoObjectBarrel)objectManufacturer.ProtoWorldObject;
            var state = protoBarrel.GetBarrelPrivateState(objectManufacturer);
            if (state.LiquidAmount > 0
                && state.LiquidType != input.LiquidType)
            {
                // input contains liquid of other type
                return false;
            }

            if (state.LiquidAmount >= protoBarrel.LiquidCapacity)
            {
                // capacity exceeded
                return false;
            }

            if (craftingQueue.ContainerOutput.OccupiedSlotsCount > 0
                && craftingQueue.ContainerOutput.Items.Any(
                    i => i.ProtoItem is not TOutputItem))
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

            this.inputItem = GetProtoEntity<TInputItem>();
            inputItems.Add(this.inputItem);

            outputItems.Add<TOutputItem>();
        }
    }
}