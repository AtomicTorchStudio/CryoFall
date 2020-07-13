namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
    using System.Collections.Generic;

    public class ItemPragmiumSensor : ProtoItemPragmiumSensor
    {
        public override string Description =>
            "This sensor makes it possible to search for large pragmium crystals by detecting their unique psionic footprints, even at large distances.";

        public override ushort DurabilityDecreasePerSecond => 2;

        /// <summary>
        /// The device will last for an hour of active scan with the specified durability decrease speed.
        /// </summary>
        public override uint DurabilityMax => 2 * 60 * 60;

        public override double EnergyConsumptionPerSecond => 2;

        public override double MaxRange => 100;

        public override string Name => "Pragmium sensor";

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.UsesPowerBanks);
        }
    }
}