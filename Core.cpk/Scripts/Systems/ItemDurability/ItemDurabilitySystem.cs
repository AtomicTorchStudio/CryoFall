namespace AtomicTorch.CBND.CoreMod.Systems.ItemDurability
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemDurabilitySystem : ProtoSystem<ItemDurabilitySystem>
    {
        public const string NotificationItemBroke_MessageFormat = "Your {0} has broken down and is gone forever.";

        public const string NotificationItemBroke_Title = "Item broke";

        // if durability is >= than this threshold, it's displayed as green
        public const double ThresholdFractionGreenStatus = 0.5;

        // if durability is >= than this threshold, it's displayed as yellow
        public const double ThresholdFractionYellowStatus = 0.2;

        public override string Name => "Item durability system";

        /// <summary>
        /// An item with durability can be broken by using this method.
        /// (item that has unlimited durability will be ignored)
        /// </summary>
        public static void ServerBreakItem(IItem item)
        {
            if ((item?.IsDestroyed ?? true)
                || item.ProtoItem is not IProtoItemWithDurability protoItem)
            {
                return;
            }

            var durabilityMax = protoItem.DurabilityMax;
            if (durabilityMax == 0)
            {
                // unlimited durability
                return;
            }

            var privateState = item.GetPrivateState<IItemWithDurabilityPrivateState>();
            privateState.DurabilityCurrent = 0;

            // destroy the item and notify its owner that item is broken
            Logger.Important("Item broke (durability 0): " + item);

            var container = item.Container;
            var slotId = item.ContainerSlotId;
            var owner = container.OwnerAsCharacter;

            Api.Server.Items.DestroyItem(item);
            Api.SafeInvoke(() => protoItem.ServerOnItemBrokeAndDestroyed(item, container, slotId));

            if (owner is not null)
            {
                Instance.CallClient(owner,
                                    _ => _.ClientRemote_ItemBroke(item.ProtoItem));
            }
        }

        public static void ServerInitializeItem(IItemWithDurabilityPrivateState privateState, bool isFirstTimeInit)
        {
            var item = (IItem)privateState.GameObject;
            var protoItem = item.ProtoItem as IProtoItemWithDurability
                            ?? throw new Exception("The item "
                                                   + item
                                                   + " proto class don't implement "
                                                   + nameof(IProtoItemWithDurability));

            privateState.DurabilityCurrent = isFirstTimeInit
                                                 ? protoItem.DurabilityMax
                                                 : Math.Min(privateState.DurabilityCurrent, protoItem.DurabilityMax);
        }

        /// <param name="roundUp">
        /// By default the absolute delta value is rounded down (floor) to nearest integer.
        /// You could change this to round up (ceiling).
        /// Round up will always maximize durability loss. Round down will minimize it.
        /// </param>
        public static void ServerModifyDurability(IItem item, double delta, bool roundUp)
        {
            var deltaInt = roundUp
                               ? (int)Math.Ceiling(Math.Abs(delta))
                               : (int)Math.Floor(Math.Abs(delta));
            deltaInt *= Math.Sign(delta);
            ServerModifyDurability(item, deltaInt);
        }

        public static void ServerModifyDurability(IItem item, int delta)
        {
            if ((item?.IsDestroyed ?? true)
                || delta == 0
                || item.ProtoItem is not IProtoItemWithDurability protoItem)
            {
                return;
            }

            var durabilityMax = protoItem.DurabilityMax;
            if (durabilityMax == 0)
            {
                // unlimited durability
                return;
            }

            var privateState = item.GetPrivateState<IItemWithDurabilityPrivateState>();
            var durability = (long)privateState.DurabilityCurrent;
            durability += delta;

            durability = MathHelper.Clamp(durability, 0, durabilityMax);
            privateState.DurabilityCurrent = (uint)durability;

            if (durability > 0)
            {
                return;
            }

            ServerBreakItem(item);
        }

        public static double SharedGetDurabilityFraction(IItem item)
        {
            var protoItem = item.ProtoItem as IProtoItemWithDurability;
            if (protoItem is null
                || protoItem.DurabilityMax <= 0)
            {
                // non-degradable item
                return 1.0;
            }

            var result = SharedGetDurabilityValue(item)
                         / (double)protoItem.DurabilityMax;
            return MathHelper.Clamp(result, 0, 1);
        }

        public static int SharedGetDurabilityPercent(IItem item)
        {
            var result = SharedGetDurabilityFraction(item);
            result = Math.Round(100 * result, MidpointRounding.AwayFromZero);
            return (int)result;
        }

        public static uint SharedGetDurabilityValue(IItem item)
        {
            return item.GetPrivateState<IItemWithDurabilityPrivateState>().DurabilityCurrent;
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        public void ClientRemote_ItemBroke(IProtoItem protoItem)
        {
            NotificationSystem.ClientShowNotification(
                NotificationItemBroke_Title,
                string.Format(NotificationItemBroke_MessageFormat, protoItem.Name),
                color: NotificationColor.Bad,
                icon: protoItem.Icon,
                playSound: false);

            protoItem.SharedGetItemSoundPreset()
                     .PlaySound(ItemSound.Broken);
        }
    }
}