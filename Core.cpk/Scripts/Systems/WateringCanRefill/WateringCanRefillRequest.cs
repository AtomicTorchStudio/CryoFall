namespace AtomicTorch.CBND.CoreMod.Systems.WateringCanRefill
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class WateringCanRefillRequest : ItemWorldActionRequest
    {
        public WateringCanRefillRequest(
            ICharacter character,
            IWorldObject worldObject,
            IItem item)
            : base(character, worldObject, item)
        {
        }
    }
}