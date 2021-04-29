namespace AtomicTorch.CBND.CoreMod.Drones
{
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IProtoDrone : IProtoDynamicWorldObject
    {
        IProtoItemDrone ProtoItemDrone { get; }

        IProtoItemWeapon ProtoItemMiningTool { get; }

        double StatMoveSpeed { get; }

        void ServerDropDroneToGround(
            IDynamicWorldObject objectDrone,
            Tile tile,
            ICharacter forOwnerCharacter);

        IItemsContainer ServerGetStorageItemsContainer(IDynamicWorldObject objectDrone);

        void ServerOnDroneDroppedOrReturned(
            IDynamicWorldObject objectDrone,
            ICharacter toCharacter,
            bool isReturnedToPlayer);

        void ServerSetDroneTarget(
            IDynamicWorldObject objectDrone,
            IStaticWorldObject targetWorldObject,
            Vector2D fromStartPosition);

        void ServerSetupAssociatedItem(
            IDynamicWorldObject objectDrone,
            IItem item);

        void ServerStartDrone(
            IDynamicWorldObject objectDrone,
            ICharacter character);
    }
}