namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ClientOptions.General;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CharacterOverlayControl : BaseUserControl
    {
        private readonly ICharacter character;

        private readonly bool isCurrentClientCharacter;

        private Grid layoutRoot;

        private ViewModelCharacterOverlayControl viewModel;

        private Visibility visibilityOverCurrentCharacter = Visibility.Visible;

        public CharacterOverlayControl(ICharacter character)
        {
            this.character = character;
            this.isCurrentClientCharacter = this.character.IsCurrentClientCharacter;
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

            if (this.isCurrentClientCharacter)
            {
                ClientUpdateHelper.UpdateCallback += this.Update;
                this.Update();
            }
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;

            if (this.isCurrentClientCharacter)
            {
                ClientUpdateHelper.UpdateCallback -= this.Update;
            }
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

        private void Update()
        {
            var currentVisibility = GeneralOptionDisplayHealthbarAboveCurrentCharacter.IsDisplay
                                        ? Visibility.Visible
                                        : Visibility.Collapsed;

            // we're using cached field here for optimization reasons
            if (this.visibilityOverCurrentCharacter == currentVisibility)
            {
                return;
            }

            this.Visibility = this.visibilityOverCurrentCharacter = currentVisibility;
        }
    }
}