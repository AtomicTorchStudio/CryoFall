namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays.Data
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ViewModelItemSlotMedicineCooldownOverlayControl : BaseViewModel
    {
        private NetworkSyncList<ILogicObject> statusEffects;

        public ViewModelItemSlotMedicineCooldownOverlayControl()
        {
            this.statusEffects = ClientCurrentCharacterHelper.PrivateState.StatusEffects;
            this.statusEffects.ClientElementInserted += this.StatusEffectsInsertedHandler;
            this.RefreshMedicalCooldown();
        }

        public double MedicalCooldownDuration { get; private set; }

        protected override void DisposeViewModel()
        {
            this.statusEffects.ClientElementInserted -= this.StatusEffectsInsertedHandler;
            this.statusEffects = null;
            base.DisposeViewModel();
        }

        private void RefreshMedicalCooldown()
        {
            // assign twice in order to reset the animated countdown
            this.MedicalCooldownDuration = 0;

            var medicalCooldownEffect =
                this.statusEffects.FirstOrDefault(se => se.ProtoGameObject is StatusEffectMedicalCooldown);
            if (medicalCooldownEffect is null)
            {
                return;
            }

            var intensity = StatusEffectMedicalCooldown.GetPublicState(medicalCooldownEffect).Intensity;
            this.MedicalCooldownDuration = intensity * StatusEffectMedicalCooldown.MaxDuration;
        }

        private void StatusEffectsInsertedHandler(
            NetworkSyncList<ILogicObject> source,
            int index,
            ILogicObject value)
        {
            if (value.ProtoGameObject is StatusEffectMedicalCooldown)
            {
                this.RefreshMedicalCooldown();
            }
        }
    }
}