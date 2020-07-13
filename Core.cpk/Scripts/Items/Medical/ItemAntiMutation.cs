namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemAntiMutation : ProtoItemMedical
    {
        public const string NotificationMutation_Message = "No out-of-control mutation is detected in the body.";

        public const string NotificationMutation_Title = "No mutations";

        public override string Description =>
            "Protects and restores cell DNA damaged by radiation exposure and other harmful effects. Stops uncontrolled mutations in the body.";

        public override double MedicalToxicity => 0.2;

        public override string Name => "Anti-mutation medicine";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillRemoveEffect<StatusEffectMutation>(); // removes mutation
        }

        protected override bool SharedCanUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // does the player even have mutation?
            if (character.SharedHasStatusEffect<StatusEffectMutation>())
            {
                return true;
            }

            if (IsClient)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationMutation_Title,
                    NotificationMutation_Message,
                    NotificationColor.Bad,
                    icon: this.Icon);
            }

            return false;
        }
    }
}