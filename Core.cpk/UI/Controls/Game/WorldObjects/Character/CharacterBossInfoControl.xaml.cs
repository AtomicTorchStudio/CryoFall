namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CharacterBossInfoControl : BaseUserControl
    {
        private readonly ICharacter character;

        private ViewModelCharacterBossInfoControl viewModel;

        public CharacterBossInfoControl(ICharacter character)
        {
            this.character = character;
        }

        protected override void OnLoaded()
        {
            VisualStateManager.GoToElementState(this.GetByName<Grid>("LayoutRoot"),
                                                "Collapsed",
                                                useTransitions: false);

            this.DataContext = this.viewModel = new ViewModelCharacterBossInfoControl(this.character);
            ClientUpdateHelper.UpdateCallback += this.Update;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;

            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private void Update()
        {
            if (this.viewModel is null
                || this.viewModel.IsReadyForDispose)
            {
                // this control is no longer necessary as the character client state is disposed
                ((Panel)this.Parent)?.Children.Remove(this);
            }
        }
    }
}