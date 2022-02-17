namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CharacterOverlayControl : BaseUserControl
    {
        private readonly ICharacter character;

        private Grid layoutRoot;

        private ViewModelCharacterOverlayControl viewModel;

        public CharacterOverlayControl(ICharacter character)
        {
            this.character = character;
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.DataContext
                = this.viewModel
                      = new ViewModelCharacterOverlayControl(this.character,
                                                             () => this.RefreshVisualState(useTransitions: true));

            // switch instantly to the initial state
            // (e.g. a creature enters the scope during the night, there should be no visible->collapsed animation)
            this.RefreshVisualState(useTransitions: false);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }

        private void RefreshVisualState(bool useTransitions)
        {
            if (!this.isLoaded)
            {
                return;
            }

            VisualStateManager.GoToElementState(this.layoutRoot,
                                                this.viewModel.GetVisualStateName(),
                                                useTransitions);
        }
    }
}