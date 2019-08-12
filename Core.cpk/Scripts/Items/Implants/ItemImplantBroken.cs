namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;

    public class ItemImplantBroken : ProtoItemEquipmentImplant
    {
        // non-installable
        public override ushort BiomaterialAmountRequiredToInstall => ushort.MaxValue;

        public override string Description =>
            "You have leftover pieces of a broken implant in your body. It doesn't do you any good and you should remove it as soon as possible.";

        public override uint DurabilityMax => 0; // non-degradeable

        public override TimeSpan Lifetime => TimeSpan.Zero; // non-degradeable

        public override string Name => "Broken implant";
    }
}