namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    // partial class containing only item methods
    public partial class PlayerCharacter
    {
        private static readonly IItemsServerService ServerItemsService = Api.IsServer
                                                                             ? Api.Server.Items
                                                                             : null;

        public static void ServerTrySpawnItemToCharacterOrGround(
            ICharacter character,
            IProtoItem protoItem,
            uint countToSpawn,
            ref IItemsContainer groundContainer)
        {
            var result = Server.Items.CreateItem(protoItem, character, countToSpawn);
            if (result.IsEverythingCreated)
            {
                // successfully spawned at character
                return;
            }

            countToSpawn -= result.TotalCreatedCount;

            // cannot spawn - try spawn to the ground
            if (groundContainer is null)
            {
                groundContainer = ObjectGroundItemsContainer.ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(
                    character,
                    character.Tile);

                if (groundContainer is null)
                {
                    return;
                }
            }

            ServerItemsService.CreateItem(protoItem,
                                          groundContainer,
                                          count: countToSpawn);

            if (groundContainer.OccupiedSlotsCount > 0)
            {
                return;
            }

            // nothing is spawned, the ground container should be destroyed
            Server.World.DestroyObject(groundContainer.OwnerAsStaticObject);
            groundContainer = null;
        }

        public override MoveItemsResult ClientTryTakeAllItems(
            ICharacter character,
            IItemsContainer fromContainer,
            bool showNotificationIfInventoryFull = true)
        {
            if (fromContainer.OccupiedSlotsCount == 0)
            {
                // no items to move
                return new MoveItemsResult() { AreAllItemMoved = true };
            }

            var privateState = GetPrivateState(character);
            var containerHotbar = privateState.ContainerHotbar;
            var containerInventory = privateState.ContainerInventory;

            // define a function for spawning item at specified container
            var clientItemsService = Client.Items;

            var result = new MoveItemsResult();

            bool TryMoveItemsTo(IItemsContainer toContainer, bool onlyToExistingStacks)
            {
                var movedItemResult = clientItemsService.TryMoveAllItems(
                    fromContainer,
                    toContainer,
                    onlyToExistingStacks);

                if (movedItemResult.MovedItems.Count == 0)
                {
                    // cannot move any item
                    return false;
                }

                // something moved (perhaps all)
                result.MergeWith(movedItemResult, areAllItemsMoved: movedItemResult.AreAllItemMoved);
                return movedItemResult.AreAllItemMoved;
            }

            // 1. Try to add to existing stacks in hotbar.
            // 3. Try to add to existing stacks or move to in inventory.
            // 3. Try to move to in hotbar.
            if (TryMoveItemsTo(containerHotbar,       onlyToExistingStacks: true)
                || TryMoveItemsTo(containerInventory, onlyToExistingStacks: false)
                || TryMoveItemsTo(containerHotbar,    onlyToExistingStacks: false))
            {
                // all items are moved!
            }

            if (result.MovedItems.Count > 0)
            {
                ItemsSoundPresets.ItemGeneric.PlaySound(ItemSound.Pick);
            }

            if (!result.AreAllItemMoved
                && showNotificationIfInventoryFull)
            {
                NotificationSystem.ClientShowNotificationNoSpaceInInventory();
            }

            return result;
        }

        public override IEnumerable<IItemsContainer> SharedEnumerateAllContainers(
            ICharacter character,
            bool includeEquipmentContainer)
        {
            var publicState = GetPublicState(character);
            if (IsClient
                && !ReferenceEquals(character, Client.Characters.CurrentPlayerCharacter))
            {
                // not a current player character - return only the equipment container
                if (includeEquipmentContainer)
                {
                    yield return publicState.ContainerEquipment;
                }

                yield break;
            }

            var privateState = GetPrivateState(character);
            yield return privateState.ContainerInventory;
            yield return privateState.ContainerHotbar;
            
            if (includeEquipmentContainer)
            {
                yield return publicState.ContainerEquipment;
            }

            // hand container is not included
            //yield return privateState.ContainerHand;
        }
    }
}