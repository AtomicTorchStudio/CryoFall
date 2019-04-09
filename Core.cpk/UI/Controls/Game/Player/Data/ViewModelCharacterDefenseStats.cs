namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ViewModelCharacterDefenseStats : BaseViewModel
    {
        private bool isActive;

        private PlayerCharacterPrivateState privateState;

        private StateSubscriptionToken privateStateToken;

        public float Chemical { get; private set; }

        public float Cold { get; private set; }

        public ICharacter CurrentCharacter { get; set; }

        public float Electrical { get; private set; }

        public float Heat { get; private set; }

        public float Impact { get; private set; }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;

                this.PrivateState = this.isActive
                                        ? PlayerCharacter.GetPrivateState(this.CurrentCharacter)
                                        : null;
            }
        }

        public float Kinetic { get; private set; }

        public float Psi { get; private set; }

        public float Radiation { get; private set; }

        private PlayerCharacterPrivateState PrivateState
        {
            get => this.privateState;
            set
            {
                if (this.privateState == value)
                {
                    return;
                }

                this.privateStateToken?.Unsubscribe();
                this.privateStateToken = null;

                this.privateState = value;

                if (this.privateState == null)
                {
                    return;
                }

                this.privateStateToken = this.privateState.ClientSubscribe(
                    s => s.FinalStatsCache,
                    newValue => this.UpdateValues(),
                    this);

                this.UpdateValues();
            }
        }

        private void UpdateValues()
        {
            if (this.privateState == null)
            {
                return;
            }

            var finalStatsCache = this.privateState.FinalStatsCache;
            this.Chemical = (float)finalStatsCache[StatName.DefenseChemical];
            this.Cold = (float)finalStatsCache[StatName.DefenseCold];
            this.Electrical = (float)finalStatsCache[StatName.DefenseElectrical];
            this.Heat = (float)finalStatsCache[StatName.DefenseHeat];
            this.Kinetic = (float)finalStatsCache[StatName.DefenseKinetic];
            this.Impact = (float)finalStatsCache[StatName.DefenseImpact];
            this.Psi = (float)finalStatsCache[StatName.DefensePsi];
            this.Radiation = (float)finalStatsCache[StatName.DefenseRadiation];
        }
    }
}