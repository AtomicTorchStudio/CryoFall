namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
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

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelHUDStatusEffectsBar();
            Instance = this;
        }

        protected override void OnUnloaded()
        {
            Instance = null;
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}