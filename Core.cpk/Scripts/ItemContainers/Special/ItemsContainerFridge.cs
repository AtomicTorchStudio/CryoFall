namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerFridge : ItemsContainerDefault, IProtoItemsContainerFridge
    {
        protected override bool IsValidateContainerInPrivateScope => false;

        public override bool CanAddItem(CanAddItemContext context)
        {
            // allow items with freshness
            // (even with zero freshness such as beverage cans)
            return context.Item.ProtoItem is IProtoItemWithFreshness
                   || context.ByCharacter == null;
        }

        public double SharedGetCurrentFoodFreshnessDecreaseCoefficient(IItemsContainer container)
        {
            var ownerObject = container.OwnerAsStaticObject;
            var protoFridge = (IProtoObjectFridge)ownerObject.ProtoStaticWorldObject;
            var multiplier = protoFridge.ServerGetCurrentFreshnessDurationMultiplier(ownerObject);
            if (multiplier <= 1)
            {
                // no change
                return 1;
            }

            return 1 / multiplier;
        }
    }
}