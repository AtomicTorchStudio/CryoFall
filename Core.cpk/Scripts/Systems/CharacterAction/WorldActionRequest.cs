namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class WorldActionRequest : DefaultActionRequest
    {
        public WorldActionRequest(ICharacter character, IWorldObject worldObject) : base(character)
        {
            this.WorldObject = worldObject;
        }

        public IWorldObject WorldObject { get; }

        public override bool Equals(IActionRequest other)
        {
            return base.Equals(other)
                   && this.WorldObject == ((WorldActionRequest)other).WorldObject;
        }
    }
}