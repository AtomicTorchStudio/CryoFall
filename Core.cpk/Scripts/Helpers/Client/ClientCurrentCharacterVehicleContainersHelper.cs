namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static class ClientCurrentCharacterVehicleContainersHelper
    {
        private static IClientItemsContainer[] subscribedContainers
            = Array.Empty<IClientItemsContainer>();

        public static event Action ContainersItemsReset;

        public static event Action<IItem> ItemAddedOrRemovedOrCountChanged;

        private static IClientItemsContainer[] SubscribedContainers
        {
            get => subscribedContainers;
            set
            {
                if (subscribedContainers == value)
                {
                    return;
                }

                if (subscribedContainers is not null)
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

                if (subscribedContainers is not null)
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

        public static void Init(IClientItemsContainer[] containers)
        {
            //Api.Logger.Dev("Init vehicle containers: " + containers.GetJoinedString());
            SubscribedContainers = containers;
            ContainersItemsReset?.Invoke();
        }

        public static void Reset(IClientItemsContainer[] clientItemsContainers)
        {
            if (!SubscribedContainers.SequenceEqual(clientItemsContainers))
            {
                return;
            }

            //Api.Logger.Dev("Reset vehicle containers: " + SubscribedContainers.GetJoinedString());
            SubscribedContainers = Array.Empty<IClientItemsContainer>();
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