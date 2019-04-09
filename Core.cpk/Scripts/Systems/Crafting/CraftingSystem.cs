namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
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
        public const int MaxCraftingQueueEntriesCount = 6;

        public const string NotificationCraftingQueueFull_Message = "The crafting queue is full.";

        public const string NotificationCraftingQueueFull_Title = "Cannot add new recipe";

        public override string Name => "Crafting system";

        public static IStaticWorldObject SharedFindNearbyStationOfTypes(
            IReadOnlyStationsList stationTypes,
            ICharacter character)
        {
            using (var objectsInCharacterInteractionArea =
                InteractionCheckerSystem.SharedGetTempObjectsInCharacterInteractionArea(character))
            {
                if (objectsInCharacterInteractionArea == null)
                {
                    return null;
                }

                foreach (var testResult in objectsInCharacterInteractionArea)
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
        }

        public void ClientDeleteQueueItem(CraftingQueueItem craftingQueueItem)
        {
            this.CallServer(_ => _.ServerRemote_CancelQueueItem(craftingQueueItem.LocalId));
        }

        public async Task ClientStartCrafting(Recipe recipe, ushort countToCraft)
        {
            if (this.SharedValidateQueueIsNotFull(Client.Characters.CurrentPlayerCharacter, recipe, countToCraft))
            {
                await this.CallServer(_ => _.ServerRemote_CraftRecipe(recipe, countToCraft));
            }
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

          
            if (recipe.OutputItems.Items[0].ProtoItem.IsStackable)
            {
                // stackable items
                if (!this.SharedValidateQueueIsNotFull(character, recipe, countToCraft))
                {
                    return false;
                }

                CraftingMechanics.ServerStartCrafting(station,
                                                      character,
                                                      craftingQueue,
                                                      recipe,
                                                      countToCraft,
                                                      maxQueueSize: MaxCraftingQueueEntriesCount);
            }
            else
            {
                // non-stackable items
                countToCraft = MathHelper.Clamp(countToCraft, min: 1, max: MaxCraftingQueueEntriesCount);
                for (var i = 0; i < countToCraft; i++)
                {
                    if (!this.SharedValidateQueueIsNotFull(character, recipe, countToCraft: 1))
                    {
                        return false;
                    }

                    CraftingMechanics.ServerStartCrafting(station,
                                                          character,
                                                          craftingQueue,
                                                          recipe,
                                                          countToCraft: 1,
                                                          maxQueueSize: MaxCraftingQueueEntriesCount);
                }
            }

            return true;
        }

        private void ClientShowNotificationCraftingQueueIsFull()
        {
            NotificationSystem.ClientShowNotification(
                NotificationCraftingQueueFull_Title,
                NotificationCraftingQueueFull_Message,
                color: NotificationColor.Bad);
        }

        private void ServerRemote_CancelQueueItem(ushort localId)
        {
            var character = ServerRemoteContext.Character;
            var characterServerState = PlayerCharacter.GetPrivateState(character);

            var craftingQueue = characterServerState.CraftingQueue;
            for (var index = 0; index < craftingQueue.QueueItems.Count; index++)
            {
                var item = craftingQueue.QueueItems[index];
                if (item.LocalId == localId)
                {
                    CraftingMechanics.ServerCancelCraftingQueueItem(character, item);
                    return;
                }
            }

            Logger.Warning("Cannot find crafting queue entry with localId=" + localId);
        }

        private bool SharedValidateQueueIsNotFull(ICharacter character, Recipe recipe, ushort countToCraft)
        {
            var characterServerState = PlayerCharacter.GetPrivateState(character);
            var craftingQueueItems = characterServerState.CraftingQueue.QueueItems;
            if (craftingQueueItems.Count + 1 <= MaxCraftingQueueEntriesCount)
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
                this.ClientShowNotificationCraftingQueueIsFull();
            }

            return false;
        }
    }
}