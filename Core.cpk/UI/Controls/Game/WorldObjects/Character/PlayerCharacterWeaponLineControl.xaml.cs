namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class PlayerCharacterWeaponLineControl : BaseUserControl
    {
        private static PlayerCharacterWeaponLineControl instance;

        private ViewModelPlayerCharacterWeaponLineControl viewModel;

        public static void Setup(ICharacter character)
        {
            if (instance is null)
            {
                instance = new PlayerCharacterWeaponLineControl();
                Api.Client.UI.LayoutRootChildren.Add(instance);
            }

            instance.viewModel.Character = character;
        }

        protected override void InitControl()
        {
            this.viewModel = new ViewModelPlayerCharacterWeaponLineControl();
            this.DataContext = this.viewModel;
        }
    }
}