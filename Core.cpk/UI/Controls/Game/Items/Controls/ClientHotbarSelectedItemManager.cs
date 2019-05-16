namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientHotbarSelectedItemManager
    {
        private static bool isFirstTime;

        private static IItem selectedItem;

        public static event Action<byte?> SelectedSlotIdChanged;

        public static IClientItemsContainer ContainerHotbar { get; private set; }

        public static IItem SelectedItem => selectedItem;

        public static byte? SelectedSlotId
        {
            get => ClientCurrentCharacterHelper.PrivateState.SelectedHotbarSlotId;
            set
            {
                var previousSlotId = ClientCurrentCharacterHelper.PrivateState.SelectedHotbarSlotId;
                if (previousSlotId == value)
                {
                    // no need to change slot
                    return;
                }

                Api.GetProtoEntity<PlayerCharacter>().ClientSelectHotbarSlot(value);
                RefreshSelectedItem();

                var currentSlotId = ClientCurrentCharacterHelper.PrivateState.SelectedHotbarSlotId;
                if (previousSlotId != currentSlotId)
                {
                    SelectedSlotIdChanged?.Invoke(currentSlotId);
                }
            }
        }

        public static void Init()
        {
            isFirstTime = true;
            var currentPlayerCharacter = ClientCurrentCharacterHelper.Character;
            var privateState = PlayerCharacter.GetPrivateState(currentPlayerCharacter);
            ContainerHotbar = (IClientItemsContainer)currentPlayerCharacter.SharedGetPlayerContainerHotbar();
            SelectedSlotId = privateState.SelectedHotbarSlotId;
            Update();
        }

        public static void Select(IItem item)
        {
            if (item.Container != ContainerHotbar)
            {
                throw new Exception("Cannot select item from not the hotbar container: " + item);
            }

            SelectedSlotId = item.ContainerSlotId;
        }

        public static void SelectNextSlot(int indexDelta)
        {
            if (indexDelta == 0)
            {
                return;
            }

            var newSelectedSlotId = SelectedSlotId + indexDelta;
            var slotsCount = ContainerHotbar.SlotsCount;
            while (newSelectedSlotId < 0)
            {
                newSelectedSlotId = slotsCount + newSelectedSlotId;
            }

            newSelectedSlotId %= slotsCount;
            SelectedSlotId = (byte)newSelectedSlotId;
        }

        public static void Update()
        {
            RefreshSelectedItem();
        }

        private static void RefreshSelectedItem()
        {
            if (!TimeOfDaySystem.ClientIsInitialized)
            {
                // wait until DayNightSystem is initialized as some times (such as hand lamp) need to know time of day
                return;
            }

            var isByPlayer = !isFirstTime;
            isFirstTime = false;

            var newSelectedItem = ClientCurrentCharacterHelper.Character.SharedGetPlayerSelectedHotbarItem();
            if (selectedItem == newSelectedItem)
            {
                return;
            }

            selectedItem?.ProtoItem.ClientItemHotbarSelectionChanged(selectedItem, isSelected: false, isByPlayer);
            selectedItem = newSelectedItem;
            selectedItem?.ProtoItem.ClientItemHotbarSelectionChanged(selectedItem, isSelected: true, isByPlayer);
        }
    }
}