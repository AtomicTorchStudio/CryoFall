namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
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
            var result = character.ProtoCharacter.ServerCreateItem(character, protoItem, countToSpawn);
            if (result.IsEverythingCreated)
            {
                // successfully spawned at character
                return;
            }

            countToSpawn -= result.TotalCreatedCount;

            // cannot spawn - try spawn to the ground
            if (groundContainer == null)
            {
                groundContainer = ObjectGroundItemsContainer.ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(
                    character.Tile);

                if (groundContainer == null)
                {
                    // cannot drop on ground
                    return;
                }

                ServerItemsService.SetContainerType<ItemsContainerOutputPublic>(groundContainer);
            }

            // estimate how many slots are required
            var groundSlotsCount = (int)groundContainer.OccupiedSlotsCount;
            groundSlotsCount += (int)Math.Ceiling(countToSpawn / (double)protoItem.MaxItemsPerStack);
            ServerItemsService.SetSlotsCount(
                groundContainer,
                (byte)Math.Min(byte.MaxValue, groundSlotsCount));

            ServerItemsService.CreateItem(protoItem,
                                          groundContainer,
                                          count: countToSpawn);
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

        public override CreateItemResult ServerCreateItem(
            ICharacter character,
            IProtoItem protoItem,
            uint countToSpawn = 1)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (protoItem == null)
            {
                throw new ArgumentNullException(nameof(protoItem));
            }

            if (countToSpawn == 0)
            {
                throw new Exception("Cannot add item to character: item count is 0");
            }

            var countToSpawnRemains = countToSpawn;
            var privateState = GetPrivateState(character);
            var containerHotbar = privateState.ContainerHotbar;
            var containerInventory = privateState.ContainerInventory;
            var serverItemsService = Server.Items;
            var result = new CreateItemResult();

            // define a function for spawning item at specified container
            bool TrySpawnItem(IItemsContainer container, bool onlyToExistingStacks)
            {
                var createItemResult = serverItemsService.CreateItem(
                    protoItem,
                    container,
                    countToSpawnRemains,
                    slotId: null,
                    onlyAddToExistingStacks: onlyToExistingStacks);

                if (createItemResult.TotalCreatedCount == 0)
                {
                    // cannot create item
                    return false;
                }

                // something created (perhaps all)
                result.MergeWith(createItemResult);
                countToSpawnRemains = (ushort)(countToSpawnRemains - createItemResult.TotalCreatedCount);
                var isEverythingCreated = countToSpawnRemains == 0;
                return isEverythingCreated;
            }

            // 1. Try to add to existing stacks in hotbar.
            // 3. Try to add to existing stacks or spawn as new stacks in inventory.
            // 3. Try to spawn as new stacks in hotbar.
            // TODO: 4. Try to spawn on the ground. (actually implemented separately via scripting)
            if (TrySpawnItem(containerHotbar,       onlyToExistingStacks: true)
                || TrySpawnItem(containerInventory, onlyToExistingStacks: false)
                || TrySpawnItem(containerHotbar,    onlyToExistingStacks: false))
            {
                // all items are created!
                result.IsEverythingCreated = true;
            }

            return result;
        }

        public override IEnumerable<IItemsContainer> SharedEnumerateAllContainers(
            ICharacter character,
            bool includeEquipmentContainer)
        {
            var publicState = GetPublicState(character);
            if (IsClient && Client.Characters.CurrentPlayerCharacter != character)
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

            if (includeEquipmentContainer)
            {
                yield return publicState.ContainerEquipment;
            }

            yield return privateState.ContainerHotbar;

            // hand container is not included
            //yield return privateState.ContainerHand;
        }
    }
}