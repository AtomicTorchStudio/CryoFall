namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using System;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDLayoutControl : BaseUserControl
    {
        private static ViewModelHUD viewModel;

        public static HUDLayoutControl Instance { get; private set; }

        public UIElementCollection GridHotbarChildren { get; private set; }

        protected override void InitControl()
        {
            this.GridHotbarChildren = this.GetByName<Grid>("GridHotbar").Children;
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

            this.DataContext = viewModel ??= new ViewModelHUD();
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