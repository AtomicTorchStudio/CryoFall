namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemsContainerTrashCan : ProtoItemsContainer
    {
        public override double ServerUpdateIntervalSeconds => 1;

        public override bool CanAddItem(CanAddItemContext context)
        {
            var character = context.ByCharacter;
            if (character == null)
            {
                return true;
            }

            if (!LandClaimSystem.ValidateIsNotUnderRaidblock(context.Container.OwnerAsStaticObject,
                                                             character))
            {
                // don't allow to place anything in a trash can while the area is under raid
                return false;
            }

            // allow everything
            return true;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            if (data.GameObject.OccupiedSlotsCount == 0)
            {
                return;
            }

            using var tempList = Api.Shared.WrapInTempList(data.GameObject.Items);
            foreach (var item in tempList.AsList())
            {
                Server.Items.DestroyItem(item);
            }
        }
    }
}