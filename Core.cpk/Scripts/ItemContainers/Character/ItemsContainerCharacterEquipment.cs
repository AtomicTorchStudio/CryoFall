namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class ItemsContainerCharacterEquipment : ProtoItemsContainer
    {
        public const string NotificationCannotEquip = "Cannot equip";

        public const string NotificationRemoveFullBody = "You must remove full body equipment first.";

        public const string NotificationRemoveOtherEquipment = "You must remove other equipment first.";

        public const string NotificationUseStationToInstallImplant_Message =
            "You need to use the medical station to install the implant.";

        public const string NotificationUseStationToInstallImplant_Title = "Cannot install";

        public const string NotificationUseStationToRemoveImplant_Message =
            "You need to use the medical station to remove the implant.";

        public const string NotificationUseStationToRemoveImplant_Title = "Cannot remove";

        public static bool HasArmorOrFullBodyEquipment(ICharacter character)
        {
            var container = character.SharedGetPlayerContainerEquipment();
            return container.Items.Any(
                item => item.ProtoItem is IProtoItemEquipment eq
                        && (eq.EquipmentType == EquipmentType.Armor
                            || eq.EquipmentType == EquipmentType.FullBody));
        }

        public override bool CanAddItem(CanAddItemContext context)
        {
            if (!(context.Item.ProtoItem is IProtoItemEquipment protoItemEquipment))
            {
                // not an equipment - cannot be placed here
                return false;
            }

            if (!context.SlotId.HasValue)
            {
                // no specific slot provided - so answer "true" because any equipment item could be added here
                return true;
            }

            var slotIdValue = (EquipmentType)context.SlotId.Value;
            var itemEquipmentType = protoItemEquipment.EquipmentType;

            if (!context.IsExploratoryCheck
                && (itemEquipmentType == EquipmentType.Armor
                    || itemEquipmentType == EquipmentType.FullBody)
                && !StatusEffectPeredozinApplication.SharedCheckCanEquipArmor(context.Container.OwnerAsCharacter,
                                                                   clientShowNotification: true))
            {
                // don't allow equipping armor while peredozin is applying
                return false;
            }

            if (itemEquipmentType == EquipmentType.FullBody)
            {
                if (slotIdValue != EquipmentType.Armor)
                {
                    // cannot place full body armor in another slots
                    return false;
                }

                if (context.IsExploratoryCheck)
                {
                    // no more checks
                    return true;
                }

                // It's actual operation, perform more checks!
                // Need to verify that head is an empty slot
                // Please note: can equip full body when there is an armor without the head item - will swap items.
                var container = context.Container;
                if (IsSlotEmpty(container, EquipmentType.Head))
                {
                    // can place here
                    return true;
                }

                // head and legs are not empty
                if (IsClient)
                {
                    NotificationSystem.ClientShowNotification(
                        NotificationCannotEquip,
                        NotificationRemoveOtherEquipment,
                        NotificationColor.Bad,
                        protoItemEquipment.Icon);
                }

                return false;
            }

            var isValidSlot = protoItemEquipment.CompatibleContainerSlotsIds.Contains((byte)slotIdValue);
            if (!isValidSlot)
            {
                // the selected slot doesn't match the equipment
                return false;
            }

            if (itemEquipmentType == EquipmentType.Implant)
            {
                // implant item
                if (context.ByCharacter is null
                    || CreativeModeSystem.SharedIsInCreativeMode(context.ByCharacter))
                {
                    // Allowed to add/remove implant item by the game only (via medical station).
                    // But allow to characters in the creative mode to do this directly.
                    return true;
                }

                if (IsClient
                    && !context.IsExploratoryCheck
                    && !(protoItemEquipment is ItemImplantBroken))
                {
                    NotificationSystem.ClientShowNotification(
                        NotificationUseStationToInstallImplant_Title,
                        NotificationUseStationToInstallImplant_Message,
                        NotificationColor.Bad,
                        protoItemEquipment.Icon);
                }

                return false;
            }

            if (!context.IsExploratoryCheck
                && itemEquipmentType == EquipmentType.Head)
            {
                // Regular equipment - can equip head ONLY if there is no full body armor.
                // Please note: can equip regular armor (without the helmet) even if there is full body armor - will swap items.
                if (IsHasFullBodyArmor(context.Container))
                {
                    // has full body armor - cannot equip this item
                    if (IsClient)
                    {
                        NotificationSystem.ClientShowNotification(
                            NotificationCannotEquip,
                            NotificationRemoveFullBody,
                            NotificationColor.Bad,
                            protoItemEquipment.Icon);
                    }

                    return false;
                }
            }

            return true;
        }

        public override bool CanRemoveItem(CanRemoveItemContext context)
        {
            if (!(context.Item.ProtoItem is IProtoItemEquipmentImplant protoItemEquipment))
            {
                // impossible - how did it end up here?
                return true;
            }

            var itemEquipmentType = protoItemEquipment.EquipmentType;
            if (itemEquipmentType == EquipmentType.Implant)
            {
                // implant item
                if (context.ByCharacter is null
                    || CreativeModeSystem.SharedIsInCreativeMode(context.ByCharacter))
                {
                    // Allowed to add/remove implant item by the game only (via medical station).
                    // But allow to characters in the creative mode to do this directly.
                    return true;
                }

                if (IsClient)
                {
                    ClientShowNotificationCannotRemoveImplant(protoItemEquipment);
                }
                else
                {
                    this.CallClient(context.ByCharacter,
                                    _ => _.ClientRemote_ClientShowNotificationCannotRemoveImplant(protoItemEquipment));
                }

                return false;
            }

            // can remove anything
            return true;
        }

        public override byte? FindSlotForItem(IItemsContainer container, IProtoItem protoItem)
        {
            if (!(protoItem is IProtoItemEquipment protoEquipment))
            {
                // not an equipment - cannot be placed here
                return null;
            }

            var allowedSlotsIds = protoEquipment.CompatibleContainerSlotsIds;
            if (allowedSlotsIds.Length == 1)
            {
                // return only one appropriate slot
                return allowedSlotsIds[0];
            }

            // this equipment type allows placing to multiple slots
            // find an empty one of them
            foreach (var allowedSlotsId in allowedSlotsIds)
            {
                if (!container.IsSlotOccupied(allowedSlotsId))
                {
                    // empty slot found
                    return allowedSlotsId;
                }
            }

            // no empty slots found, return first allowed slot id
            return allowedSlotsIds[0];
        }

        private static void ClientShowNotificationCannotRemoveImplant(IProtoItemEquipmentImplant protoItemImplant)
        {
            NotificationSystem.ClientShowNotification(
                NotificationUseStationToRemoveImplant_Title,
                NotificationUseStationToRemoveImplant_Message,
                NotificationColor.Bad,
                protoItemImplant.Icon);
        }

        private static bool IsHasFullBodyArmor(IItemsContainer container)
        {
            return container.Items.Any(
                item => item.ProtoItem is IProtoItemEquipment protoEquipment
                        && protoEquipment.EquipmentType == EquipmentType.FullBody);
        }

        private static bool IsSlotEmpty(IItemsContainer container, EquipmentType slotId)
        {
            return container.GetItemAtSlot((byte)slotId) is null;
        }

        private void ClientRemote_ClientShowNotificationCannotRemoveImplant(
            IProtoItemEquipmentImplant protoItemImplant)
        {
            ClientShowNotificationCannotRemoveImplant(protoItemImplant);
        }
    }
}