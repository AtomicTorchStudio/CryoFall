namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemRemedyHerbal : ProtoItemMedical
    {
        public override string Description =>
            "Homebrewed herbal remedy. Restores some health overtime and removes nausea. Probably doesn't have any side effects.";

        public override double MedicalToxicity => 0.2;

        public override string Name => "Herbal remedy";

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // adds health regeneration
            character.ServerAddStatusEffect<StatusEffectHealingSlow>(intensity: 0.35); // 35 seconds

            // removes nausea
            character.ServerRemoveStatusEffectIntensity<StatusEffectNausea>(intensityToRemove: 1);

            base.ServerOnUse(character, currentStats);
        }
    }
}