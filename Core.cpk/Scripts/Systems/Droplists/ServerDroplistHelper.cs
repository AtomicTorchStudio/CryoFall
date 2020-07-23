namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public static class ServerDroplistHelper
    {
        private static readonly IItemsServerService Items = Api.Server.Items;

        private static readonly IWorldServerService World = Api.Server.World;

        public static CreateItemResult TryDropToCharacter(
            IReadOnlyDropItemsList dropItemsList,
            ICharacter character,
            bool sendNoFreeSpaceNotification,
            double probabilityMultiplier,
            DropItemContext context)
        {
            if (character is null)
            {
                return new CreateItemResult() { IsEverythingCreated = false };
            }

            var result = dropItemsList.Execute(
                (protoItem, count) => Items.CreateItem(protoItem, character, count),
                context,
                probabilityMultiplier);

            if (sendNoFreeSpaceNotification
                && !result.IsEverythingCreated)
            {
                NotificationSystem.ServerSendNotificationNoSpaceInInventory(character);
            }

            return result;
        }

        public static CreateItemResult TryDropToCharacterOrGround(
            IReadOnlyDropItemsList dropItemsList,
            ICharacter toCharacter,
            Vector2Ushort tilePosition,
            bool sendNotificationWhenDropToGround,
            double probabilityMultiplier,
            DropItemContext context,
            out IItemsContainer groundContainer)
        {
            var containersProvider = new CharacterAndGroundContainersProvider(toCharacter, tilePosition);

            var result = dropItemsList.Execute(
                (protoItem, count) => Items.CreateItem(protoItem, containersProvider, count),
                context,
                probabilityMultiplier);

            groundContainer = containersProvider.GroundContainer;
            if (groundContainer is null)
            {
                return result;
            }

            if (groundContainer.OccupiedSlotsCount == 0)
            {
                // nothing is spawned, the ground container should be destroyed
                World.DestroyObject(groundContainer.OwnerAsStaticObject);
                groundContainer = null;
            }
            else
            {
                if (sendNotificationWhenDropToGround && result.TotalCreatedCount > 0)
                {
                    // notify player that there were not enough space in inventory so the items were dropped to the ground
                    NotificationSystem.ServerSendNotificationNoSpaceInInventoryItemsDroppedToGround(
                        toCharacter,
                        protoItemForIcon: result.ItemAmounts
                                                .Keys
                                                .FirstOrDefault(
                                                    k => k.Container == containersProvider.GroundContainer)?
                                                .ProtoItem);
                }
                
                WorldObjectClaimSystem.ServerTryClaim(groundContainer.OwnerAsStaticObject,
                                                      toCharacter,
                                                      WorldObjectClaimDuration.GroundItems);
            }

            return result;
        }

        public static CreateItemResult TryDropToContainer(
            IReadOnlyDropItemsList dropItemsList,
            IItemsContainer toContainer,
            double probabilityMultiplier,
            DropItemContext context)
        {
            var result = dropItemsList.Execute(
                (protoItem, count) => Items.CreateItem(protoItem, toContainer, count),
                context,
                probabilityMultiplier);

            return result;
        }

        public static CreateItemResult TryDropToGround(
            IReadOnlyDropItemsList dropItemsList,
            Vector2Ushort tilePosition,
            double probabilityMultiplier,
            DropItemContext context,
            [CanBeNull] out IItemsContainer groundContainer)
        {
            var character = context.HasCharacter ? context.Character : null;

            // obtain the ground container to drop the items into
            var tile = World.GetTile(tilePosition);
            groundContainer = ObjectGroundItemsContainer
                .ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(character, tile);

            if (groundContainer is null)
            {
                // cannot drop because there are no free space available on the ground
                return new CreateItemResult() { IsEverythingCreated = false };
            }

            var result = dropItemsList.TryDropToContainer(groundContainer, context, probabilityMultiplier);
            if (groundContainer.OccupiedSlotsCount == 0)
            {
                // nothing is spawned, the ground container should be destroyed
                World.DestroyObject(groundContainer.OwnerAsStaticObject);
                groundContainer = null;
                return result;
            }

            WorldObjectClaimSystem.ServerTryClaim(groundContainer.OwnerAsStaticObject,
                                                  character,
                                                  WorldObjectClaimDuration.GroundItems);

            return result;
        }
    }
}