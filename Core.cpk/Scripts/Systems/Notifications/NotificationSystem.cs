namespace AtomicTorch.CBND.CoreMod.Systems.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.ItemsNotifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using JetBrains.Annotations;

    public class NotificationSystem : ProtoSystem<NotificationSystem>
    {
        public const string NotificationNoFreeSpace = "No free space in inventory";

        public const string NotificationSomeItemsDropped = "Some items were dropped to the ground!";

        public static readonly SoundResource SoundResourceBad
            = new("UI/Notifications/NotificationBad");

        public static readonly SoundResource SoundResourceGood
            = new("UI/Notifications/NotificationGood");

        public static readonly SoundResource SoundResourceNeutral
            = new("UI/Notifications/NotificationNeutral");

        public static event Action<HUDItemNotificationControl> ClientItemNotificationDisplayed;

        public static event Action<HudNotificationControl> ClientNotificationDisplayed;

        public static CreateItemResult CalculateItemsResultExceptContainer(
            CreateItemResult result,
            IItemsContainer itemsContainer)
        {
            if (itemsContainer is null)
            {
                return result;
            }

            var itemAmounts = new Dictionary<IItem, ushort>(result.ItemAmounts);
            foreach (var groundItem in itemsContainer.Items)
            {
                if (!itemAmounts.TryGetValue(groundItem, out var count))
                {
                    continue;
                }

                var newCount = count - groundItem.Count;
                if (newCount > 0)
                {
                    itemAmounts[groundItem] = (ushort)newCount;
                }
                else
                {
                    itemAmounts.Remove(groundItem);
                }
            }

            var newResult = new CreateItemResult(itemAmounts, (uint)itemAmounts.Sum(p => p.Value));
            return newResult;
        }

        public static void ClientShowItemsNotification(Dictionary<IProtoItem, int> itemsChangedCount)
        {
            if (itemsChangedCount.Count == 0)
            {
                return;
            }

            foreach (var itemCountPair in itemsChangedCount)
            {
                ClientShowItemNotification(itemCountPair.Key, itemCountPair.Value);
            }
        }

        public static void ClientShowItemsNotification(CreateItemResult createItemResult)
        {
            var itemsChangedCount = SharedGetItemsChangedCount(createItemResult);
            if (itemsChangedCount is not null)
            {
                ClientShowItemsNotification(itemsChangedCount);
            }
        }

        public static HudNotificationControl ClientShowNotification(
            string title,
            string message = null,
            NotificationColor color = NotificationColor.Neutral,
            ITextureResource icon = null,
            Action onClick = null,
            bool autoHide = true,
            bool playSound = true,
            bool writeToLog = true)
        {
            Api.ValidateIsClient();
            var (brushBackground, brushBorder) = GetBrush(color);
            var soundToPlay = playSound
                                  ? GetSound(color)
                                  : null;

            if (writeToLog)
            {
                Api.Logger.Important(
                    string.Format(
                        "Showing notification:{0}Title: {1}{0}Message: {2}",
                        Environment.NewLine,
                        title,
                        message));
            }

            var notificationControl = HudNotificationControl.Create(
                title,
                message,
                brushBackground,
                brushBorder,
                icon,
                onClick,
                autoHide,
                soundToPlay);

            HudNotificationsPanelControl.ShowNotificationControl(notificationControl);
            Api.SafeInvoke(() => ClientNotificationDisplayed?.Invoke(notificationControl));
            return notificationControl;
        }

        public static void ClientShowNotificationNoSpaceInInventory()
        {
            ClientShowNotification(title: NotificationNoFreeSpace + '.',
                                   color: NotificationColor.Bad);
        }

        public static void ClientShowNotificationNoSpaceInInventoryItemsDroppedToGround(
            [CanBeNull] IProtoItem protoItemForIcon)
        {
            ClientShowNotification(title: NotificationNoFreeSpace,
                                   message: NotificationSomeItemsDropped,
                                   color: NotificationColor.Bad,
                                   icon: protoItemForIcon?.Icon);
        }

        public static void ServerSendItemsNotification(
            ICharacter character,
            CreateItemResult createItemResult,
            IItemsContainer exceptItemsContainer)
        {
            createItemResult = CalculateItemsResultExceptContainer(createItemResult, exceptItemsContainer);
            ServerSendItemsNotification(character, createItemResult);
        }

        public static void ServerSendItemsNotification(
            ICharacter character,
            CreateItemResult createItemResult)
        {
            if (createItemResult.TotalCreatedCount == 0)
            {
                return;
            }

            // clone the result object to not modify the original
            createItemResult = new CreateItemResult(new Dictionary<IItem, ushort>(createItemResult.ItemAmounts),
                                                    createItemResult.TotalCreatedCount);
            // do not send notifications for the items which were not added to the character
            createItemResult.ItemAmounts.RemoveAllByKey(k => k.Container.Owner != character);
            if (createItemResult.TotalCreatedCount == 0)
            {
                return;
            }

            var itemsChangedCount = SharedGetItemsChangedCount(createItemResult);
            if (itemsChangedCount is not null)
            {
                ServerSendItemsNotification(character, itemsChangedCount);
            }
        }

        public static void ServerSendItemsNotification(
            ICharacter character,
            IProtoItem protoItem,
            int deltaCount)
        {
            if (deltaCount == 0)
            {
                return;
            }

            ServerSendItemsNotification(
                character,
                new Dictionary<IProtoItem, int>() { { protoItem, deltaCount } });
        }

        public static void ServerSendItemsNotification(
            ICharacter character,
            Dictionary<IProtoItem, int> itemsChangedCount)
        {
            if (itemsChangedCount.Count == 0)
            {
                return;
            }

            var needToSend = false;
            foreach (var pair in itemsChangedCount)
            {
                if (pair.Value != 0)
                {
                    needToSend = true;
                    break;
                }
            }

            if (!needToSend)
            {
                // no actual changes done
                return;
            }

            var data = new UINotificationItemRemoteCallData(itemsChangedCount);
            Instance.CallClient(
                character,
                _ => _.ClientRemote_ShowItemsNotification(data));
        }

        public static void ServerSendNotification(
            ICharacter character,
            string title,
            string message,
            NotificationColor color,
            string iconTextureLocalPath = null,
            bool autoHide = true)
        {
            Instance.CallClient(
                character,
                _ => _.ClientRemote_ShowNotification(title, message, color, iconTextureLocalPath, autoHide));
        }

        public static void ServerSendNotificationNoSpaceInInventory(ICharacter character)
        {
            Instance.CallClient(
                character,
                _ => _.ClientRemote_ShowNotificationNoSpaceInInventory());
        }

        public static void ServerSendNotificationNoSpaceInInventoryItemsDroppedToGround(
            ICharacter character,
            [CanBeNull] IProtoItem protoItemForIcon)
        {
            Instance.CallClient(
                character,
                _ => _.ClientRemote_ShowNotificationNoSpaceInInventoryItemsDroppedToGround(protoItemForIcon));
        }

        public static Dictionary<IProtoItem, int> SharedGetItemsChangedCount(CreateItemResult createItemResult)
        {
            if (createItemResult.TotalCreatedCount == 0)
            {
                return null;
            }

            var itemAmounts = createItemResult.ItemAmounts;
            if (itemAmounts is null
                || itemAmounts.Count == 0)
            {
                return null;
            }

            var itemsChangedCount = new Dictionary<IProtoItem, int>();
            foreach (var pair in itemAmounts)
            {
                var protoItem = pair.Key.ProtoItem;
                var count = (int)pair.Value;

                if (itemsChangedCount.TryGetValue(protoItem, out var currentCount))
                {
                    count += currentCount;
                }

                itemsChangedCount[protoItem] = count;
            }

            return itemsChangedCount;
        }

        private static void ClientShowItemNotification(IProtoItem protoItem, int deltaCount)
        {
            Api.ValidateIsClient();
            var notificationControl = HUDItemsNotificationsPanelControl.Show(protoItem, deltaCount);
            Api.SafeInvoke(() => ClientItemNotificationDisplayed?.Invoke(notificationControl));
        }

        private static (Brush background, Brush border) GetBrush(NotificationColor notificationColor)
        {
            Brush background;
            Brush border;
            switch (notificationColor)
            {
                case NotificationColor.Neutral:
                    background = NotificationBrushes.BrushBackgroundNeutral;
                    border = NotificationBrushes.BrushBorderNeutral;
                    break;

                case NotificationColor.Good:
                    background = NotificationBrushes.BrushBackgroundGood;
                    border = NotificationBrushes.BrushBorderGood;
                    break;

                case NotificationColor.Bad:
                    background = NotificationBrushes.BrushBackgroundBad;
                    border = NotificationBrushes.BrushBorderBad;
                    break;

                case NotificationColor.Event:
                    background = NotificationBrushes.BrushBackgroundEvent;
                    border = NotificationBrushes.BrushBorderEvent;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(notificationColor), notificationColor, null);
            }

            return (background, border);
        }

        private static SoundResource GetSound(NotificationColor color)
        {
            return color switch
            {
                NotificationColor.Neutral => SoundResourceNeutral,
                NotificationColor.Good    => SoundResourceGood,
                NotificationColor.Bad     => SoundResourceBad,
                // most events have their own sound but it's a fallback
                NotificationColor.Event => SoundResourceNeutral,
                _                       => throw new ArgumentOutOfRangeException(nameof(color), color, null)
            };
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered)]
        private void ClientRemote_ShowItemsNotification(UINotificationItemRemoteCallData data)
        {
            ClientShowItemsNotification(data.ItemsChangedCount);
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered)]
        private void ClientRemote_ShowNotification(
            string title,
            string message,
            NotificationColor color,
            string iconTextureLocalPath,
            bool autoHide)
        {
            ClientShowNotification(
                title,
                message,
                color,
                icon: iconTextureLocalPath is not null
                          ? new TextureResource(iconTextureLocalPath)
                          : null,
                autoHide: autoHide);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_ShowNotificationNoSpaceInInventory()
        {
            ClientShowNotificationNoSpaceInInventory();
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_ShowNotificationNoSpaceInInventoryItemsDroppedToGround(
            [CanBeNull] IProtoItem protoItemForIcon)
        {
            ClientShowNotificationNoSpaceInInventoryItemsDroppedToGround(protoItemForIcon);
        }
    }
}