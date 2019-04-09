namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientDroppedItemsNotifier
    {
        public const string NotificationItemsDropped_MessageFormat
            = @"A marker has been added to the map.
  [br]You have {0} until the items disappear.";

        public const string NotificationItemsDropped_Title = "Your items were dropped";

        private static NetworkSyncList<Vector2Ushort> droppedItemsLocations;

        private static WeakReference<HUDNotificationControl> lastNotification;

        public static void Init(ICharacter character)
        {
            if (droppedItemsLocations != null)
            {
                droppedItemsLocations.ClientElementInserted -= MarkerAddedHandler;
                droppedItemsLocations.ClientElementRemoved -= MarkerRemovedHandler;
                droppedItemsLocations = null;
            }

            droppedItemsLocations = PlayerCharacter.GetPrivateState(character)
                                                   .DroppedItemsLocations;

            droppedItemsLocations.ClientElementInserted += MarkerAddedHandler;
            droppedItemsLocations.ClientElementRemoved += MarkerRemovedHandler;
        }

        private static void HideLastNotification()
        {
            if (lastNotification != null
                && lastNotification.TryGetTarget(out var control))
            {
                control.Hide(quick: true);
            }

            lastNotification = null;
        }

        private static void MarkerAddedHandler(NetworkSyncList<Vector2Ushort> source, int index, Vector2Ushort value)
        {
            HideLastNotification();

            var icon = Api.GetProtoEntity<ObjectPlayerLootContainer>().DefaultTexture;

            var notification = NotificationSystem.ClientShowNotification(
                title: NotificationItemsDropped_Title,
                message: string.Format(NotificationItemsDropped_MessageFormat,
                                       ClientTimeFormatHelper.FormatTimeDuration(
                                           ObjectPlayerLootContainer.AutoDestroyTimeoutSeconds)),
                color: NotificationColor.Bad,
                icon: icon,
                autoHide: false);

            lastNotification = new WeakReference<HUDNotificationControl>(notification);
        }

        private static void MarkerRemovedHandler(
            NetworkSyncList<Vector2Ushort> source,
            int index,
            Vector2Ushort removedValue)
        {
            HideLastNotification();
        }
    }
}