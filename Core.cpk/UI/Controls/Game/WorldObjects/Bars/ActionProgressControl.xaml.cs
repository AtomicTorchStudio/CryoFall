namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ActionProgressControl : BaseUserControl
    {
        private IActionState actionState;

        private ViewModelActionProgressControl viewModel;

        public IActionState ActionState
        {
            get => this.actionState;
            set
            {
                if (this.actionState == value)
                {
                    return;
                }

                this.actionState = value;
                this.UpdateViewModel();
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.viewModel = new ViewModelActionProgressControl();
            this.UpdateViewModel();
            this.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void UpdateViewModel()
        {
            if (this.viewModel is not null)
            {
                this.viewModel.ActionState = this.actionState;
            }
        }
    }
}