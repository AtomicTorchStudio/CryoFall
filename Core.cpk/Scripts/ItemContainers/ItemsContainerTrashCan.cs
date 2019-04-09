namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemsContainerTrashCan : ProtoItemsContainer
    {
        public override double ServerUpdateIntervalSeconds => 1;

        public override bool CanAddItem(CanAddItemContext context)
        {
            // allow everything
            return true;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            if (data.GameObject.OccupiedSlotsCount == 0)
            {
                return;
            }

            using (var tempList = Api.Shared.WrapInTempList(data.GameObject.Items))
            {
                foreach (var item in tempList)
                {
                    Server.Items.DestroyItem(item);
                }
            }
        }
    }
}