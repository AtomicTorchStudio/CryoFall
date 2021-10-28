namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;

    public class ItemRemedyHerbal : ProtoItemMedical
    {
        public override double CooldownDuration => MedicineCooldownDuration.VeryLong;

        public override string Description =>
            "Homebrewed herbal remedy. Restores some health overtime and removes nausea. Probably doesn't have any side effects.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override double MedicalToxicity => 0.2;

        public override string Name => "Herbal remedy";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHealingSlow>(intensity: 0.30) // adds health regeneration
                .WillRemoveEffect<StatusEffectNausea>();                 // removes nausea
        }
    }
}