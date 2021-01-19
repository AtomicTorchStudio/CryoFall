namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ViewModelCharacterBossHealthbarControl : BaseViewModel
    {
        private readonly ICharacter character;

        private readonly BaseCharacterClientState clientState;

        private readonly ICharacterPublicState publicState;

        private bool isDisposeScheduled;

        public ViewModelCharacterBossHealthbarControl(ICharacter character)
        {
            this.character = character;
            this.publicState = character.GetPublicState<ICharacterPublicState>();
            this.clientState = character.GetClientState<BaseCharacterClientState>();

            this.ViewModelCharacterHealthBarControl = new ViewModelCharacterHealthBarControl
            {
                CharacterCurrentStats = this.publicState.CurrentStats
            };

            ClientUpdateHelper.UpdateCallback += this.Update;
        }

        public string BossName => this.character.ProtoCharacter.Name;

        public bool IsReadyForDispose { get; private set; }

        public ViewModelCharacterHealthBarControl ViewModelCharacterHealthBarControl { get; }

        public string VisualStateName { get; private set; } = "Collapsed";

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private void Update()
        {
            if (this.isDisposeScheduled)
            {
                return;
            }

            if (this.publicState.IsDead
                || (this.clientState?.IsDisposed ?? true))
            {
                if (this.VisualStateName == "Collapsed")
                {
                    // dispose instantly
                    this.IsReadyForDispose = true;
                    return;
                }

                // dispose after fade out
                this.VisualStateName = "Collapsed";
                ClientTimersSystem.AddAction(0.5,
                                             () => this.IsReadyForDispose = true);
                this.isDisposeScheduled = true;
                return;
            }

            this.VisualStateName = "Visible";
        }
    }
}