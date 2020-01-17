namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class ItemNeuralEnhancer : ProtoItemMedical
    {
        public const string NotificationNeuralEnhancer_Message = "You've gained {0} LP.";

        public const string NotificationNeuralEnhancer_Title = "Neural enhancer";

        public const int UsageGivesLearningPointsAmount = 100;

        public override string Description =>
            "This disposable neural enhancer contains preprogrammed nanomachines. Artificially expands neural connections in the brain, increasing complexity of thoughts. Instantly gives learning points upon usage.";

        public override double MedicalToxicity => 0.0;

        public override string Name => "Neural enhancer";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemDataLog;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // adding LP
            var technologies = character.SharedGetTechnologies();
            technologies.ServerAddLearningPoints(UsageGivesLearningPointsAmount
                                                 * TechConstants.ServerLearningPointsGainMultiplier,
                                                 allowModifyingByStats: false);

            // add pain
            character.ServerAddStatusEffect<StatusEffectPain>(intensity: 1.0); // max

            // notify player
            this.CallClient(character, _ => _.ClientRemote_DisplayUseNotification());

            base.ServerOnUse(character, currentStats);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_DisplayUseNotification()
        {
            NotificationSystem.ClientShowNotification(
                NotificationNeuralEnhancer_Title,
                string.Format(NotificationNeuralEnhancer_Message, UsageGivesLearningPointsAmount),
                NotificationColor.Good,
                icon: this.Icon);
        }
    }
}