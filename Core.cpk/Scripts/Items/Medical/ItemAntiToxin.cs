namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemAntiToxin : ProtoItemMedical
    {
        public const string NotificationNoToxins_Message =
            "You don't have toxins in your body. There's no point in using this medicine now.";

        public const string NotificationNoToxins_Title = "No toxins";

        public override double CooldownDuration => MedicineCooldownDuration.Short;

        public override string Description =>
            "This emergency toxin exposure treatment could immediately reduce any effects of accumulated toxins in the body.";

        public override double MedicalToxicity => 0.2;

        public override string Name => "Anti-toxin";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillRemoveEffect<StatusEffectToxins>(intensityToRemove: 0.50);
        }

        protected override bool SharedCanUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // does the player even have toxins?
            if (character.SharedHasStatusEffect<StatusEffectToxins>())
            {
                return true;
            }

            if (IsClient)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationNoToxins_Title,
                    NotificationNoToxins_Message,
                    NotificationColor.Bad,
                    icon: this.Icon);
            }

            return false;
        }
    }
}