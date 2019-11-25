namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.CooldownIndicator
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.ClockProgressIndicator;

    public class ViewModelCooldownIndicatorControl : ViewModelClockProgressIndicator
    {
        private ClientComponentCooldownIndicator componentCooldownIndicatorUpdater;

        private bool isTurnedOn;

        private double timeRemainsSeconds;

        private double totalDurationSeconds;

        public ViewModelCooldownIndicatorControl() : base(
            isReversed: true,
            isAutoDisposeFields: true)
        {
        }

        public bool IsVisible { get; set; }

        public string TestText => "Test";

        public void TurnOff()
        {
            if (IsDesignTime)
            {
                return;
            }

            if (this.isTurnedOn)
            {
                this.isTurnedOn = false;
                Logger.Info("Cooldown turn off!");

                this.componentCooldownIndicatorUpdater.SceneObject.Destroy();
                this.componentCooldownIndicatorUpdater = null;
            }

            this.IsVisible = false;
        }

        public void TurnOn(double lengthSeconds)
        {
            if (IsDesignTime)
            {
                return;
            }

            if (lengthSeconds <= 0)
            {
                this.TurnOff();
                return;
            }

            Logger.Info("Cooldown started: " + lengthSeconds + " seconds");

            this.ResetLastProgressFraction();
            this.totalDurationSeconds = lengthSeconds;
            this.timeRemainsSeconds = lengthSeconds;

            if (!this.isTurnedOn)
            {
                this.isTurnedOn = true;
                this.IsVisible = true;

                this.componentCooldownIndicatorUpdater = Client.Scene.CreateSceneObject("Cooldown scene object")
                                                               .AddComponent<ClientComponentCooldownIndicator>();
                this.componentCooldownIndicatorUpdater.ViewModelToUpdate = this;
            }

            this.Update(0);
        }

        public void Update(double deltaTime)
        {
            this.timeRemainsSeconds -= deltaTime;

            var progressFraction = (this.totalDurationSeconds - this.timeRemainsSeconds) / this.totalDurationSeconds;
            if (progressFraction >= 1)
            {
                this.TurnOff();
                return;
            }

            this.ProgressFraction = progressFraction;
        }
    }
}