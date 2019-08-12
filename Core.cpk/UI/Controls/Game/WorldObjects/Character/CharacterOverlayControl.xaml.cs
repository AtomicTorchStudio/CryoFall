namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CharacterOverlayControl : BaseUserControl
    {
        private readonly ICharacter character;

        private ViewModelCharacterOverlayControl viewModel;

        public CharacterOverlayControl(ICharacter character)
        {
            this.character = character;
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelCharacterOverlayControl(this.character);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }
    }
}