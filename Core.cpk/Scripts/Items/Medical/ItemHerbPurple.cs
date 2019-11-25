namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class ItemHerbPurple : ProtoItemMedical, IProtoItemOrganic
    {
        public override string Description =>
            "Medicinal herb that can be used to help the body get rid of toxins and other unnatural substances. Can be used as-is, but becomes more potent once properly prepared.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override double MedicalToxicity => 0.1;

        public override string Name => "Purple herb";

        public ushort OrganicValue => 3;

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            character.ServerRemoveStatusEffectIntensity<StatusEffectRadiationPoisoning>(intensityToRemove: 0.05); // 15 sec
            character.ServerRemoveStatusEffectIntensity<StatusEffectToxins>(intensityToRemove: 0.05); // 15 sec

            base.ServerOnUse(character, currentStats);
        }
    }
}