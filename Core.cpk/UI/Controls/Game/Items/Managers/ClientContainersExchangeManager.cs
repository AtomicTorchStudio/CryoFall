namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.DataStructures;

    public static class ClientContainersExchangeManager
    {
        private static readonly List<IClientItemsContainer> ActiveContainersList
            = new List<IClientItemsContainer>();

        private static readonly Dictionary<IClientItemsContainer, int> ActiveContainersReferencesCount
            = new Dictionary<IClientItemsContainer, int>();

        private static readonly ListDictionary<object, List<WrappedContainer>> ContainersOpenedBy
            = new ListDictionary<object, List<WrappedContainer>>();

        /// <summary>
        /// Returns collection of containers with fast move action available from.
        /// </summary>
        public static List<IClientItemsContainer> GetTargetContainers(IItemsContainer forAbstractContainer)
        {
            if (forAbstractContainer is null)
            {
                return new List<IClientItemsContainer>(0);
            }

            var forContainer = (IClientItemsContainer)forAbstractContainer;
            if (!ActiveContainersList.Contains(forContainer))
            {
                return new List<IClientItemsContainer>(0);
            }

            var result = new List<IClientItemsContainer>();
            var isFound = false;
            foreach (var openedBy in ContainersOpenedBy)
            {
                foreach (var wrappedContainer in openedBy.Value)
                {
                    if (wrappedContainer.Container != forContainer)
                    {
                        continue;
                    }

                    if (!wrappedContainer.IsHasAllowRequirements)
                    {
                        continue;
                    }

                    isFound = true;

                    // add only allowed containers subset
                    result.AddRange(ActiveContainersList.Where(
                                        c => wrappedContainer.IsAllowed(c)));
                }
            }

            if (!isFound)
            {
                // return all containers
                result.AddRange(ActiveContainersList);
            }

            // never return the same container
            result.Remove(forContainer);
            return result;
        }

        public static void Register(
            object key,
            IItemsContainer container,
            IItemsContainer[] allowedTargets = null,
            bool registerAllowedTargetsReverse = true)
        {
            //Api.Logger.Dev(
            //    string.Format("Registering container for exchange:{0}(by {1}){2}{3}Allowed targets: {4}",
            //                  Environment.NewLine,
            //                  key.GetType().Name,
            //                  container,
            //                  Environment.NewLine,
            //                  allowedTargets?.Select(t => t.Id).GetJoinedString()));

            if (!ContainersOpenedBy.TryGetValue(key, out var list))
            {
                list = new List<WrappedContainer>();
                ContainersOpenedBy.Add(key, list);
            }

            var clientItemsContainer = (IClientItemsContainer)container;
            var wrappedContainer = list.FirstOrDefault(p => p.Container == clientItemsContainer);
            if (wrappedContainer is null)
            {
                wrappedContainer = new WrappedContainer(clientItemsContainer);
                list.Add(wrappedContainer);

                if (!ActiveContainersReferencesCount.TryGetValue(clientItemsContainer, out var referencesCount))
                {
                    referencesCount = 1;
                    ActiveContainersList.Add(clientItemsContainer);
                }
                else
                {
                    referencesCount++;
                    // Ensure the order is modified - same as if the container was added for the first time.
                    // Otherwise order of container operations (such as Shift+Click to move item)
                    // might be incorrect in some cases
                    // (such as when inventory opened while crafting menu was opened).
                    ActiveContainersList.Remove(clientItemsContainer);
                    ActiveContainersList.Add(clientItemsContainer);
                }

                ActiveContainersReferencesCount[clientItemsContainer] = referencesCount;
            }

            if (allowedTargets is not null)
            {
                wrappedContainer.AddAllowedTargetContainers(allowedTargets);

                if (registerAllowedTargetsReverse)
                {
                    foreach (var allowedTargetContainer in allowedTargets)
                    {
                        Register(key,
                                 allowedTargetContainer,
                                 new[] { container },
                                 registerAllowedTargetsReverse: false);
                    }
                }
            }
        }

        public static void Reset()
        {
            ActiveContainersReferencesCount.Clear();
            ActiveContainersList.Clear();
            ContainersOpenedBy.Clear();
        }

        public static void Unregister(object key)
        {
            if (!ContainersOpenedBy.TryGetValue(key, out var list))
            {
                return;
            }

            //Api.Logger.Dev($"Registering containers for exchange by {key.GetType().Name}");

            foreach (var wrappedContainer in list)
            {
                var container = wrappedContainer.Container;
                // decrease references count
                var referencesCount = ActiveContainersReferencesCount[container];
                referencesCount--;
                if (referencesCount > 0)
                {
                    // update count
                    ActiveContainersReferencesCount[container] = referencesCount;
                }
                else
                {
                    // remove from dictionary and list
                    ActiveContainersReferencesCount.Remove(container);
                    ActiveContainersList.Remove(container);
                }
            }

            ContainersOpenedBy.Remove(key);
        }

        public class WrappedContainer
        {
            public readonly IClientItemsContainer Container;

            private HashSet<IClientItemsContainer> allowedTargetContainers;

            public WrappedContainer(IClientItemsContainer container)
            {
                this.Container = container;
            }

            public bool IsHasAllowRequirements => this.allowedTargetContainers is not null;

            public void AddAllowedTargetContainers(IItemsContainer[] containersList)
            {
                if (containersList is null
                    || containersList.Length == 0)
                {
                    return;
                }

                this.allowedTargetContainers ??= new HashSet<IClientItemsContainer>();

                foreach (var itemsContainer in containersList)
                {
                    this.allowedTargetContainers.Add((IClientItemsContainer)itemsContainer);
                }
            }

            public bool IsAllowed(IClientItemsContainer clientItemsContainer)
            {
                return this.allowedTargetContainers.Contains(clientItemsContainer);
            }
        }
    }
}