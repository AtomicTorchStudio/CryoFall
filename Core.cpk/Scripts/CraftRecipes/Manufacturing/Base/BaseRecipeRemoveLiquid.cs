namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public abstract class BaseRecipeRemoveLiquid
        <TInputProtoItem,
         TOutputProtoItem>
        : BaseRecipeRemoveLiquidAbstract<TInputProtoItem, TOutputProtoItem>
        where TOutputProtoItem : class, IProtoItemLiquidStorage, new()
        where TInputProtoItem : class, IProtoItem, new()
    {
        protected override double OutputItemLiquidCapacity => this.OutputItem.Capacity;
    }
}