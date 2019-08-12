namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerFridge : ItemsContainerDefault, IProtoItemsContainerFridge
    {
        protected override bool IsValidateContainerInPrivateScope => false;

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