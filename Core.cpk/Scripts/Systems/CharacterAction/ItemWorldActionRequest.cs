namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ItemWorldActionRequest : WorldActionRequest
    {
        public ItemWorldActionRequest(ICharacter character, IWorldObject worldObject, IItem item)
            : base(character, worldObject)
        {
            this.Item = item;
        }

        public PlayerCharacterPublicState CharacterPublicState
            => this.Character.GetPublicState<PlayerCharacterPublicState>();

        public IItem Item { get; }

        public override bool Equals(IActionRequest other)
        {
            return base.Equals(other)
                   && this.Item == ((ItemWorldActionRequest)other).Item;
        }
    }
}