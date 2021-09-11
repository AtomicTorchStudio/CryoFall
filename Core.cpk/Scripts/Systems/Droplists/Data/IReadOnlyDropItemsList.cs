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
            double probabilityMultiplier,
            bool sendNoFreeSpaceNotification = true);

        CreateItemResult TryDropToCharacterOrGround(
            ICharacter character,
            Vector2Ushort tilePosition,
            DropItemContext context,
            [CanBeNull] out IItemsContainer groundContainer,
            double probabilityMultiplier,
            bool sendNotificationWhenDropToGround = true);

        CreateItemResult TryDropToContainer(
            IItemsContainer container,
            DropItemContext context,
            double probabilityMultiplier);

        CreateItemResult TryDropToGround(
            Vector2Ushort tilePosition,
            DropItemContext context,
            [CanBeNull] out IItemsContainer groundContainer,
            double probabilityMultiplier);
    }
}