namespace AtomicTorch.CBND.CoreMod.Systems.FishingSystem
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class FishingActionRequest : ItemActionRequest
    {
        public FishingActionRequest(ICharacter character, IItem item, Vector2D fishingTargetPosition)
            : base(character, item)
        {
            this.FishingTargetPosition = fishingTargetPosition;
        }

        public Vector2D FishingTargetPosition { get; }

        public override bool Equals(IActionRequest other)
        {
            return base.Equals(other)
                   && this.FishingTargetPosition == ((FishingActionRequest)other).FishingTargetPosition;
        }
    }
}