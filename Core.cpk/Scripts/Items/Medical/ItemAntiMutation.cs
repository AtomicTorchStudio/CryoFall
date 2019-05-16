namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemAntiMutation : ProtoItemMedical
    {
        public override string Description => "Protects and restores cell DNA damaged by radiation exposure and other harmful effects. Stops uncontrolled mutations in the body.";

        public override string Name => "Anti-mutation medicine";

        public override double MedicalToxicity => 0.2;

        public const string NotificationMutation_Title = "No mutations";

        public const string NotificationMutation_Message = "No out of control mutation is detected in the body.";
        
        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // remove mutation (100%)
            character.ServerRemoveStatusEffectIntensity<StatusEffectMutation>(intensityToRemove: 1.0);

            base.ServerOnUse(character, currentStats);
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