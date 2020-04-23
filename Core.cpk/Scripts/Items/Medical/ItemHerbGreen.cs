namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemHerbGreen : ProtoItemMedical, IProtoItemOrganic
    {
        public override string Description =>
            "Medicinal herb with unique healing and antibacterial properties. Can be used as-is, but becomes much more potent once properly prepared.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override double MedicalToxicity => 0.05;

        public override string Name => "Green herb";

        public ushort OrganicValue => 3;

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            character.ServerAddStatusEffect<StatusEffectHealingSlow>(intensity: 0.07); // 7 seconds (7hp)

            base.ServerOnUse(character, currentStats);
        }
    }
}