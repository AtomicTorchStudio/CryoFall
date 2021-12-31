namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientOptions.General;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CharacterOverlayControl : BaseUserControl
    {
        private readonly ICharacter character;

        private readonly bool isCurrentPlayerCharacter;

        private Visibility cachedVisibility = Visibility.Visible;

        private bool isUpdateSubscribed;

        private Grid layoutRoot;

        private ViewModelCharacterOverlayControl viewModel;

        public CharacterOverlayControl(ICharacter character)
        {
            this.character = character;
            this.isCurrentPlayerCharacter = this.character.IsCurrentClientCharacter;
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

            if (!this.character.IsNpc)
            {
                ClientUpdateHelper.UpdateCallback += this.UpdateForPlayerCharacter;
                this.isUpdateSubscribed = true;
                this.UpdateForPlayerCharacter();
            }
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;

            if (this.isUpdateSubscribed)
            {
                this.isUpdateSubscribed = false;
                ClientUpdateHelper.UpdateCallback -= this.UpdateForPlayerCharacter;
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

        private void UpdateForPlayerCharacter()
        {
            var visibility = this.isCurrentPlayerCharacter
                                 ? GeneralOptionDisplayHealthbarAboveCurrentCharacter.IsDisplay
                                   && this.character.ProtoGameObject.GetType() == typeof(PlayerCharacter)
                                       ? Visibility.Visible
                                       : Visibility.Collapsed
                                 : Visibility.Visible;

            if (visibility == Visibility.Visible
                && PlayerCharacter.GetPublicState(this.character).CurrentPublicActionState
                    is CharacterLaunchpadEscapeAction.PublicState)
            {
                // launching on a rocket
                visibility = Visibility.Collapsed;
            }

            // we're using cached field here for optimization reasons
            if (this.cachedVisibility == visibility)
            {
                return;
            }

            this.Visibility = this.cachedVisibility = visibility;
        }
    }
}