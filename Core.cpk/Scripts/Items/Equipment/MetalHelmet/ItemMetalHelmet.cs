﻿namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemMetalHelmet : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemMetalArmor>().Description;

        public override uint DurabilityMax => 1000;

        public override bool IsHairVisible => false;

        public override string Name => "Metal helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.60,
                kinetic: 0.45,
                explosion: 0.40,
                heat: 0.20,
                cold: 0.10,
                chemical: 0.15,
                radiation: 0.10,
                psi: 0.0);

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Psi = 0.35 / defense.Multiplier;
        }
    }
}