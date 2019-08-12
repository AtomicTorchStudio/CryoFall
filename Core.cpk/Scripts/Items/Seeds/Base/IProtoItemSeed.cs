namespace AtomicTorch.CBND.CoreMod.Items.Seeds
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IProtoItemSeed : IProtoItem
    {
        IProtoObjectVegetation ObjectPlantProto { get; }

        void ClientPlaceAt(IItem itemSeed, Vector2Ushort tilePosition);

        void SharedIsValidPlacementPosition(
            Vector2Ushort tilePosition,
            ICharacter character,
            bool logErrors,
            out bool canPlace,
            out bool isTooFar);
    }
}