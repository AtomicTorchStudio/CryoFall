namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using JetBrains.Annotations;

    public partial class HUDStatusEffectsBar : BaseUserControl
    {
        [CanBeNull]
        public static HUDStatusEffectsBar Instance;

        private ViewModelHUDStatusEffectsBar viewModel;

        public void Flicker<TProtoStatusEffect>()
            where TProtoStatusEffect : IProtoStatusEffect
        {
            this.viewModel.Flicker<TProtoStatusEffect>();
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            var statusEffects = Api.Client.Characters.CurrentPlayerCharacter
                                   .GetPrivateState<BaseCharacterPrivateState>()
                                   .StatusEffects;
            this.DataContext = this.viewModel = new ViewModelHUDStatusEffectsBar(statusEffects);

            Instance = this;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            Instance = null;
        }
    }
}