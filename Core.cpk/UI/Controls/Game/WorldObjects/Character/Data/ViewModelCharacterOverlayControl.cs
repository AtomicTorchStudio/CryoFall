namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ViewModelCharacterOverlayControl : BaseViewModel
    {
        private readonly ICharacter character;

        private readonly ICharacterPublicState publicState;

        public ViewModelCharacterOverlayControl(ICharacter character)
        {
            this.character = character;
            this.publicState = character.GetPublicState<ICharacterPublicState>();

            if (!character.IsNpc)
            {
                this.ViewModelCharacterNameControl = new ViewModelCharacterNameControl(character);
            }

            this.ViewModelCharacterUnstuckInfoControl = new ViewModelCharacterUnstuckInfoControl(character);

            this.ViewModelCharacterHealthBarControl = new ViewModelCharacterHealthBarControl();
            this.ViewModelCharacterHealthBarControl.CharacterCurrentStats = this.publicState.CurrentStats;

            this.ViewModelCharacterPublicStatusEffects =
                new ViewModelCharacterPublicStatusEffects(this.publicState.CurrentPublicStatusEffects);

            ClientUpdateHelper.UpdateCallback += this.Update;
            this.Update();
        }

        public ViewModelCharacterHealthBarControl ViewModelCharacterHealthBarControl { get; }

        public ViewModelCharacterNameControl ViewModelCharacterNameControl { get; }

        public ViewModelCharacterPublicStatusEffects ViewModelCharacterPublicStatusEffects { get; }

        public ViewModelCharacterUnstuckInfoControl ViewModelCharacterUnstuckInfoControl { get; }

        public string VisualStateName { get; private set; } = "Collapsed";

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private void Update()
        {
            if (this.publicState.IsDead)
            {
                this.VisualStateName = "Collapsed";
                return;
            }

            if (!ClientTimeOfDayVisibilityHelper.ClientIsObservable(this.character))
            {
                this.VisualStateName = "Collapsed";
                return;
            }

            this.VisualStateName = "Visible";
        }
    }
}