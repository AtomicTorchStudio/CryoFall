namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStyle;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowInventory : BaseViewModel
    {
        private readonly ICharacter character;

        private readonly IClientItemsContainer containerHand;

        private bool isActive;

        private bool isHeadEquipmentDisplayed;

        public ViewModelWindowInventory(FrameworkElement controlSkeletonView = null)
        {
            if (IsDesignTime)
            {
                return;
            }

            this.character = ClientCurrentCharacterHelper.Character;
            this.ContainerInventory = (IClientItemsContainer)this.character.SharedGetPlayerContainerInventory();
            this.ContainerEquipment = (IClientItemsContainer)this.character.SharedGetPlayerContainerEquipment();
            this.containerHand = (IClientItemsContainer)this.character.SharedGetPlayerContainerHand();

            this.InventorySkeleton.Control = controlSkeletonView;
            this.InventorySkeleton.CurrentCharacter = this.character;

            this.DefenseStats.CurrentCharacter = this.character;

            var publicState = ClientCurrentCharacterHelper.PublicState;
            this.isHeadEquipmentDisplayed = publicState.IsHeadEquipmentHiddenForSelfAndPartyMembers;
        }

        public BaseCommand CommandOpenHelpMenu
            => WindowContainerHelp.CommandOpenMenu;

        public BaseCommand CommandOpenStyleMenu
            => new ActionCommand(
                () => Client.UI.LayoutRootChildren.Add(
                    new WindowCharacterStyleCustomization()));

        public BaseCommand CommandOpenUnstuckMenu
            => new ActionCommand(
                WindowCharacterUnstuckHelper.ShowWindow);

        public IClientItemsContainer ContainerEquipment { get; }

        public IClientItemsContainer ContainerInventory { get; }

        public ViewModelCharacterDefenseStats DefenseStats { get; }
            = new ViewModelCharacterDefenseStats();

        public bool HasHeadEquipmentOrFullBodyArmorEquipped { get; set; }

        public ViewModelInventorySkeleton InventorySkeleton { get; }
            = new ViewModelInventorySkeleton();

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;

                this.InventorySkeleton.IsActive = this.isActive;
                this.DefenseStats.IsActive = this.isActive;

                if (this.isActive)
                {
                    ClientContainersExchangeManager.Register(this, this.character.SharedGetPlayerContainerEquipment());
                    ClientContainersExchangeManager.Register(this, this.character.SharedGetPlayerContainerInventory());
                    ClientContainersExchangeManager.Register(this, this.character.SharedGetPlayerContainerHotbar());

                    this.ContainerEquipment.StateHashChanged += this.ContainerEquipmentStateHashChangedHandler;
                    this.RefreshVisibilityHeadAndLegsSlots();

                    this.RefreshItemInHandUseText();
                    this.containerHand.StateHashChanged += this.ContainerHandStateHashChangedHandler;
                }
                else
                {
                    ClientContainersExchangeManager.Unregister(this);

                    this.ContainerEquipment.StateHashChanged -= this.ContainerEquipmentStateHashChangedHandler;
                    this.containerHand.StateHashChanged -= this.ContainerHandStateHashChangedHandler;
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public bool IsHeadEquipmentDisplayed
        {
            get => this.isHeadEquipmentDisplayed;
            set
            {
                if (this.isHeadEquipmentDisplayed == value)
                {
                    return;
                }

                this.isHeadEquipmentDisplayed = value;
                this.NotifyThisPropertyChanged();

                CharacterStyleSystem.ClientSetHeadEquipmentVisibility(this.isHeadEquipmentDisplayed);
            }
        }

        public bool IsHeadSlotBlocked { get; private set; }

        public string ItemInHandUseText { get; private set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.IsActive = false;
        }

        private void ContainerEquipmentStateHashChangedHandler()
        {
            this.RefreshVisibilityHeadAndLegsSlots();
        }

        private void ContainerHandStateHashChangedHandler()
        {
            this.RefreshItemInHandUseText();
        }

        private void RefreshItemInHandUseText()
        {
            var protoUsableItem = this.containerHand.GetItemAtSlot(0)?.ProtoItem as IProtoItemUsableFromContainer;
            if (protoUsableItem is null)
            {
                this.ItemInHandUseText = null;
                return;
            }

            this.ItemInHandUseText = protoUsableItem.ItemUseCaption;
        }

        private void RefreshVisibilityHeadAndLegsSlots()
        {
            var chestItem = this.ContainerEquipment.GetItemAtSlot((byte)EquipmentType.Armor);
            var hasFullBodyArmor = chestItem is not null
                                   && chestItem.ProtoGameObject is IProtoItemEquipment protoEquipment
                                   && protoEquipment.EquipmentType == EquipmentType.FullBody;

            // hide head and legs slot when full body armor is equipped
            this.IsHeadSlotBlocked = hasFullBodyArmor;

            var hasHeadEquipment = this.ContainerEquipment.GetItemAtSlot((byte)EquipmentType.Head) is not null;
            this.HasHeadEquipmentOrFullBodyArmorEquipped = hasFullBodyArmor || hasHeadEquipment;
        }
    }
}