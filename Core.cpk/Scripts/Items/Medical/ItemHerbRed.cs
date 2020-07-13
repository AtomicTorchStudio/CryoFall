namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class ItemHerbRed : ProtoItemMedical, IProtoItemOrganic
    {
        public override string Description =>
            "Medicinal herb with a strong stimulating effect. Should not be taken raw, as it contains toxins.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override double MedicalToxicity => 0.25;

        public override string Name => "Red herb";

        public ushort OrganicValue => 3;

        public override float StaminaRestore => 50;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectEnergyRush>(intensity: 0.10)
                .WillAddEffect<StatusEffectToxins>(intensity: 0.10);
        }
    }
}