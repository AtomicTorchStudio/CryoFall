namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientDroppedItemsNotifier
    {
        public const string NotificationItemsDropped_MessageFormat
            = @"A marker has been added to the map.
  [br]You have {0} until the items disappear.";

        public const string NotificationItemsDropped_Title = "Your items were dropped";

        private static readonly Dictionary<DroppedLootInfo, HudNotificationControl> Notifications
            = new();

        private static NetworkSyncList<DroppedLootInfo> droppedItemsLocations;

        public static void Init(ICharacter character)
        {
            if (droppedItemsLocations is not null)
            {
                droppedItemsLocations.ClientElementInserted -= MarkerAddedHandler;
                droppedItemsLocations.ClientElementRemoved -= MarkerRemovedHandler;
                droppedItemsLocations = null;
            }

            foreach (var notification in Notifications)
            {
                notification.Value.Hide(quick: true);
            }

            Notifications.Clear();

            droppedItemsLocations = PlayerCharacter.GetPrivateState(character)
                                                   .DroppedLootLocations;

            droppedItemsLocations.ClientElementInserted += MarkerAddedHandler;
            droppedItemsLocations.ClientElementRemoved += MarkerRemovedHandler;

            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                () =>
                {
                    foreach (var droppedLootInfo in droppedItemsLocations)
                    {
                        ShowNotification(droppedLootInfo);
                    }
                });
        }

        private static string GetNotificationText(DroppedLootInfo mark)
        {
            var secondsRemains = mark.DestroyAtTime - Api.Client.CurrentGame.ServerFrameTimeApproximated;
            if (secondsRemains < 1)
            {
                secondsRemains = 1;
            }

            return string.Format(NotificationItemsDropped_MessageFormat,
                                 ClientTimeFormatHelper.FormatTimeDuration(
                                     secondsRemains));
        }

        private static void MarkerAddedHandler(
            NetworkSyncList<DroppedLootInfo> droppedLootInfos,
            int index,
            DroppedLootInfo droppedLootInfo)
        {
            ShowNotification(droppedLootInfo);
        }

        private static void MarkerRemovedHandler(
            NetworkSyncList<DroppedLootInfo> droppedLootInfos,
            int index,
            DroppedLootInfo droppedLootInfo)
        {
            if (Notifications.TryGetValue(droppedLootInfo, out var notificationControl))
            {
                notificationControl.Hide(quick: false);
            }
        }

        private static void ShowNotification(DroppedLootInfo droppedLootInfo)
        {
            if (Notifications.TryGetValue(droppedLootInfo, out _))
            {
                // already has a notification
                return;
            }

            var icon = Api.GetProtoEntity<ObjectPlayerLootContainer>().DefaultTexture;

            var notification = NotificationSystem.ClientShowNotification(
                title: NotificationItemsDropped_Title,
                message: GetNotificationText(droppedLootInfo),
                color: NotificationColor.Bad,
                icon: icon,
                autoHide: false,
                playSound: false);

            UpdateNotification(droppedLootInfo, notification);
            Notifications[droppedLootInfo] = notification;
        }

        private static void UpdateNotification(DroppedLootInfo mark, HudNotificationControl notification)
        {
            if (notification.IsHiding)
            {
                return;
            }

            var timeRemains = mark.DestroyAtTime - Api.Client.CurrentGame.ServerFrameTimeApproximated;
            if (timeRemains <= 0)
            {
                notification.Hide(quick: false);
                return;
            }

            notification.Message = GetNotificationText(mark);

            // schedule recursive update in a second
            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                () => UpdateNotification(mark, notification));
        }
    }
}