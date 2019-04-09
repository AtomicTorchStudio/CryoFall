namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientCurrentCharacterContainersHelper
    {
        private static ICharacter currentCharacter;

        private static List<IClientItemsContainer> subscribedContainers;

        public static event Action ContainersItemsReset;

        public static event Action<IItem> ItemAddedOrRemovedOrCountChanged;

        private static List<IClientItemsContainer> SubscribedContainers
        {
            get => subscribedContainers;
            set
            {
                if (subscribedContainers == value)
                {
                    return;
                }

                if (subscribedContainers != null)
                {
                    // unsubscribe
                    foreach (var container in subscribedContainers)
                    {
                        container.ItemAdded -= ContainerItemAddedHandler;
                        container.ItemRemoved -= ContainerItemRemovedHandler;
                        container.ItemCountChanged -= ContainerItemCountChangedHandler;
                        container.ItemsReset -= ContainerItemsResetHandler;
                    }
                }

                subscribedContainers = value;

                if (subscribedContainers != null)
                {
                    // subscribe
                    foreach (var container in subscribedContainers)
                    {
                        container.ItemAdded += ContainerItemAddedHandler;
                        container.ItemRemoved += ContainerItemRemovedHandler;
                        container.ItemCountChanged += ContainerItemCountChangedHandler;
                        container.ItemsReset += ContainerItemsResetHandler;
                    }
                }
            }
        }

        public static void Init(ICharacter newCurrentCharacter)
        {
            if (currentCharacter != null)
            {
                throw new Exception("Already initialized");
            }

            currentCharacter = newCurrentCharacter;

            var character = Api.Client.Characters.CurrentPlayerCharacter;
            SubscribedContainers = new List<IClientItemsContainer>()
            {
                (IClientItemsContainer)character.SharedGetPlayerContainerInventory(),
                (IClientItemsContainer)character.SharedGetPlayerContainerHotbar()
            };

            ContainersItemsReset?.Invoke();
        }

        public static void Reset()
        {
            if (currentCharacter == null)
            {
                return;
            }

            currentCharacter = null;
            SubscribedContainers = new List<IClientItemsContainer>(capacity: 0);
            ContainersItemsReset?.Invoke();
        }

        private static void ContainerItemAddedHandler(IItem item)
        {
            ItemAddedOrRemovedOrCountChanged?.Invoke(item);
        }

        private static void ContainerItemCountChangedHandler(IItem item, ushort oldcount, ushort value)
        {
            ItemAddedOrRemovedOrCountChanged?.Invoke(item);
        }

        private static void ContainerItemRemovedHandler(IItem item, byte slotId)
        {
            ItemAddedOrRemovedOrCountChanged?.Invoke(item);
        }

        private static void ContainerItemsResetHandler()
        {
            ContainersItemsReset?.Invoke();
        }
    }
}