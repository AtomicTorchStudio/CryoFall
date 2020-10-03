namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    /// <summary>
    /// Item management cases:
    /// . 1. Empty hand, Full slot, LMB:
    /// .    - Take in hand item.
    /// .
    /// . 2. Empty hand, Full slot, RMB:
    /// .    - A. If slot contains stackable item: take in hand half of stackable item.
    /// .    - B. Otherwise: call case 1.
    /// .
    /// . 3. Full hand, Empty slot, LMB:
    /// .    - Move/swap item/stack completely from hand to this slot.
    /// .
    /// . 4. Full hand, Full slot, LMB:
    /// .    - A. If same type and stackable: stack items from hand to this slot.
    /// .    - B. Otherwise: call case 3.
    /// .
    /// . 5. Full hand, Empty slot, RMB:
    /// .    - A. If stackable: stack only 1 item from hand to this slot.
    /// .    - B. Otherwise: call case 3.
    /// .
    /// . 6. Full hand, Full slot, RMB:
    /// .    - If same type and stackable: stack single item from hand to this slot.
    /// .
    /// . 7. Empty hand, Full slot, Shift+LMB:
    /// .    - A. If stackable: move all stacks of this item type from one container to another.
    /// .    - B. Otherwise: move item from slot to another container.
    /// .
    /// . 8. Empty hand, Full slot, Shift+RMB:
    /// .    - A. If stackable: move selected stack of items from one container to another.
    /// .    - B. Otherwise: call case 7B.
    /// .
    /// . 9. Empty hand, Full slot, Ctrl+(LMB|RMB):
    /// .    - A. If stackable: move one item from selected stack into another container.
    /// .    - B. Otherwise: call case 7B.
    /// .
    /// . 10. Alt+LMB on usable item to use it (for food, medical, etc).
    /// Alt+RMB on any item to drop it on ground nearby.
    /// </summary>
    internal static class ClientItemManagementCases
    {
        public const string NotificationCannotUseItemDirectly = "Cannot use this item directly.";

        public const string NotificationNoFreeSpaceToMoveItem = "No free space to move item.";

        private static readonly IReadOnlyList<Operation> Cases;

        private static readonly IInputClientService Input = Api.Client.Input;

        private static readonly IItemsClientService ItemsService = Api.Client.Items;

        static ClientItemManagementCases()
        {
            Cases = new Operation[]
            {
                // check in reverse order
                CheckCase10,
                CheckCase9,
                CheckCase8,
                CheckCase7,
                CheckCase6,
                CheckCase5,
                CheckCase4,
                CheckCase3,
                CheckCase2,
                CheckCase1
            };
        }

        private delegate bool Operation(Context c);

        public static bool Execute(ItemSlotControl slotControl, IItem itemInHand, bool isLeftMouseButton, bool isDown)
        {
            var itemInSlot = slotControl.Item;
            if (itemInSlot is null
                && itemInHand is null)
            {
                return false;
            }

            var context = new Context(slotControl, itemInHand, isLeftMouseButton);
            if (!isDown
                && (context.IsShiftHeld || context.IsControlHeld))
            {
                // we don't allow modifier actions on button up
                // no operation executed
                return false;
            }

            // try execute operations one by one
            foreach (var operation in Cases)
            {
                if (operation(context))
                {
                    return true;
                }
            }

            // no operation executed
            return false;
        }

        public static void TryUseItem(IItem item)
        {
            if (item is null)
            {
                return;
            }

            var protoItem = item.ProtoItem;
            if (protoItem is IProtoItemUsableFromContainer)
            {
                protoItem.ClientItemUseStart(item);
                protoItem.ClientItemUseFinish(item);
            }
            else
            {
                NotificationSystem.ClientShowNotification(NotificationCannotUseItemDirectly,
                                                          color: NotificationColor.Bad,
                                                          icon: protoItem.Icon);
            }
        }

        private static bool CheckCase1(Context c)
        {
            if (c.IsEmptyHand
                && c.IsFullSlot
                && c.IsLeftMouseButton)
            {
                InvokeCase1(c);
                return true;
            }

            return false;
        }

        private static bool CheckCase10(Context c)
        {
            if (!c.IsAltHeld)
            {
                return false;
            }

            if (c.IsFullSlot
                && c.IsLeftMouseButton)
            {
                TryUseItem(c.ItemInSlot);
            }
            else if (c.IsRightMouseButton)
            {
                ObjectGroundItemsContainer.ClientTryDropItemOnGround(
                    c.ItemInSlot,
                    countToDrop: c.IsControlHeld
                                     ? (ushort)1
                                     : c.ItemInSlot.Count);
            }

            // even if we don't execute the event we need to be sure no other item management cases handled as Alt key is held
            return true;
        }

        private static bool CheckCase2(Context c)
        {
            if (c.IsEmptyHand
                && c.IsFullSlot
                && c.IsRightMouseButton)
            {
                if (c.ItemInSlotIsStackable)
                {
                    InvokeCase2A(c);
                }
                else
                {
                    InvokeCase1(c);
                }

                return true;
            }

            return false;
        }

        private static bool CheckCase3(Context c)
        {
            if (c.IsFullHand
                && c.IsEmptySlot
                && c.IsLeftMouseButton)
            {
                InvokeCase3(c);
                return true;
            }

            return false;
        }

        private static bool CheckCase4(Context c)
        {
            if (c.IsFullHand
                && c.IsFullSlot
                && c.IsLeftMouseButton)
            {
                if (c.CanStackItems)
                {
                    InvokeCase4A(c);
                }
                else
                {
                    // case 4B - call 3
                    InvokeCase3(c);
                }

                return true;
            }

            return false;
        }

        private static bool CheckCase5(Context c)
        {
            if (c.IsFullHand
                && c.IsEmptySlot
                && c.IsRightMouseButton)
            {
                if (c.ItemInHandIsStackable)
                {
                    InvokeCase5A(c);
                }
                else
                {
                    // case 5B - call 3
                    InvokeCase3(c);
                }

                return true;
            }

            return false;
        }

        private static bool CheckCase6(Context c)
        {
            if (c.IsFullHand
                && c.IsFullSlot
                && c.IsRightMouseButton)
            {
                if (!c.CanStackItems)
                {
                    // non stackable items
                    return true;
                }

                // Stack only 1 item from hand to this slot.
                var itemFrom = c.ItemInHand;
                var itemTo = c.ItemInSlot;

                if (ItemsService.StackItems(
                    itemFrom: itemFrom,
                    itemTo: itemTo,
                    count: 1))
                {
                    // play sound, etc
                    itemTo.ProtoItem.ClientOnItemDrop(itemTo);
                }

                return true;
            }

            return false;
        }

        private static bool CheckCase7(Context c)
        {
            if (c.IsEmptyHand
                && c.IsFullSlot
                && c.IsShiftHeld
                && c.IsLeftMouseButton)
            {
                if (c.ItemInSlotIsStackable)
                {
                    // case 7A
                    MoveAllStacks(c.ItemInSlot, c.TargetContainers);
                }
                else
                {
                    // case 7B
                    InvokeCase7B(c);
                }

                return true;
            }

            return false;
        }

        private static bool CheckCase8(Context c)
        {
            if (c.IsEmptyHand
                && c.IsFullSlot
                && c.IsShiftHeld
                && c.IsRightMouseButton)
            {
                if (c.ItemInSlotIsStackable)
                {
                    // case 8A
                    MoveSingleStack(c.ItemInSlot, c.TargetContainers);
                }
                else
                {
                    // case 8B - call 7B
                    InvokeCase7B(c);
                }

                return true;
            }

            return false;
        }

        private static bool CheckCase9(Context c)
        {
            if (c.IsEmptyHand
                && c.IsFullSlot
                && c.IsControlHeld
                && (c.IsLeftMouseButton || c.IsRightMouseButton))
            {
                if (c.ItemInSlotIsStackable)
                {
                    // case 9A
                    MoveOneItemFromStack(c.ItemInSlot, c.TargetContainers);
                }
                else
                {
                    // case 9B - call 7B
                    InvokeCase7B(c);
                }

                return true;
            }

            return false;
        }

        private static void ExecuteContainersAction(
            IItem item,
            List<IClientItemsContainer> targetContainers,
            Func<IClientItemsContainer, bool> func,
            bool trimTargetContainersListOnSuccess = true)
        {
            IClientItemsContainer newContainer = null;
            foreach (var targetContainer in targetContainers)
            {
                if (!func(targetContainer))
                {
                    continue;
                }

                // successfully moved - break targetContainers cycle and continue with new item
                newContainer = targetContainer;

                if (trimTargetContainersListOnSuccess)
                {
                    // remove other containers to prevent moving items to different containers
                    // (if you move items with shift-click, it should consider moving to only one container,
                    // so if it moved to one container, we could move other items only to that one container)
                    targetContainers.RemoveAll(container => container != newContainer);
                }

                break;
            }

            if (newContainer is not null)
            {
                item.ProtoItem.ClientOnItemDrop(item, newContainer);
            }
        }

        private static void InvokeCase1(Context c)
        {
            // Take item in hand.
            var item = c.ItemInSlot;
            var fromContainer = item.Container;
            var containerHand = ClientItemsManager.HandContainer;

            if (Api.Client.Items.MoveOrSwapItem(item, containerHand, slotId: 0))
            {
                item.ProtoItem.ClientOnItemPick(item, fromContainer);
            }
        }

        private static void InvokeCase2A(Context c)
        {
            // Take half of stackable item.
            var item = c.ItemInSlot;
            var fromContainer = item.Container;
            var countToTake = item.Count;

            // we will round taken count to the closest odd number:
            // so in hand we will have odd count number, and in slot remains even count number
            countToTake = RoundToBiggerOddNumber(countToTake / 2d);

            if (ItemsService.MoveOrSwapItem(
                item,
                ClientItemsManager.HandContainer,
                slotId: 0,
                countToMove: countToTake))
            {
                item.ProtoItem.ClientOnItemPick(item, fromContainer);
            }
        }

        private static void InvokeCase3(Context c)
        {
            // Move/swap item/stack completely from hand to this slot.
            var item = c.ItemInHand;

            if (ItemsService.MoveOrSwapItem(
                item,
                toContainer: c.SlotContainer,
                slotId: c.SlotContainerSlotId))
            {
                item.ProtoItem.ClientOnItemDrop(item);
            }
        }

        private static void InvokeCase4A(Context c)
        {
            // Stack items from hand to this slot.
            var item = c.ItemInHand;

            if (ItemsService.StackItems(
                itemFrom: item,
                itemTo: c.ItemInSlot))
            {
                item.ProtoItem.ClientOnItemDrop(c.ItemInSlot);
            }
        }

        private static void InvokeCase5A(Context c)
        {
            // Move only 1 item from hand to this slot.
            var item = c.ItemInHand;

            if (ItemsService.MoveOrSwapItem(
                item,
                toContainer: c.SlotContainer,
                slotId: c.SlotContainerSlotId,
                countToMove: 1,
                isLogErrors: false))
            {
                item.ProtoItem.ClientOnItemDrop(item);
            }
        }

        private static void InvokeCase7B(Context c)
        {
            // Move item from slot to another container.
            var itemInSlot = c.ItemInSlot;
            var fromContainer = itemInSlot.Container;

            if (c.TargetContainers.Count == 0)
            {
                // no target containers to move the item
                Api.Client.Audio.PlayOneShot(NotificationSystem.SoundResourceBad,
                                             SoundConstants.VolumeUINotifications);
                return;
            }

            foreach (var container in c.TargetContainers)
            {
                if (!ItemsService.MoveOrSwapItem(
                        itemInSlot,
                        container,
                        isLogErrors: false))
                {
                    continue;
                }

                // successfully moved the item
                if (fromContainer.ProtoItemsContainer is ItemsContainerCharacterEquipment)
                {
                    // HACK: we want to play the equipment container unequip sound when
                    // an item is moved from the equipment container by shift-clicking on it.
                    itemInSlot.ProtoItem.ClientOnItemPick(itemInSlot, fromContainer);
                }
                else
                {
                    itemInSlot.ProtoItem.ClientOnItemDrop(itemInSlot, container);
                }

                return;
            }

            NotificationSystem.ClientShowNotification(title: NotificationNoFreeSpaceToMoveItem,
                                                      color: NotificationColor.Bad,
                                                      icon: itemInSlot.ProtoItem.Icon);
        }

        private static void MoveAllStacks(
            IItem item,
            List<IClientItemsContainer> targetContainers)
        {
            var protoItem = item.ProtoItem;
            if (!protoItem.IsStackable)
            {
                throw new Exception("Only stackable items are moveable this way");
            }

            var itemsToMove = item.Container.Items.Where(it => it.ProtoItem == protoItem).ToList();
            if (!itemsToMove.Contains(item))
            {
                Api.Logger.Error("Impossible! Items to move doesn't contain source item");
                return;
            }

            // reorder list so the item is first
            itemsToMove.Remove(item);
            itemsToMove.Insert(0, item);

            foreach (var itemToMove in itemsToMove)
            {
                ExecuteContainersAction(
                    itemToMove,
                    targetContainers,
                    container => ItemsService.MoveOrSwapItem(
                        itemToMove,
                        container,
                        allowSwapping: false,
                        isLogErrors: false));
            }
        }

        private static void MoveOneItemFromStack(IItem item, List<IClientItemsContainer> targetContainers)
        {
            if (!item.ProtoItem.IsStackable)
            {
                throw new Exception("Only stackable items are moveable this way");
            }

            ExecuteContainersAction(
                item,
                targetContainers,
                container => ItemsService.MoveOrSwapItem(
                    item,
                    container,
                    countToMove: 1,
                    allowSwapping: false,
                    isLogErrors: false));
        }

        private static void MoveOrSwapItem(IItem item, List<IClientItemsContainer> targetContainers)
        {
            ExecuteContainersAction(
                item,
                targetContainers,
                container => ItemsService.MoveOrSwapItem(
                    item,
                    container,
                    isLogErrors: false));
        }

        private static void MoveSingleStack(
            IItem item,
            List<IClientItemsContainer> targetContainers)
        {
            if (!item.ProtoItem.IsStackable)
            {
                throw new Exception("Only stackable items are moveable this way");
            }

            ExecuteContainersAction(
                item,
                targetContainers,
                container => ItemsService.MoveOrSwapItem(
                    item,
                    container,
                    allowSwapping: false,
                    isLogErrors: false));
        }

        private static ushort RoundToBiggerOddNumber(double number)
        {
            var intNumber = (int)number;
            var fraction = (int)((number - intNumber) * 10);
            if (fraction == 0)
            {
                return (ushort)intNumber;
            }

            if (intNumber % 2 != 0)
            {
                return (ushort)intNumber;
            }

            return (ushort)(intNumber + 1);
        }

        private class Context
        {
            public Context(ItemSlotControl slotControl, IItem itemInHand, bool isLeftMouseButton)
            {
                var itemInSlot = slotControl.Item;
                this.ItemInSlot = itemInSlot;
                this.ItemInHand = itemInHand;
                this.IsLeftMouseButton = isLeftMouseButton;
                this.SlotContainer = slotControl.Container;
                this.SlotContainerSlotId = slotControl.SlotId;

                this.CanStackItems = itemInHand is not null
                                     && itemInHand.ProtoItem.IsStackable
                                     && itemInSlot is not null
                                     && itemInHand.Container.CanStackItems(itemInHand, itemInSlot);

                this.TargetContainers = itemInSlot is not null
                                            ? ClientContainersExchangeManager.GetTargetContainers(itemInSlot.Container)
                                            : null;

                this.IsShiftHeld = Input.IsKeyHeld(InputKey.Shift,     evenIfHandled: true);
                this.IsControlHeld = Input.IsKeyHeld(InputKey.Control, evenIfHandled: true);
                this.IsAltHeld = Input.IsKeyHeld(InputKey.Alt,         evenIfHandled: true);
            }

            public bool CanStackItems { get; }

            public bool IsAltHeld { get; }

            public bool IsControlHeld { get; }

            public bool IsEmptyHand => this.ItemInHand is null;

            public bool IsEmptySlot => this.ItemInSlot is null;

            public bool IsFullHand => this.ItemInHand is not null;

            public bool IsFullSlot => this.ItemInSlot is not null;

            public bool IsLeftMouseButton { get; }

            public bool IsRightMouseButton => !this.IsLeftMouseButton;

            public bool IsShiftHeld { get; }

            public IItem ItemInHand { get; }

            public bool ItemInHandIsStackable => this.ItemInHand?.ProtoItem.IsStackable ?? false;

            public IItem ItemInSlot { get; }

            public bool ItemInSlotIsStackable => this.ItemInSlot?.ProtoItem.IsStackable ?? false;

            public IItemsContainer SlotContainer { get; }

            public byte SlotContainerSlotId { get; }

            public List<IClientItemsContainer> TargetContainers { get; }
        }
    }
}