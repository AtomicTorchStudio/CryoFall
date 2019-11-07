namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    public class ViewModelItemsContainerExchange : BaseViewModel
    {
        private readonly Action callbackTakeAllItemsSuccess;

        private readonly bool enableShortcuts;

        private IClientItemsContainer container;

        private ClientInputContext inputListener;

        private bool isActive;

        public ViewModelItemsContainerExchange(
            IItemsContainer container,
            [CanBeNull] Action callbackTakeAllItemsSuccess,
            bool enableShortcuts = true)
        {
            this.callbackTakeAllItemsSuccess = callbackTakeAllItemsSuccess;
            this.Container = (IClientItemsContainer)container;

            this.enableShortcuts = enableShortcuts;
        }

        public BaseCommand CommandMatch => new ActionCommandWithParameter(this.ExecuteCommandMatch);

        public BaseCommand CommandOpenHelpMenu
            => WindowContainerHelp.CommandOpenMenu;

        public BaseCommand CommandTakeAll => new ActionCommand(this.ExecuteCommandTakeAll);

        public IClientItemsContainer Container
        {
            get => this.container;
            set
            {
                this.SetProperty(ref this.container, value);
                this.ContainerTitle = value?.Owner?.ProtoGameObject.Name.ToUpperInvariant() ?? string.Empty;
            }
        }

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public string ContainerTitle { get; set; } = "Container title";

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
                this.NotifyThisPropertyChanged();

                if (!this.enableShortcuts)
                {
                    return;
                }

                if (this.isActive)
                {
                    // setup shortcuts
                    var character = ClientCurrentCharacterHelper.Character;
                    ClientContainersExchangeManager.Register(
                        this,
                        this.Container,
                        allowedTargets: new[]
                        {
                            character.SharedGetPlayerContainerInventory(),
                            character.SharedGetPlayerContainerHotbar()
                        });

                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    this.inputListener = ClientInputContext
                                         .Start("Container exchange")
                                         .HandleButtonDown(GameButton.ContainerTakeAll, this.ExecuteCommandTakeAll)
                                         .HandleButtonDown(GameButton.ContainerMoveItemsMatchDown,
                                                           this.ExecuteCommandMatchDown)
                                         .HandleButtonDown(GameButton.ContainerMoveItemsMatchUp,
                                                           this.ExecuteCommandMatchUp);
                }
                else
                {
                    ClientContainersExchangeManager.Unregister(this);
                    this.inputListener.Stop();
                    this.inputListener = null;
                }
            }
        }

        public bool IsContainerTitleVisible { get; set; } = true;

        public bool IsManagementButtonsVisible { get; set; } = true;

        public void ExecuteCommandMatch(bool isUp)
        {
            var scriptingApi = Api.Client;
            var itemsClientService = scriptingApi.Items;
            var character = scriptingApi.Characters.CurrentPlayerCharacter;
            var playerState = PlayerCharacter.GetPrivateState(character);
            var containerInventory = playerState.ContainerInventory;
            var containerHotbar = playerState.ContainerHotbar;

            IItemsContainer toContainer1, toContainer2;
            IEnumerable<IItem> sourceItems;

            if (isUp)
            {
                // move items "up" - from player inventory to this crate container
                sourceItems = containerInventory.Items; //.Concat(containerHotbar.Items);
                toContainer1 = this.Container;
                toContainer2 = null;

                ClientContainerSortHelper.ConsolidateItemStacks((IClientItemsContainer)toContainer1);
            }
            else
            {
                // move items "down" - from this crate container to player containers
                sourceItems = this.Container.Items;
                toContainer1 = containerInventory;
                toContainer2 = containerHotbar;
            }

            var itemTypesToMove = new HashSet<IProtoItem>(toContainer1.Items.Select(i => i.ProtoItem));
            if (toContainer2 != null)
            {
                itemTypesToMove.AddRange(toContainer2.Items.Select(i => i.ProtoItem));
            }

            var itemsToMove = sourceItems
                              .Where(item => itemTypesToMove.Contains(item.ProtoItem))
                              .OrderBy(i => i.ProtoItem.Id)
                              .ToList();

            var isAtLeastOneItemMoved = false;
            foreach (var itemToMove in itemsToMove)
            {
                if (itemsClientService.MoveOrSwapItem(
                    itemToMove,
                    toContainer1,
                    allowSwapping: false,
                    isLogErrors: false))
                {
                    isAtLeastOneItemMoved = true;
                    continue;
                }

                if (toContainer2 != null
                    && itemsClientService.MoveOrSwapItem(
                        itemToMove,
                        toContainer2,
                        allowSwapping: false,
                        isLogErrors: false))
                {
                    isAtLeastOneItemMoved = true;
                    continue;
                }
            }

            if (isAtLeastOneItemMoved)
            {
                ItemsSoundPresets.ItemGeneric.PlaySound(ItemSound.Drop);
            }

            if (!isUp
                && itemsToMove.Any(i => i.Container == this.Container))
            {
                // at least one item stuck in the container when matching down
                // it means there are not enough space
                NotificationSystem.ClientShowNotificationNoSpaceInInventory();
            }
        }

        public void ExecuteCommandTakeAll()
        {
            this.ExecuteCommandMatchDown();

            var scriptingApi = Api.Client;
            var character = scriptingApi.Characters.CurrentPlayerCharacter;
            character.ProtoCharacter.ClientTryTakeAllItems(character,
                                                           this.Container,
                                                           showNotificationIfInventoryFull: true);
            if (this.container.OccupiedSlotsCount == 0)
            {
                this.callbackTakeAllItemsSuccess?.Invoke();
            }
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.inputListener?.Stop();
            this.inputListener = null;
        }

        private void ExecuteCommandMatch(object direction)
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            var isUp = ((string)direction).Equals("Up", StringComparison.Ordinal);
            this.ExecuteCommandMatch(isUp);
        }

        private void ExecuteCommandMatchDown()
        {
            this.ExecuteCommandMatch(isUp: false);
        }

        private void ExecuteCommandMatchUp()
        {
            this.ExecuteCommandMatch(isUp: true);
        }
    }
}