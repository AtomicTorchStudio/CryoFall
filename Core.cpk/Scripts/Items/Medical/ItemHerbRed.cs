namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemHerbRed : ProtoItemMedical, IProtoItemOrganic
    {
        public override string Description =>
            "Medicinal herb with a strong stimulating effect. Should not be taken raw, as it contains toxins.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override double MedicalToxicity => 0.25;

        public override string Name => "Red herb";

        public ushort OrganicValue => 3;

        public override float StaminaRestore => 50;

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            character.ServerAddStatusEffect<StatusEffectEnergyRush>(intensity: 0.1);
            character.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.1);

            base.ServerOnUse(character, currentStats);
        }
    }
}