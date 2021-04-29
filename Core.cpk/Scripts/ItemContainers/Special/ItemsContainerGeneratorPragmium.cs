namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items.Reactor;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerGeneratorPragmium : ItemsContainerDefault
    {
        public const string Notification_ErrorIncompatibleItem_Message = "Only reactor parts can be placed here.";

        public const string Notification_ErrorIncompatibleItem_Title = "Incompatible item";

        public const string Notification_ReactorActive_Message = "Cannot add or remove reactor components.";

        public const string Notification_ReactorActive_Title = "Reactor is active";

        protected override bool IsValidateContainerInPrivateScope => true;

        public override bool CanAddItem(CanAddItemContext context)
        {
            if (context.ByCharacter is null)
            {
                // server can place here anything
                return true;
            }

            var protoItem = context.Item.ProtoItem;
            if (protoItem is not ItemReactorFuelRod
                && protoItem is not ProtoItemReactorModule)
            {
                // player can place here only specific items
                if (IsClient
                    && !context.IsExploratoryCheck)
                {
                    NotificationSystem.ClientShowNotification(Notification_ErrorIncompatibleItem_Title,
                                                              Notification_ErrorIncompatibleItem_Message,
                                                              NotificationColor.Bad,
                                                              icon: protoItem.Icon);
                }

                return false;
            }

            return ServerCanPlayerInteract(context.Container,
                                           allowClientNotifications: !context.IsExploratoryCheck);
        }

        public override bool CanRemoveItem(CanRemoveItemContext context)
        {
            if (context.ByCharacter is null)
            {
                // server can remove anything
                return true;
            }

            var itemsContainer = context.Container;
            return ServerCanPlayerInteract(itemsContainer, allowClientNotifications: true);
        }

        private static bool ServerCanPlayerInteract(IItemsContainer itemsContainer, bool allowClientNotifications)
        {
            var worldObjectReactor = itemsContainer.OwnerAsStaticObject;
            var protoObject = (ProtoObjectGeneratorPragmium)
                itemsContainer.OwnerAsStaticObject.ProtoGameObject;

            if (!ProtoObjectGeneratorPragmium.SharedIsReactorActiveForItemsContainer(worldObjectReactor,
                    itemsContainer))
            {
                // can interact as the reactor is shut down
                return true;
            }

            // reactor is active
            if (IsClient
                && allowClientNotifications)
            {
                NotificationSystem.ClientShowNotification(Notification_ReactorActive_Title,
                                                          Notification_ReactorActive_Message,
                                                          NotificationColor.Bad,
                                                          icon: protoObject.Icon);
            }

            return false;
        }
    }
}