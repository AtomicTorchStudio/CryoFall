namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.BottleRefillSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemBottleEmpty : ProtoItemGeneric
    {
        private ClientInputContext helperInputListener;

        public override string Description => "Just an empty bottle. Could be used to store some water.";

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Empty bottle";

        public static void ServerSpawnEmptyBottle(ICharacter character, ushort count = 1)
        {
            var createItemResult = Server.Items.CreateItem<ItemBottleEmpty>(character, count);
            if (createItemResult.IsEverythingCreated)
            {
                // notify the owner about the spawned empty bottle
                NotificationSystem.ServerSendItemsNotification(character, createItemResult);
                return;
            }

            createItemResult.Rollback();

            var groundContainer =
                ObjectGroundItemsContainer.ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(character.Tile);
            if (groundContainer == null)
            {
                return;
            }

            createItemResult = Server.Items.CreateItem<ItemBottleEmpty>(groundContainer, count);
            if (createItemResult.IsEverythingCreated)
            {
                // notify the owner about the spawned empty bottle
                NotificationSystem.ServerSendNotificationNoSpaceInInventoryItemsDroppedToGround(
                    character,
                    createItemResult.ItemAmounts.FirstOrDefault().Key?.ProtoItem);
                return;
            }

            // BUG: cannot spawn an empty bottle either to player or to the ground. It's a rare case, but still possible.
            // It's better to return a false result and cancel the action such as drinking of the water.
            NotificationSystem.ServerSendNotificationNoSpaceInInventory(character);
        }

        protected override void ClientItemHotbarSelectionChanged(ClientHotbarItemData data)
        {
            if (data.IsSelected)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                this.helperInputListener = ClientInputContext
                                           .Start("Current empty bottle item")
                                           .HandleButtonDown(
                                               GameButton.ItemReload,
                                               BottleRefillSystem.Instance.ClientTryStartAction);
            }
            else
            {
                this.helperInputListener?.Stop();
                this.helperInputListener = null;
            }
        }

        protected override void ClientItemUseStart(ClientItemData data)
        {
            // try to refill
            BottleRefillSystem.Instance.ClientTryStartAction();
        }
    }
}