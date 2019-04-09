namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemAntiRadiation : ProtoItemMedical
    {
        public const string NotificationNoRadiationPoisoning_Message =
            "You don't have radiation poisoning. There's no point in using this medicine now.";

        public const string NotificationNoRadiationPoisoning_Title = "No radiation poisoning";

        public override string Description =>
            "This emergency radiation exposure treatment could immediately reduce any effects of accumulated radiation in the body.";

        public override double MedicalToxicity => 0.2;

        public override string Name => "Anti-radiation";

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // remove radiation
            character.ServerRemoveStatusEffectIntensity<StatusEffectRadiationPoisoning>(intensityToRemove: 0.3);

            base.ServerOnUse(character, currentStats);
        }

        protected override bool SharedCanUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // does the player even have radiation?
            if (character.SharedHasStatusEffect<StatusEffectRadiationPoisoning>())
            {
                return true;
            }

            if (IsClient)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationNoRadiationPoisoning_Title,
                    NotificationNoRadiationPoisoning_Message,
                    NotificationColor.Bad,
                    icon: this.Icon);
            }

            return false;
        }
    }
}