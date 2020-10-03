namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class ItemHerbBlue : ProtoItemMedical, IProtoItemOrganic
    {
        public override double CooldownDuration => MedicineCooldownDuration.Medium;

        public override string Description =>
            "Medicinal herb used as a catalyst for complex medical recipes. Should not be taken raw, as it contains toxins.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override double MedicalToxicity => 0.1;

        public override string Name => "Blue herb";

        public ushort OrganicValue => 3;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectToxins>(intensity: 0.10)
                .WillAddEffect<StatusEffectNausea>(intensity: 0.05);
        }
    }
}