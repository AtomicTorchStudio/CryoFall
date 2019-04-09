namespace AtomicTorch.CBND.CoreMod.Systems.Notifications
{
    using System;
    using System.Collections.Generic;
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

    public class NotificationSystem : ProtoSystem<NotificationSystem>
    {
        public const byte BackgroundAlpha = 0xBB;

        public const byte BorderAlpha = 0xEE;

        public const string NotificationNoFreeSpace = "No free space in inventory";

        public const string NotificationSomeItemsDropped = "Some items were dropped to the ground!";

        public static readonly Lazy<Brush> BrushBackgroundBad =
            new Lazy<Brush>(() => new SolidColorBrush(ColorBad.WithAlpha(BackgroundAlpha)));

        public static readonly Lazy<Brush> BrushBackgroundGood =
            new Lazy<Brush>(() => new SolidColorBrush(ColorGood.WithAlpha(BackgroundAlpha)));

        public static readonly Lazy<Brush> BrushBackgroundNeutral =
            new Lazy<Brush>(() => new SolidColorBrush(ColorNeutral.WithAlpha(BackgroundAlpha)));

        public static readonly Lazy<Brush> BrushBorderBad =
            new Lazy<Brush>(() => new SolidColorBrush(ColorBad.WithAlpha(BorderAlpha)));

        public static readonly Lazy<Brush> BrushBorderGood =
            new Lazy<Brush>(() => new SolidColorBrush(ColorGood.WithAlpha(BorderAlpha)));

        public static readonly Lazy<Brush> BrushBorderNeutral =
            new Lazy<Brush>(() => new SolidColorBrush(ColorNeutral.WithAlpha(BorderAlpha)));

        public static readonly SoundResource SoundResourceBad
            = new SoundResource("UI/Notifications/NotificationBad");

        public static readonly SoundResource SoundResourceGood
            = new SoundResource("UI/Notifications/NotificationGood");

        public static readonly SoundResource SoundResourceNeutral
            = new SoundResource("UI/Notifications/NotificationNeutral");

        private static readonly Color ColorBad = Color.FromRgb(0x99, 0x33, 0x33);

        private static readonly Color ColorGood = Color.FromRgb(0x33, 0x88, 0x33);

        private static readonly Color ColorNeutral = Color.FromRgb(0x33, 0x66, 0x88);

        public override string Name => "Notification system";

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
            if (itemsChangedCount != null)
            {
                ClientShowItemsNotification(itemsChangedCount);
            }
        }

        public static HUDNotificationControl ClientShowNotification(
            string title,
            string message = null,
            NotificationColor color = NotificationColor.Neutral,
            ITextureResource icon = null,
            Action onClick = null,
            bool autoHide = true,
            bool playSound = true)
        {
            Api.ValidateIsClient();
            var (brushBackground, brushBorder) = GetBrush(color);
            var soundToPlay = playSound
                                  ? GetSound(color)
                                  : null;
            return HUDNotificationsPanelControl.Show(
                title,
                message,
                brushBackground,
                brushBorder,
                icon,
                onClick,
                autoHide,
                soundToPlay);
        }

        public static void ClientShowNotificationNoSpaceInInventory()
        {
            ClientShowNotification(title: NotificationNoFreeSpace + '.',
                                   color: NotificationColor.Bad);
        }

        public static void ClientShowNotificationNoSpaceInInventoryItemsDroppedToGround()
        {
            ClientShowNotification(title: NotificationNoFreeSpace,
                                   message: NotificationSomeItemsDropped,
                                   color: NotificationColor.Bad);
        }

        public static void ServerSendItemsNotification(
            ICharacter character,
            CreateItemResult createItemResult)
        {
            var itemsChangedCount = SharedGetItemsChangedCount(createItemResult);
            if (itemsChangedCount != null)
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

        public static void ServerSendNotificationNoSpaceInInventoryItemsDroppedToGround(ICharacter character)
        {
            Instance.CallClient(
                character,
                _ => _.ClientRemote_ShowNotificationNoSpaceInInventoryItemsDroppedToGround());
        }

        public static Dictionary<IProtoItem, int> SharedGetItemsChangedCount(CreateItemResult createItemResult)
        {
            if (createItemResult.TotalCreatedCount == 0)
            {
                return null;
            }

            var itemAmounts = createItemResult.ItemAmounts;
            if (itemAmounts == null
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
            HUDItemsNotificationsPanelControl.Show(protoItem, deltaCount);
        }

        private static (Brush background, Brush border) GetBrush(NotificationColor notificationColor)
        {
            Brush background;
            Brush border;
            switch (notificationColor)
            {
                case NotificationColor.Neutral:
                    background = BrushBackgroundNeutral.Value;
                    border = BrushBorderNeutral.Value;
                    break;

                case NotificationColor.Good:
                    background = BrushBackgroundGood.Value;
                    border = BrushBorderGood.Value;
                    break;

                case NotificationColor.Bad:
                    background = BrushBackgroundBad.Value;
                    border = BrushBorderBad.Value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(notificationColor), notificationColor, null);
            }

            return (background, border);
        }

        private static SoundResource GetSound(NotificationColor color)
        {
            switch (color)
            {
                case NotificationColor.Neutral:
                    return SoundResourceNeutral;
                case NotificationColor.Good:
                    return SoundResourceGood;
                case NotificationColor.Bad:
                    return SoundResourceBad;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }
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
                icon: iconTextureLocalPath != null
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
        private void ClientRemote_ShowNotificationNoSpaceInInventoryItemsDroppedToGround()
        {
            ClientShowNotificationNoSpaceInInventoryItemsDroppedToGround();
        }
    }
}