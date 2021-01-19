namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;

    public class ViewModelActionProgressControl : BaseViewModel
    {
        private IActionState actionState;

        public IActionState ActionState
        {
            get => this.actionState;
            set
            {
                if (this.actionState == value)
                {
                    return;
                }

                if (this.actionState is not null)
                {
                    this.actionState.ProgressPercentsChanged -= this.ProgressPercentsChangedHandler;
                }

                this.actionState = value;

                if (this.actionState is null)
                {
                    if (!this.IsDisposed)
                    {
                        this.ProgressBar.ValueCurrent = 0;
                    }

                    return;
                }

                this.ProgressBar.ValueCurrent = (float)this.actionState.ProgressPercents;
                this.actionState.ProgressPercentsChanged += this.ProgressPercentsChangedHandler;
            }
        }

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public ViewModelHUDStatBar ProgressBar { get; }
            = new("Action progress") { ValueMax = 100 };

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.ActionState = null;
        }

        private void ProgressPercentsChangedHandler(double newValue)
        {
            this.ProgressBar.ValueCurrent = (float)newValue;
        }
    }
}