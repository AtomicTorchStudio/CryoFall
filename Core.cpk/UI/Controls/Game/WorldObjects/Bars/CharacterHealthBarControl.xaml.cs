namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CharacterHealthBarControl : BaseUserControl
    {
        private CharacterCurrentStats characterCurrentStats;

        private ViewModelCharacterHealthBarControl viewModel;

        public CharacterHealthBarControl()
        {
        }

        public CharacterCurrentStats CharacterCurrentStats
        {
            get => this.characterCurrentStats;
            set
            {
                if (this.characterCurrentStats == value)
                {
                    return;
                }

                this.characterCurrentStats = value;

                if (this.viewModel != null)
                {
                    this.viewModel.CharacterCurrentStats = this.characterCurrentStats;
                }
            }
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            this.viewModel = new ViewModelCharacterHealthBarControl();
            this.viewModel.CharacterCurrentStats = this.characterCurrentStats;
            this.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}