namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Core;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelCharacterHealthBarControl : BaseViewModel
    {
        private CharacterCurrentStats characterCurrentStats;

        public ViewModelCharacterHealthBarControl()
        {
            ClientComponentUpdateHelper.UpdateCallback += this.Update;
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

                if (this.characterCurrentStats != null)
                {
                    this.ReleaseSubscriptions();
                }

                this.characterCurrentStats = value;

                if (this.characterCurrentStats == null)
                {
                    return;
                }

                // set current values
                this.StatBar.ValueCurrent = this.characterCurrentStats.HealthCurrent;
                this.StatBar.ValueMax = this.characterCurrentStats.HealthMax;

                // subscribe on updates
                this.characterCurrentStats.ClientSubscribe(
                    _ => _.HealthCurrent,
                    this.HealthCurrentUpdated,
                    this);

                this.characterCurrentStats.ClientSubscribe(
                    _ => _.HealthMax,
                    this.HealthMaxUpdated,
                    this);

                this.HealthCurrentUpdated(this.characterCurrentStats.HealthCurrent);
            }
        }

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public ViewModelHUDStatBar StatBar { get; } = new ViewModelHUDStatBar("Health");

        public string VisualStateName { get; private set; } = "Collapsed";

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.characterCurrentStats = null;
            ClientComponentUpdateHelper.UpdateCallback -= this.Update;
        }

        private void HealthCurrentUpdated(float healthCurrent)
        {
            this.StatBar.ValueCurrent = healthCurrent;
            this.UpdateVisibility();
        }

        private void HealthMaxUpdated(float healthMax)
        {
            this.StatBar.ValueMax = healthMax;
        }

        private void Update()
        {
            this.UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (this.StatBar.ValueCurrent <= 0)
            {
                this.VisualStateName = "Collapsed";
                return;
            }

            var character = (ICharacter)this.characterCurrentStats.GameObject;

            if (!ClientTimeOfDayVisibilityHelper.ClientIsObservable(character))
            {
                this.VisualStateName = "Collapsed";
                return;
            }

            this.VisualStateName = "Visible";
        }
    }
}