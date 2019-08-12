namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System;

    public class ItemMulch : ProtoItemFertilizer
    {
        public const string ShortDescriptionText = "Improves growth speed by {0}%";

        public ItemMulch()
        {
            this.FertilizerShortDescription =
                string.Format(ShortDescriptionText,
                              "+"
                              + (int)Math.Round(
                                  (this.PlantGrowthSpeedMultiplier - 1.0) * 100,
                                  MidpointRounding.AwayFromZero));
        }

        public override string Description =>
            "Mulch is essentially decomposed organic material. Great as a natural organic fertilizer.";

        public override string FertilizerShortDescription { get; }

        public override string Name => "Mulch";

        public override double PlantGrowthSpeedMultiplier => 2;
    }
}