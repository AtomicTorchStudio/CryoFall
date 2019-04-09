namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers
{
    using System;
    using System.Collections.Generic;
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
        private static readonly HashSet<ItemSlotControl> AllSlotControls = new HashSet<ItemSlotControl>();

        private static ClientInputContext clientInputContext;

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

                if (itemInHand != null)
                {
                    clientInputContext.Stop();
                    clientInputContext = null;
                }

                itemInHand = value;

                if (itemInHand != null)
                {
                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    clientInputContext = ClientInputContext
                                         .Start("Drop item from hand to ground")
                                         .HandleAll(
                                             () =>
                                             {
                                                 if (ClientInputManager.IsButtonDown(GameButton.ActionUseCurrentItem))
                                                 {
                                                     ClientInputManager.ConsumeButton(GameButton.ActionUseCurrentItem);
                                                     TryPlaceItemInHandOnGround();
                                                 }
                                                 else if (ClientInputManager.IsButtonDown(GameButton.ActionInteract))
                                                 {
                                                     ClientInputManager.ConsumeButton(GameButton.ActionInteract);
                                                     TryPlaceItemInHandOnGround(count: 1);
                                                 }
                                             });
                }
            }
        }

        public static void Init(ICharacter currentCharacter)
        {
            if (HandContainer != null)
            {
                HandContainer.StateHashChanged -= HandContainerStateHashChanged;
            }

            HandContainer = (IClientItemsContainer)currentCharacter.SharedGetPlayerContainerHand();
            HandContainer.StateHashChanged += HandContainerStateHashChanged;
            HandContainerStateHashChanged();

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

        private static void HandContainerStateHashChanged()
        {
            ItemInHand = HandContainer.GetItemAtSlot(0);
        }

        private static void TryPlaceItemInHandOnGround(ushort? count = null)
        {
            if (Api.Client.UI.GetVisualInPointedPosition() != null)
            {
                // mouse is over some UI control which can take the input/focus
                return;
            }

            var item = ItemInHand;
            if (item == null)
            {
                return;
            }

            var countToDrop = count.HasValue
                              && count.Value <= item.Count
                                  ? count.Value
                                  : item.Count;

            var tilePosition = (Vector2Ushort)Api.Client.Input.MouseWorldPosition;
            ObjectGroundItemsContainer.ClientTryDropItemOnGround(item,
                                                                 countToDrop,
                                                                 tilePosition);
        }
    }
}