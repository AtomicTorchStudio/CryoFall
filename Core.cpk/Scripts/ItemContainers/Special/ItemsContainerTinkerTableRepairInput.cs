namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemsContainerTinkerTableRepairInput : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            var character = context.ByCharacter;
            if (character is null)
            {
                return true;
            }

            if (context.Item.ProtoItem
                    is not IProtoItemWithDurability { DurabilityMax: > 0, IsRepairable: true })
            {
                // not a repairable item
                return false;
            }

            if (!context.SlotId.HasValue)
            {
                return true;
            }

            var otherSlotId = context.SlotId.Value != 0
                                  ? (byte)0
                                  : (byte)1;

            var otherItem = context.Container.GetItemAtSlot(otherSlotId);
            if (otherItem is null)
            {
                // there is no item placed in the other slot - allow to place any item in any slot
                return true;
            }

            if (context.Item.ProtoItem == otherItem.ProtoItem)
            {
                // a matching item is placed in the other slot - repair allowed
                return true;
            }

            // there is an item placed in the other slot and their type is not the same
            if (context.IsExploratoryCheck)
            {
                return false;
            }

            NotificationSystem.ClientShowNotification(
                ObjectTinkerTable.ErrorTitle,
                ObjectTinkerTable.ErrorMessage_Input,
                NotificationColor.Bad,
                icon: Api.GetProtoEntity<ObjectTinkerTable>().Icon);

            return false;
        }
    }
}