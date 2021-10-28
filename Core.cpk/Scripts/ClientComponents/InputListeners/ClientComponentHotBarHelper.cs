namespace AtomicTorch.CBND.CoreMod.ClientComponents.InputListeners
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Camera;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ClientComponentHotbarHelper : ClientComponent
    {
        private static readonly Dictionary<GameButton, byte> ButtonMappingQuickUseSlot
            = new()
            {
                { GameButton.HotbarQuickUseSlot1, 0 },
                { GameButton.HotbarQuickUseSlot2, 1 },
                { GameButton.HotbarQuickUseSlot3, 2 },
                { GameButton.HotbarQuickUseSlot4, 3 },
                { GameButton.HotbarQuickUseSlot5, 4 },
                { GameButton.HotbarQuickUseSlot6, 5 },
                { GameButton.HotbarQuickUseSlot7, 6 },
                { GameButton.HotbarQuickUseSlot8, 7 },
                { GameButton.HotbarQuickUseSlot9, 8 },
                { GameButton.HotbarQuickUseSlot0, 9 }
            };

        private static readonly Dictionary<GameButton, byte> ButtonMappingSelectSlot
            = new()
            {
                { GameButton.HotbarSelectSlot1, 0 },
                { GameButton.HotbarSelectSlot2, 1 },
                { GameButton.HotbarSelectSlot3, 2 },
                { GameButton.HotbarSelectSlot4, 3 },
                { GameButton.HotbarSelectSlot5, 4 },
                { GameButton.HotbarSelectSlot6, 5 },
                { GameButton.HotbarSelectSlot7, 6 },
                { GameButton.HotbarSelectSlot8, 7 },
                { GameButton.HotbarSelectSlot9, 8 },
                { GameButton.HotbarSelectSlot0, 9 }
            };

        private static readonly IInputClientService Input = Client.Input;

        private ClientInputContext clientInputContext;

        private bool inputIsUsingItem;

        private PlayerCharacterPrivateState privateState;

        private PlayerCharacterPublicState publicState;

        public static bool IsMouseWheelReversedDirection { get; set; }

        public static bool IsUsingMouseWheelInputToScrollHotbar { get; set; }

        protected bool InputIsUsingItem
        {
            set
            {
                if (this.inputIsUsingItem == value)
                {
                    return;
                }

                this.inputIsUsingItem = value;

                var selectedItem = ClientHotbarSelectedItemManager.SelectedItem;
                var protoItem = selectedItem is not null
                                    ? selectedItem.ProtoItem
                                    : ItemNoWeapon.Instance;

                if (this.inputIsUsingItem)
                {
                    protoItem.ClientItemUseStart(selectedItem);
                    return;
                }

                // mouse left button up
                protoItem.ClientItemUseFinish(selectedItem);
            }
        }

        public override void Update(double deltaTime)
        {
            this.ProcessInputUpdate();
            ClientHotbarSelectedItemManager.Update();
        }

        protected override void OnDisable()
        {
            this.clientInputContext.Stop();
        }

        protected override void OnEnable()
        {
            this.publicState = ClientCurrentCharacterHelper.PublicState;
            this.privateState = ClientCurrentCharacterHelper.PrivateState;
            ClientHotbarSelectedItemManager.Init();

            this.clientInputContext =
                ClientInputContext.Start(nameof(ClientComponentWorldCameraZoomManager))
                                  .HandleAll(() =>
                                             {
                                                 if (!IsUsingMouseWheelInputToScrollHotbar
                                                     || WindowsManager.OpenedWindowsCount > 0)
                                                 {
                                                     return;
                                                 }

                                                 var delta = Input.MouseScrollDeltaValue;
                                                 if (delta == 0)
                                                 {
                                                     return;
                                                 }

                                                 if (IsMouseWheelReversedDirection)
                                                 {
                                                     delta *= -1;
                                                 }

                                                 ClientHotbarSelectedItemManager.SelectNextSlot(
                                                     indexDelta: (int)Math.Round(
                                                         delta,
                                                         MidpointRounding.AwayFromZero));
                                             });
        }

        private void ProcessInputUpdate()
        {
            if (this.publicState.IsDead
                || this.privateState.IsDespawned)
            {
                // cannot use hotbar items
                this.InputIsUsingItem = false;
                return;
            }

            if (!MainMenuOverlay.IsHidden)
            {
                // cannot use hotbar items - main menu overlay is displayed
                this.InputIsUsingItem = false;
                return;
            }

            foreach (var pair in ButtonMappingQuickUseSlot)
            {
                if (ClientInputManager.IsButtonDown(pair.Key))
                {
                    // the key was pressed
                    var hotbarSlotId = pair.Value;
                    var item = ClientHotbarSelectedItemManager.ContainerHotbar.GetItemAtSlot(hotbarSlotId);
                    ClientItemManagementCases.TryUseItem(item);
                }
            }

            foreach (var pair in ButtonMappingSelectSlot)
            {
                if (!ClientInputManager.IsButtonDown(pair.Key))
                {
                    continue;
                }

                // the key was pressed
                var hotbarSlotId = pair.Value;

                if (Input.IsKeyHeld(InputKey.Control, evenIfHandled: true)
                    || Input.IsKeyHeld(InputKey.Alt,  evenIfHandled: true))
                {
                    // try to use an item from the hotbar slot
                    var item = ClientHotbarSelectedItemManager.ContainerHotbar.GetItemAtSlot(hotbarSlotId);
                    ClientItemManagementCases.TryUseItem(item);
                }
                else
                {
                    // select the hotbar slot
                    ClientHotbarSelectedItemManager.SelectedSlotId = hotbarSlotId;
                }

                break;
            }

            var canUseHotbarItems = WindowsManager.OpenedWindowsCount == 0;
            if (!canUseHotbarItems)
            {
                this.InputIsUsingItem = false;
                return;
            }

            if (ClientInputManager.IsButtonDown(GameButton.ActionUseCurrentItem)
                && !this.inputIsUsingItem)
            {
                // start using item
                this.InputIsUsingItem = true;
            }
            else if (!ClientInputManager.IsButtonHeld(GameButton.ActionUseCurrentItem))
            {
                // stop using item
                this.InputIsUsingItem = false;
            }
        }
    }
}