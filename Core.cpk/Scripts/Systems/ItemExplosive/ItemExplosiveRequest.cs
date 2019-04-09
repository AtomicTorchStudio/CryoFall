namespace AtomicTorch.CBND.CoreMod.Systems.ItemExplosive
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemExplosiveRequest : ItemActionRequest
    {
        public readonly Vector2Ushort TargetPosition;

        public ItemExplosiveRequest(ICharacter character, IItem item, Vector2Ushort targetPosition)
            : base(character, item)
        {
            this.TargetPosition = targetPosition;
        }

        public override bool Equals(IActionRequest other)
        {
            return base.Equals(other)
                   && this.TargetPosition == ((ItemExplosiveRequest)other).TargetPosition;
        }
    }
}