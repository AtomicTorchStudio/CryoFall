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

        public override string Name => "Item durability system";

        public static void ServerInitializeItem(IItemWithDurabilityPrivateState privateState, bool isFirstTimeInit)
        {
            var item = (IItem)privateState.GameObject;
            var protoItem = item.ProtoItem as IProtoItemWithDurablity
                            ?? throw new Exception("The item "
                                                   + item
                                                   + " proto class don't implement "
                                                   + nameof(IProtoItemWithDurablity));

            privateState.DurabilityCurrent = isFirstTimeInit
                                                 ? protoItem.DurabilityMax
                                                 : Math.Min(privateState.DurabilityCurrent, protoItem.DurabilityMax);
        }

        public static void ServerModifyDurability(IItem item, int delta)
        {
            if ((item?.IsDestroyed ?? true)
                || delta == 0)
            {
                return;
            }

            var protoItem = (IProtoItemWithDurablity)item.ProtoItem;
            var durabilityMax = protoItem.DurabilityMax;
            if (durabilityMax == 0)
            {
                // unlimited durability
                return;
            }

            var privateState = item.GetPrivateState<IItemWithDurabilityPrivateState>();
            var durability = (int)privateState.DurabilityCurrent;
            durability += delta;

            durability = MathHelper.Clamp(durability, 0, durabilityMax);
            privateState.DurabilityCurrent = (ushort)durability;

            if (durability > 0)
            {
                return;
            }

            // destroy the item and notify its owner that item is broken!
            Logger.Important("Item broke (durability 0): " + item);

            var container = item.Container;
            var slotId = item.ContainerSlotId;
            var owner = container.OwnerAsCharacter;

            Api.Server.Items.DestroyItem(item);
            Api.SafeInvoke(() => protoItem.ServerOnItemBrokeAndDestroyed(item, container, slotId));

            if (owner != null)
            {
                Instance.CallClient(owner, _ => _.ClientRemote_ItemBroke(item.ProtoItem));
            }
        }

        public static double SharedGetDurabilityFraction(IItem item)
        {
            var protoItem = item.ProtoItem as IProtoItemWithDurablity
                            ?? throw new Exception(
                                $"{item} prototype doesn't implement {typeof(IProtoItemWithDurablity)}");

            return SharedGetDurabilityValue(item)
                   / (double)protoItem.DurabilityMax;
        }

        public static ushort SharedGetDurabilityValue(IItem item)
        {
            return item.GetPrivateState<IItemWithDurabilityPrivateState>().DurabilityCurrent;
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_ItemBroke(IProtoItem protoItem)
        {
            NotificationSystem.ClientShowNotification(
                NotificationItemBroke_Title,
                string.Format(NotificationItemBroke_MessageFormat, protoItem.Name),
                color: NotificationColor.Bad,
                icon: protoItem.Icon,
                playSound: false);

            protoItem.SharedGetItemSoundPreset().PlaySound(ItemSound.Broken);
        }
    }
}