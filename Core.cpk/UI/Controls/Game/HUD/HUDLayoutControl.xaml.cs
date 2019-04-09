namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDLayoutControl : BaseUserControl
    {
        private static ViewModelHUD viewModel;

        public HUDLayoutControl()
        {
        }

        public static HUDLayoutControl Instance { get; private set; }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            if (Instance != null)
            {
                throw new Exception(this.GetType() + " instance is already exists");
            }

            Instance = this;

            if (viewModel == null)
            {
                viewModel = new ViewModelHUD();
            }

            this.DataContext = viewModel;
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            if (Instance == this)
            {
                Instance = null;
            }

            this.DataContext = null;
            viewModel.Dispose();
            viewModel = null;
        }
    }
}