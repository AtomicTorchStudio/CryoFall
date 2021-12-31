namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemFishingRod : ProtoItemFishingRod
    {
        public override string Description =>
            "This fishing rod can be used to catch fish in any body of water. Requires bait.";

        public override uint DurabilityMax => 100;

        public override Vector2F FishingLineStartScreenOffset => (47, 283);

        public override double FishingSpeedMultiplier => 1.0;

        public override string Name => "Fishing rod";
    }
}