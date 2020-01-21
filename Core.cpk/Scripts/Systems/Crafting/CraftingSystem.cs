namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
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

        public static readonly double ServerCraftingSpeedMultiplier;

        // actual value is received from the server by bootstrapper
        public static double ClientCraftingSpeedMultiplier = 1.0;

        static CraftingSystem()
        {
            ServerCraftingSpeedMultiplier = ServerRates.Get(
                "CraftingSpeedMultiplier",
                defaultValue: 1.0,
                @"This rate determines the crafting speed of recipes
                  started from crafting menu or from any workbench.
                  Does NOT apply to manufacturing structures (such as furnace) - edit ManufacturingSpeedMultiplier for these.");
        }

        public static event Action ClientCraftingSpeedMultiplierChanged;

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

        public static void ClientSetLearningPointsGainMultiplier(double rate)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (ClientCraftingSpeedMultiplier == rate)
            {
                return;
            }

            ClientCraftingSpeedMultiplier = rate;
            Api.SafeInvoke(ClientCraftingSpeedMultiplierChanged);
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
            if (objectsInCharacterInteractionArea == null)
            {
                return null;
            }

            foreach (var testResult in objectsInCharacterInteractionArea.AsList())
            {
                var worldObject = testResult.PhysicsBody.AssociatedWorldObject as IStaticWorldObject;
                if (worldObject == null
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
            if (recipe.RecipeType == RecipeType.Hand)
            {
                // simply craft by character
                station = null;
            }
            else
            {
                var recipeForStation = (Recipe.BaseRecipeForStation)recipe;
                station = SharedFindNearbyStationOfTypes(recipeForStation.StationTypes, character);
                if (station == null)
                {
                    Logger.Error(
                        $"No crafting stations of types {recipeForStation.StationTypes.GetJoinedString()} found nearby character {character} at position {character.Position}");
                    return false;
                }

                if (recipeForStation is Recipe.RecipeForManufacturing)
                {
                    // manufacture on station
                    throw new Exception("Cannot craft in hand recipe for station manufacturing");
                }
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

            var lastRecipe = craftingQueueItems.Last();
            var macCountToCraft = recipe.OutputItems.Items[0].ProtoItem.MaxItemsPerStack;
            if (lastRecipe.CanCombineWith(recipe)
                && (int)lastRecipe.CountToCraftRemains + (int)countToCraft <= macCountToCraft)
            {
                // allow to increase the recipe count
                return true;
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

        private double ServerRemote_RequestLearningPointsGainMultiplierRate()
        {
            return ServerCraftingSpeedMultiplier;
        }

        // This bootstrapper requests ServerCraftingSpeedMultiplier rate value from server.
        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;
                Refresh();

                async void Refresh()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter == null)
                    {
                        return;
                    }

                    var rate = await Instance.CallServer(
                                   _ => _.ServerRemote_RequestLearningPointsGainMultiplierRate());

                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (ClientCraftingSpeedMultiplier == rate)
                    {
                        return;
                    }

                    ClientCraftingSpeedMultiplier = rate;
                    Api.SafeInvoke(ClientCraftingSpeedMultiplierChanged);
                }
            }
        }
    }
}