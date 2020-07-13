namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Items;

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

        public static void Init(IClientItemsContainer[] containers)
        {
            SubscribedContainers = containers;
            ContainersItemsReset?.Invoke();
        }

        public static void Reset()
        {
            if (SubscribedContainers.Length == 0)
            {
                return;
            }

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