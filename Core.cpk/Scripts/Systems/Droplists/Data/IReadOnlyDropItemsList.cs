namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public interface IReadOnlyDropItemsList
    {
        bool ContainsDroplist(IReadOnlyDropItemsList other);

        IEnumerable<IProtoItem> EnumerateAllItems();

        CreateItemResult Execute(
            DelegateSpawnDropItem delegateSpawnDropItem,
            DropItemContext dropItemContext,
            double probabilityMultiplier);

        CreateItemResult TryDropToCharacter(
            ICharacter character,
            DropItemContext context,
            bool sendNoFreeSpaceNotification = true,
            double probabilityMultiplier = 1.0);

        CreateItemResult TryDropToCharacterOrGround(
            ICharacter character,
            Vector2Ushort tilePosition,
            DropItemContext context,
            [CanBeNull] out IItemsContainer groundContainer,
            bool sendNotificationWhenDropToGround = true,
            double probabilityMultiplier = 1.0);

        CreateItemResult TryDropToContainer(
            IItemsContainer container,
            DropItemContext context,
            double probabilityMultiplier = 1.0);

        CreateItemResult TryDropToGround(
            Vector2Ushort tilePosition,
            DropItemContext context,
            [CanBeNull] out IItemsContainer groundContainer,
            double probabilityMultiplier = 1.0);
    }

    public delegate CreateItemResult DelegateSpawnDropItem(IProtoItem protoItem, ushort count);
}