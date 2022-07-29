namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.ItemFreshnessSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    /// <summary>
    /// Mechanic for items crafting (crafting queue manipulation and processing).
    /// </summary>
    public static class CraftingMechanics
    {
        private static readonly ILogger Logger = Api.Logger;

        public delegate void RecipeCraftedDelegate(CraftingQueueItem craftingQueueItem);

        public static event RecipeCraftedDelegate ServerNonManufacturingRecipeCrafted;

        /// <summary>
        /// Return all input items to player - or drop on the ground
        /// </summary>
        /// <param name="character"></param>
        /// <param name="craftingQueue"></param>
        public static void ServerCancelCraftingQueue(ICharacter character)
        {
            var craftingQueue = PlayerCharacter.GetPrivateState(character)
                                               .CraftingQueue;
            if (craftingQueue.QueueItems.Count == 0)
            {
                return;
            }

            IItemsContainer groundContainer = null;
            var queueItems = craftingQueue.QueueItems.ToList();
            foreach (var item in queueItems)
            {
                ServerCancelCraftingQueueItem(character,
                                              item,
                                              craftingQueue,
                                              ref groundContainer);
            }

            Logger.Info("Crafting queue successfully cancelled", character);
        }

        /// <summary>
        /// Cancel crafting queue item.
        /// </summary>
        public static void ServerCancelCraftingQueueItem(
            ICharacter character,
            CraftingQueueItem item)
        {
            var craftingQueue = PlayerCharacter.GetPrivateState(character).CraftingQueue;

            IItemsContainer groundContainer = null;
            ServerCancelCraftingQueueItem(character,
                                          item,
                                          craftingQueue,
                                          ref groundContainer);

            if (groundContainer is not null)
            {
                NotificationSystem.ServerSendNotificationNoSpaceInInventoryItemsDroppedToGround(
                    character,
                    protoItemForIcon: null);
            }
        }

        public static void ServerRecalculateTimeToFinish(CraftingQueue craftingQueue)
        {
            craftingQueue.ServerRecalculateTimeToFinish();
        }

        /// <summary>
        /// Enqueue crafting selected recipe.
        /// </summary>
        /// <param name="craftingQueue">Crafting queue instance.</param>
        /// <param name="recipeEntry">Recipe instance.</param>
        /// <param name="countToCraft">Count to craft - must be greater than zero.</param>
        public static void ServerStartCrafting(
            [CanBeNull] IStaticWorldObject station,
            [CanBeNull] ICharacter character,
            [NotNull] CraftingQueue craftingQueue,
            [NotNull] RecipeWithSkin recipeEntry,
            ushort countToCraft,
            ushort? maxQueueSize = null)
        {
            if (station is null
                && character is null)
            {
                throw new NullReferenceException("Character AND station cannot be null simultaneously");
            }

            if (countToCraft == 0)
            {
                throw new Exception("Are you really want to craft zero items?");
            }

            var isAdminMode = character is not null
                              && CreativeModeSystem.SharedIsInCreativeMode(character);

            if (!isAdminMode
                && recipeEntry.Recipe.RecipeType != RecipeType.ManufacturingByproduct
                && !recipeEntry.Recipe.CanBeCrafted(character,
                                               station,
                                               craftingQueue,
                                               countToCraft: recipeEntry.Recipe.RecipeType == RecipeType.Manufacturing
                                                                 ? (ushort)1
                                                                 : countToCraft))
            {
                Logger.Error($"Recipe cannot be crafted - check failed: {recipeEntry} at {station}.", character);
                return;
            }

            var queueCount = craftingQueue.QueueItems.Count;
            if (queueCount > 0)
            {
                foreach (var existingQueueItem in craftingQueue.QueueItems)
                {
                    if (!existingQueueItem.CanCombineWith(recipeEntry))
                    {
                        continue;
                    }

                    // try to increase the count to craft
                    var lastQueueItemCountBefore = existingQueueItem.CountToCraftRemains;

                    var maxCountToCraft = ushort.MaxValue;
                    var isManufacturingRecipe = recipeEntry.Recipe.RecipeType != RecipeType.Hand
                                                && recipeEntry.Recipe.RecipeType != RecipeType.StationCrafting;

                    if (!isManufacturingRecipe)
                    {
                        maxCountToCraft = recipeEntry.Recipe.OutputItems.Items[0].ProtoItem.MaxItemsPerStack;
                    }

                    var originalRequestedCountToCraft = countToCraft;
                    var lastQueueItemCountNew =
                        (ushort)Math.Min(maxCountToCraft, lastQueueItemCountBefore + countToCraft);
                    var canAddCount = lastQueueItemCountNew - lastQueueItemCountBefore;

                    if (canAddCount > 0)
                    {
                        existingQueueItem.CountToCraftRemains = lastQueueItemCountNew;
                        Logger.Info(
                            $"Recipe count extended for crafting: {recipeEntry} at {station}: from x{lastQueueItemCountBefore} to x{countToCraft}",
                            character);

                        ServerDestroyInputItems(craftingQueue,
                                                recipeEntry.Recipe,
                                                countToCraft: (ushort)canAddCount,
                                                isAdminMode);

                        if (isManufacturingRecipe)
                        {
                            return;
                        }

                        var remainingCountToCraft = originalRequestedCountToCraft
                                                    - canAddCount;
                        if (remainingCountToCraft <= 0)
                        {
                            // the last queue item took all the requested items count to craft
                            return;
                        }

                        // last queue item cannot accomodate all the requested count to craft
                        // let's try to add to previous queue item or a new queue item
                        countToCraft = (ushort)remainingCountToCraft;
                    }
                }
            }

            if (maxQueueSize.HasValue
                && craftingQueue.QueueItems.Count >= maxQueueSize.Value)
            {
                Logger.Info(
                    $"Recipe cannot be queue for crafting due to max queue size limitation: {recipeEntry} at {station} with max queue size {maxQueueSize.Value}.",
                    character);
                return;
            }

            var queueItem = new CraftingQueueItem(recipeEntry, countToCraft, craftingQueue.ServerLastQueueItemLocalId++);
            ServerDestroyInputItems(craftingQueue,
                                    recipeEntry.Recipe,
                                    countToCraft: queueItem.CountToCraftRemains,
                                    isAdminMode);

            craftingQueue.QueueItems.Add(queueItem);
            if (craftingQueue.QueueItems.Count == 1)
            {
                // the added recipe is first in queue - so we will craft it right now
                craftingQueue.SetDurationFromCurrentRecipe();
            }

            Logger.Info($"Recipe queued for crafting: {recipeEntry} at {station}.", character);
        }

        /// <summary>
        /// Update crafting queue. It will progress current crafting queue item.
        /// </summary>
        /// <param name="craftingQueue">Crafting queue item.</param>
        /// <param name="deltaTime">Delta time in seconds to progress on.</param>
        public static void ServerUpdate(CraftingQueue craftingQueue, double deltaTime)
        {
            do
            {
                var queue = craftingQueue.QueueItems;
                if (queue.Count == 0)
                {
                    // the queue is empty
                    return;
                }

                var queueItem = queue[0];
                ServerProgressQueueItem(craftingQueue, queueItem, ref deltaTime);
            }
            while (deltaTime > 0
                   && !craftingQueue.IsContainerOutputFull);
        }

        private static void ServerCancelCraftingQueueItem(
            ICharacter character,
            CraftingQueueItem item,
            CraftingQueue craftingQueue,
            ref IItemsContainer groundContainer)
        {
            if (!item.RecipeEntry.Recipe.IsCancellable)
            {
                Logger.Info("Crafting queue item is not cancellable: " + item, character);
                return;
            }

            var index = craftingQueue.QueueItems.IndexOf(item);
            if (index < 0)
            {
                throw new Exception("Doesn't have the crafting queue item: " + item);
            }

            ServerReturnCharacterCraftingQueueItemInputItems(character, item, ref groundContainer);
            craftingQueue.QueueItems.RemoveAt(index);

            if (index == 0)
            {
                // first queue item was removed - reset crafting duration
                craftingQueue.SetDurationFromCurrentRecipe();
            }

            Logger.Info("Crafting queue item successfully cancelled: " + item, character);
        }

        private static void ServerDestroyInputItems(
            Recipe recipe,
            IItemsContainerProvider inputContainers,
            ushort countToCraft,
            bool isCreativeMode)
        {
            foreach (var inputItem in recipe.InputItems)
            {
                var countToDestroy = (ushort)(inputItem.Count * countToCraft);
                var inputItemProto = inputItem.ProtoItem;

                if (isCreativeMode)
                {
                    CreativeModeAdjustCountToDestroy(inputItemProto, ref countToDestroy);
                    if (countToDestroy == 0)
                    {
                        continue;
                    }
                }

                ServerDestroyItemsOfType(inputContainers,
                                         inputItemProto,
                                         countToDestroy,
                                         out _);
            }

            void CreativeModeAdjustCountToDestroy(IProtoItem inputItemProto, ref ushort countToDestroy)
            {
                // destroy only available count
                var availableCount = 0;
                foreach (var c in inputContainers.Containers)
                {
                    foreach (var item in c.Items)
                    {
                        if (ReferenceEquals(item.ProtoItem, inputItemProto))
                        {
                            availableCount += item.Count;
                        }
                    }
                }

                if (availableCount == 0)
                {
                    // nothing to destroy
                    countToDestroy = 0;
                    return;
                }

                if (countToDestroy > availableCount)
                {
                    // limit count to destroy
                    countToDestroy = (ushort)availableCount;
                }
            }
        }

        private static void ServerDestroyInputItems(
            CraftingQueue craftingQueue,
            Recipe recipe,
            ushort countToCraft,
            bool isCreativeMode)
        {
            switch (recipe.RecipeType)
            {
                case RecipeType.Manufacturing:
                    // auto-manufacturers: do not destroy input items until crafting completed
                    break;

                case RecipeType.ManufacturingByproduct:
                    // station by-products doesn't require input items - it checks only for proper item prototype upon start
                    break;

                default:
                    // other recipes should instantly destroy the input items
                    ServerDestroyInputItems(recipe,
                                            new AggregatedItemsContainers(craftingQueue.InputContainersArray),
                                            countToCraft,
                                            isCreativeMode);
                    break;
            }
        }

        private static void ServerDestroyItemsOfType(
            IItemsContainerProvider containers,
            IProtoItem protoItem,
            uint countToDestroy,
            out uint destroyedCount)
        {
            var serverItemsService = Api.Server.Items;
            if (protoItem is not IProtoItemWithFreshness)
            {
                // for items without freshness use the default approach
                serverItemsService.DestroyItemsOfType(containers,
                                                      protoItem,
                                                      countToDestroy,
                                                      out destroyedCount);
                return;
            }

            // for items with freshness, gather all the available items and then sort them by freshness
            // prefer items with lower freshness first
            destroyedCount = 0;

            using var tempAllItemsOfType = Api.Shared.GetTempList<IItem>();
            var allItemsOfType = tempAllItemsOfType.AsList();

            foreach (var container in containers.Containers)
            {
                foreach (var item in container.Items)
                {
                    if (ReferenceEquals(item.ProtoItem, protoItem))
                    {
                        allItemsOfType.Add(item);
                    }
                }
            }

            SortItems(protoItem, allItemsOfType);

            // loop from the slot with lowest freshness to the slow with the higher freshness
            // (if the freshness it the same, it should go right-to-left unless we got a sorting issue)
            for (var index = allItemsOfType.Count - 1; index >= 0; index--)
            {
                var item = allItemsOfType[index];
                if (item.Count >= countToDestroy)
                {
                    // remove full remained count
                    destroyedCount += countToDestroy;
                    serverItemsService.SetCount(item, (ushort)(item.Count - countToDestroy));
                    countToDestroy = 0;
                    return;
                }

                // remove as much as we can
                destroyedCount += item.Count;
                countToDestroy -= item.Count;
                serverItemsService.DestroyItem(item);
            }

            if (destroyedCount != countToDestroy)
            {
                Api.Logger.Error(
                    $"Cannot remove all required to remove count. Containers {containers.Containers.GetJoinedString()}, item type {protoItem}, count destroyed {destroyedCount}, count to destroy remains {countToDestroy - destroyedCount}");
            }

            static void SortItems(IProtoItem protoItem, List<IItem> items)
            {
                if (protoItem is not IProtoItemWithFreshness protoItemWithFreshness
                    || protoItemWithFreshness.FreshnessMaxValue == 0)
                {
                    return;
                }

                // sort by freshness
                items.Sort(ItemFreshnessSystem.SharedCompareFreshnessReverse);
            }
        }

        private static void ServerProgressQueueItem(
            CraftingQueue craftingQueue,
            CraftingQueueItem queueItem,
            ref double deltaTime)
        {
            if (craftingQueue.TimeRemainsToComplete > deltaTime)
            {
                // consume deltaTime
                craftingQueue.TimeRemainsToComplete -= deltaTime;
                deltaTime = 0;
                // need more time to complete crafting
                return;
            }

            // crafting of item completed
            deltaTime -= craftingQueue.TimeRemainsToComplete;
            craftingQueue.TimeRemainsToComplete = 0;

            if (!ServerTryCreateOutputItems(craftingQueue, queueItem))
            {
                // need more space in output container
                return;
            }

            var craftedCount = 1;
            var countToCraftRemains = 0;
            
            var recipeEntry = queueItem.RecipeEntry;
            if (recipeEntry.Recipe.RecipeType 
                is RecipeType.StationCrafting
                or RecipeType.Hand)
            {
                countToCraftRemains = Math.Min(queueItem.CountToCraftRemains - 1,
                                               (int)(RateCraftingSpeedMultiplier.SharedValue - 1));
            }

            while (countToCraftRemains > 0)
            {
                if (!ServerTryCreateOutputItems(craftingQueue, queueItem))
                {
                    break;
                }

                craftedCount++;
                countToCraftRemains--;
            }

            Logger.Info($"Crafting of {queueItem} completed.");
            if (recipeEntry.Recipe.RecipeType == RecipeType.Manufacturing)
            {
                // auto-manufacturers: destroy input items when crafting completed
                ServerDestroyInputItems(
                    recipeEntry.Recipe,
                    new AggregatedItemsContainers(craftingQueue.InputContainersArray),
                    countToCraft: 1,
                    isCreativeMode: false);
            }

            for (var index = 0; index < craftedCount; index++)
            {
                Api.SafeInvoke(() => ServerNonManufacturingRecipeCrafted?.Invoke(queueItem));
            }

            if (recipeEntry.Recipe.RecipeType
                is RecipeType.Manufacturing
                or RecipeType.ManufacturingByproduct)
            {
                // do not reduce count to craft as this is a manufacturing recipe
                var station = craftingQueue.GameObject as IStaticWorldObject;
                if (recipeEntry.Recipe.RecipeType == RecipeType.Manufacturing
                    && !recipeEntry.Recipe.CanBeCrafted(character: null,
                                                        station,
                                                        craftingQueue,
                                                        countToCraft: 1))
                {
                    Logger.Info(
                        $"Manufacturing recipe cannot be crafted anymore - check failed: {recipeEntry} at {station}.");
                    // no need to set this to 0 (as it will cause unnecessary network sync)
                    // simply remove it from queue
                    //queueItem.CountToCraftRemains = 0;
                    craftingQueue.QueueItems.Remove(queueItem);
                }
            }
            else // if this is a non-manufacturing recipe
            {
                if (queueItem.CountToCraftRemains > craftedCount)
                {
                    queueItem.CountToCraftRemains = (ushort)(queueItem.CountToCraftRemains - craftedCount);
                }
                else
                {
                    // remove it from queue
                    craftingQueue.QueueItems.Remove(queueItem);
                }
            }

            craftingQueue.SetDurationFromCurrentRecipe();
        }

        private static void ServerReturnCharacterCraftingQueueItemInputItems(
            ICharacter character,
            CraftingQueueItem queueItem,
            ref IItemsContainer groundContainer)
        {
            foreach (var inputItem in queueItem.RecipeEntry.Recipe.InputItems)
            {
                var countToSpawn = queueItem.CountToCraftRemains * (uint)inputItem.Count;
                PlayerCharacter.ServerTrySpawnItemToCharacterOrGround(character,
                                                                      inputItem.ProtoItem,
                                                                      countToSpawn,
                                                                      ref groundContainer);
            }
        }

        private static bool ServerTryCreateOutputItems(CraftingQueue craftingQueue, CraftingQueueItem queueItem)
        {
            if (craftingQueue.IsContainerOutputFull
                && craftingQueue.ContainerOutputLastStateHash == craftingQueue.ContainerOutput.StateHash)
            {
                return false;
            }

            var recipeEntry = queueItem.RecipeEntry;
            var result = recipeEntry.Recipe
                               .GetOutputItems(recipeEntry.ProtoItemSkinOverride)
                               .TrySpawnToContainer(craftingQueue.ContainerOutput);

            if (!result.IsEverythingCreated)
            {
                // cannot create items at specified container
                result.Rollback();
                craftingQueue.IsContainerOutputFull = true;
                craftingQueue.ContainerOutputLastStateHash = craftingQueue.ContainerOutput.StateHash;
                return false;
            }

            // something is created, assume crafting success
            craftingQueue.IsContainerOutputFull = false;
            craftingQueue.ContainerOutputLastStateHash = craftingQueue.ContainerOutput.StateHash;

            if (recipeEntry.Recipe is Recipe.RecipeForManufacturing recipeManufacturing)
            {
                recipeManufacturing.ServerOnManufacturingCompleted(
                    ((ManufacturingCraftingQueue)craftingQueue).WorldObject,
                    craftingQueue);
            }

            if (craftingQueue is CharacterCraftingQueue characterCraftingQueue)
            {
                var character = characterCraftingQueue.Character;
                foreach (var item in result.ItemAmounts.Keys)
                {
                    if (((IProtoItemWithSkinData)item.ProtoItem).BaseProtoItem is not null)
                    {
                        // record who the item was crafted by
                        // (a delay is required for proper synchronization of the change with the client)
                        ServerTimersSystem.AddAction(
                            delaySeconds: 0.1,
                            () => item.GetPrivateState<ItemPrivateState>()
                                      .CreatedByPlayerName = character.Name);
                    }
                }

                /*NotificationSystem.ServerSendItemsNotification(
                    character,
                    result);*/
            }

            return true;
        }
    }
}