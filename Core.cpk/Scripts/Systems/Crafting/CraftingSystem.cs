namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;

    /// <summary>
    /// Character item crafting system - processes client requests to manipulate crafting queue.
    /// </summary>
    public class CraftingSystem : ProtoSystem<CraftingSystem>
    {
        public const string NotificationCraftingQueueFull_Message = "The crafting queue is full.";

        public const string NotificationCraftingQueueFull_Title = "Cannot add new recipe";

        public static ushort ClientCurrentMaxCraftingQueueEntriesCount
            => SharedGetMaxCraftingQueueEntriesCount(Client.Characters.CurrentPlayerCharacter);

        public override string Name => "Crafting system";

        public static void ClientDeleteQueueItem(CraftingQueueItem craftingQueueItem)
        {
            Instance.CallServer(_ => _.ServerRemote_CancelQueueItem(craftingQueueItem.LocalId));
        }

        public static void ClientMakeItemFirstInQueue(CraftingQueueItem craftingQueueItem)
        {
            Instance.CallServer(_ => _.ServerRemote_MakeFirstInQueue(craftingQueueItem.LocalId));
        }

        public static async Task ClientStartCrafting(Recipe recipe, ushort countToCraft)
        {
            if (SharedValidateQueueIsNotFull(Client.Characters.CurrentPlayerCharacter,
                                             recipe,
                                             countToCraft,
                                             maxCraftingQueueEntriesCount:
                                             ClientCurrentMaxCraftingQueueEntriesCount))
            {
                await Instance.CallServer(_ => _.ServerRemote_CraftRecipe(recipe, countToCraft));
            }
        }

        public static IStaticWorldObject SharedFindNearbyStationOfTypes(
            IReadOnlyStationsList stationTypes,
            ICharacter character)
        {
            using var objectsInCharacterInteractionArea =
                InteractionCheckerSystem.SharedGetTempObjectsInCharacterInteractionArea(character);
            if (objectsInCharacterInteractionArea is null)
            {
                return null;
            }

            foreach (var testResult in objectsInCharacterInteractionArea.AsList())
            {
                var worldObject = testResult.PhysicsBody.AssociatedWorldObject as IStaticWorldObject;
                if (worldObject is null
                    || !stationTypes.Contains(worldObject.ProtoWorldObject))
                {
                    continue;
                }

                if (!worldObject.ProtoWorldObject.SharedCanInteract(character, worldObject, writeToLog: false))
                {
                    continue;
                }

                // found station with which player can interact
                return worldObject;
            }

            return null;
        }

        public static ushort SharedGetMaxCraftingQueueEntriesCount(ICharacter character)
        {
            var maxCraftingQueueEntriesCount = (ushort)Math.Round(
                character.SharedGetFinalStatValue(StatName.CraftingQueueMaxSlotsCount),
                MidpointRounding.AwayFromZero);
            return maxCraftingQueueEntriesCount;
        }

        public bool ServerRemote_CraftRecipe(Recipe recipe, ushort countToCraft)
        {
            var character = ServerRemoteContext.Character;
            var characterServerState = PlayerCharacter.GetPrivateState(character);

            IStaticWorldObject station;
            var craftingQueue = characterServerState.CraftingQueue;

            switch (recipe)
            {
                case Recipe.RecipeForHandCrafting:
                    // simply craft by character
                    station = null;
                    break;

                case Recipe.RecipeForStationCrafting recipeForStation:
                    station = SharedFindNearbyStationOfTypes(recipeForStation.StationTypes, character);
                    if (station is null)
                    {
                        Logger.Error(
                            $"No crafting stations of types {recipeForStation.StationTypes.GetJoinedString()} found nearby character {character} at position {character.Position}");
                        return false;
                    }

                    break;

                default:
                    throw new Exception("Incorrect recipe for in-hand or station crafting: " + recipe);
            }

            // extra check (it's also done in the recipe itself)
            if (!recipe.SharedIsTechUnlocked(character))
            {
                // locked recipe
                return false;
            }

            var maxCraftingQueueEntriesCount = SharedGetMaxCraftingQueueEntriesCount(character);

            if (recipe.OutputItems.Items[0].ProtoItem.IsStackable)
            {
                // stackable items
                if (!SharedValidateQueueIsNotFull(character, recipe, countToCraft, maxCraftingQueueEntriesCount))
                {
                    return false;
                }

                CraftingMechanics.ServerStartCrafting(station,
                                                      character,
                                                      craftingQueue,
                                                      recipe,
                                                      countToCraft,
                                                      maxQueueSize: maxCraftingQueueEntriesCount);
            }
            else
            {
                // non-stackable items
                countToCraft = MathHelper.Clamp(countToCraft,
                                                min: (ushort)1,
                                                max: maxCraftingQueueEntriesCount);
                for (var i = 0; i < countToCraft; i++)
                {
                    if (!SharedValidateQueueIsNotFull(character,
                                                      recipe,
                                                      countToCraft: 1,
                                                      maxCraftingQueueEntriesCount))
                    {
                        return false;
                    }

                    CraftingMechanics.ServerStartCrafting(station,
                                                          character,
                                                          craftingQueue,
                                                          recipe,
                                                          countToCraft: 1,
                                                          maxQueueSize: maxCraftingQueueEntriesCount);
                }
            }

            return true;
        }

        private static void ClientShowNotificationCraftingQueueIsFull()
        {
            NotificationSystem.ClientShowNotification(
                NotificationCraftingQueueFull_Title,
                NotificationCraftingQueueFull_Message,
                color: NotificationColor.Bad);
        }

        private static bool SharedValidateQueueIsNotFull(
            ICharacter character,
            Recipe recipe,
            ushort countToCraft,
            ushort maxCraftingQueueEntriesCount)
        {
            var characterServerState = PlayerCharacter.GetPrivateState(character);
            var craftingQueueItems = characterServerState.CraftingQueue.QueueItems;
            if (craftingQueueItems.Count + 1 <= maxCraftingQueueEntriesCount)
            {
                // allow to add the recipe
                return true;
            }

            foreach (var scheduledQueueItem in craftingQueueItems)
            {
                var macCountToCraft = recipe.OutputItems.Items[0].ProtoItem.MaxItemsPerStack;
                if (scheduledQueueItem.CanCombineWith(recipe)
                    && (int)scheduledQueueItem.CountToCraftRemains + (int)countToCraft <= macCountToCraft)
                {
                    // allow to increase already queued recipe's count
                    return true;
                }
            }

            Logger.Info("Crafting queue is full", character);

            if (IsClient)
            {
                ClientShowNotificationCraftingQueueIsFull();
            }

            return false;
        }

        private void ServerRemote_CancelQueueItem(ushort localId)
        {
            var character = ServerRemoteContext.Character;
            var characterServerState = PlayerCharacter.GetPrivateState(character);

            var queue = characterServerState.CraftingQueue.QueueItems;
            for (var index = 0; index < queue.Count; index++)
            {
                var item = queue[index];
                if (item.LocalId == localId)
                {
                    CraftingMechanics.ServerCancelCraftingQueueItem(character, item);
                    return;
                }
            }

            Logger.Warning("Cannot find crafting queue entry with localId=" + localId);
        }

        private void ServerRemote_MakeFirstInQueue(ushort localId)
        {
            var character = ServerRemoteContext.Character;
            var characterServerState = PlayerCharacter.GetPrivateState(character);

            var queue = characterServerState.CraftingQueue.QueueItems;
            for (var index = 0; index < queue.Count; index++)
            {
                var item = queue[index];
                if (item.LocalId != localId)
                {
                    continue;
                }

                if (index == 0)
                {
                    // already first in queue
                    return;
                }

                queue.RemoveAt(index);
                queue.Insert(0, item);
                characterServerState.CraftingQueue.SetDurationFromCurrentRecipe();
                //Logger.Info("Reordered crafting queue entry to make the recipe first: " + item.Recipe, character);
                return;
            }

            Logger.Warning("Cannot find crafting queue entry with localId=" + localId);
        }
    }
}