namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientItemsManager
    {
        private static readonly HashSet<ItemSlotControl> AllSlotControls = new();

        private static ClientInputContext clientInputContextDropItem;

        private static IItem itemInHand;

        public static IClientItemsContainer HandContainer { get; private set; }

        public static IItem ItemInHand
        {
            get => itemInHand;
            private set
            {
                if (itemInHand == value)
                {
                    return;
                }

                if (itemInHand is not null)
                {
                    clientInputContextDropItem.Stop();
                    clientInputContextDropItem = null;
                }

                itemInHand = value;

                if (itemInHand is not null)
                {
                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    clientInputContextDropItem = ClientInputContext
                                                 .Start("Drop item from hand to ground")
                                                 .HandleAll(ProcessInputCallback);
                }

                static void ProcessInputCallback()
                {
                    if (ClientInputManager.IsButtonDown(GameButton.ActionUseCurrentItem))
                    {
                        ClientInputManager.ConsumeButton(GameButton.ActionUseCurrentItem);
                        TryPlaceItemInHandOnGround();
                    }
                    else if (ClientInputManager.IsButtonDown(GameButton.ActionInteract))
                    {
                        if (TryPlaceItemInHandOnGround(count: 1))
                        {
                            ClientInputManager.ConsumeButton(GameButton.ActionInteract);
                        }
                    }
                }
            }
        }

        public static void Init(ICharacter currentCharacter)
        {
            if (HandContainer is not null)
            {
                HandContainer.StateHashChanged -= RefreshItemInHand;
            }

            HandContainer = (IClientItemsContainer)currentCharacter.SharedGetPlayerContainerHand();
            HandContainer.StateHashChanged += RefreshItemInHand;
            RefreshItemInHand();

            ClientItemInHandDisplayManager.Init(HandContainer);
            ClientContainerSortHelper.Init();
        }

        public static bool OnSlotSelected(ItemSlotControl slotControl, bool isLeftMouseButton, bool isDown)
        {
            return ClientItemManagementCases.Execute(slotControl, ItemInHand, isLeftMouseButton, isDown);
        }

        public static void RefreshHighlight(IItem selectedItem)
        {
            foreach (var itemSlotControl in AllSlotControls)
            {
                itemSlotControl.RefreshHighlight(selectedItem);
            }
        }

        public static void RegisterSlotControl(ItemSlotControl slotControl)
        {
            if (!slotControl.IsSelectable)
            {
                return;
            }

            if (!AllSlotControls.Add(slotControl))
            {
                throw new Exception("Slot control is already registered");
            }

            slotControl.RefreshHighlight(ItemInHand);
        }

        public static void UnregisterSlotControl(ItemSlotControl slotControl)
        {
            AllSlotControls.Remove(slotControl);
        }

        private static void RefreshItemInHand()
        {
            ItemInHand = HandContainer.GetItemAtSlot(0);
        }

        private static async void TryDropItemToGroundAsync(IItem item, ushort countToDrop, Vector2Ushort tilePosition)
        {
            if (await ObjectGroundItemsContainer.ClientTryDropItemOnGround(item,
                                                                           countToDrop,
                                                                           tilePosition,
                                                                           showNotifications: false))
            {
                return;
            }

            if (item.IsDestroyed)
            {
                return;
            }

            // simply drop in the character's position or anywhere nearby (like Alt+RMB click)
            await ObjectGroundItemsContainer.ClientTryDropItemOnGround(item,
                                                                       countToDrop,
                                                                       dropTilePosition: null,
                                                                       showNotifications: true);
        }

        private static bool TryPlaceItemInHandOnGround(ushort? count = null)
        {
            if (Api.Client.UI.GetVisualInPointedPosition() is not null)
            {
                // mouse is over some UI control which can take the input/focus
                return false;
            }

            var item = ItemInHand;
            if (item is null)
            {
                return false;
            }

            var countToDrop = count.HasValue
                              && count.Value <= item.Count
                                  ? count.Value
                                  : item.Count;

            var tilePosition = (Vector2Ushort)Api.Client.Input.MouseWorldPosition;

            var tileObjects = Api.Client.World
                                 .GetStaticObjects(tilePosition);
            if (tileObjects.Count > 0
                && !tileObjects.Any(o => o.ProtoStaticWorldObject is ObjectGroundItemsContainer)
                && tileObjects.Any(o => o.ProtoStaticWorldObject is IInteractableProtoWorldObject))
            {
                // A tile with an interactive world object found.
                // Don't try to place item there and allow interaction with it.
                return false;
            }

            TryDropItemToGroundAsync(item, countToDrop, tilePosition);
            return true;
        }
    }
}