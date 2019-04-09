namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemAntiToxinPreExposure : ProtoItemMedical
    {
        public override string Description =>
            "This pre-exposure toxin aid could be used to prevent absorption of any toxins by the body, as well as boost the immune system to mitigate the effects of any toxins that do enter the bloodstream.";

        public override double MedicalToxicity => 0.2;

        public override string Name => "Toxin prevention aid";

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // remove toxins
            character.ServerRemoveStatusEffectIntensity<StatusEffectToxins>(intensityToRemove: 0.1);

            //add toxin protection
            character.ServerAddStatusEffect<StatusEffectProtectionToxins>(intensity: 0.5); // 5 minutes

            base.ServerOnUse(character, currentStats);
        }
    }
}