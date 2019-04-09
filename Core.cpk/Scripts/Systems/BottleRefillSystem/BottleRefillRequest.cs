namespace AtomicTorch.CBND.CoreMod.Systems.BottleRefillSystem
{
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class BottleRefillRequest : ItemActionRequest
    {
        public BottleRefillRequest(
            ICharacter character,
            IItem item,
            IProtoTileWater waterProtoTileToRefill)
            : base(character, item)
        {
            this.WaterProtoTileToRefill = waterProtoTileToRefill;
        }

        public IProtoTileWater WaterProtoTileToRefill { get; }
    }
}