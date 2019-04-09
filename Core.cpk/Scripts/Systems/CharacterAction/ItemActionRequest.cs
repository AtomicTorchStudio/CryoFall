namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemActionRequest : DefaultActionRequest
    {
        public ItemActionRequest(ICharacter character, IItem item)
            : base(character)
        {
            this.Item = item;
        }

        public IItem Item { get; }

        public override bool Equals(IActionRequest other)
        {
            return base.Equals(other)
                   && this.Item == ((ItemActionRequest)other).Item;
        }
    }
}