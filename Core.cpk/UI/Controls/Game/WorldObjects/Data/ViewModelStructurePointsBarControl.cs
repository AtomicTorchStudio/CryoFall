namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelStructurePointsBarControl : BaseViewModel
    {
        private ObjectStructurePointsData objectStructurePointsData;

        public ObjectStructurePointsData ObjectStructurePointsData
        {
            get => this.objectStructurePointsData;
            set
            {
                if (this.objectStructurePointsData.Equals(value))
                {
                    return;
                }

                if (this.objectStructurePointsData.State is not null)
                {
                    this.ReleaseSubscriptions();
                }

                this.objectStructurePointsData = value;

                if (value.State is null)
                {
                    return;
                }

                // set current values
                var state = value.State;
                this.StatBar.ValueCurrent = state.StructurePointsCurrent;
                this.StatBar.ValueMax = value.StructurePointsMax;

                // subscribe on updates
                state.ClientSubscribe(
                    _ => _.StructurePointsCurrent,
                    _ => this.StructurePointsCurrentUpdated(),
                    this);
            }
        }

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public ViewModelHUDStatBar StatBar { get; }
            = new("Structure");

        /// <summary>
        /// This method is required to set the initial value from which the value bar control will interpolate to current value.
        /// </summary>
        public void SetInitialStructurePoints(float sp)
        {
            this.StatBar.ValueCurrent = sp;
            // ensure that the current value will be displayed in the next frame
            ClientTimersSystem.AddAction(0, this.RefreshBar);
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.ObjectStructurePointsData = default;
        }

        private void RefreshBar()
        {
            if (this.objectStructurePointsData.State is null)
            {
                return;
            }

            this.StatBar.ValueCurrent = this.objectStructurePointsData.State.StructurePointsCurrent;
        }

        private void StructurePointsCurrentUpdated()
        {
            this.RefreshBar();
        }
    }
}