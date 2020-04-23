namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowGeneratorSolar : BaseViewModel
    {
        public const string ElectricityProductionInfoTextFormat =
            @"Current generation {0} EU/s at {1}% light level.
              [br]Maximum output {2} EU/s.";

        private readonly IStaticWorldObject objectGenerator;

        private readonly ProtoObjectGeneratorSolar protoGenerator;

        private readonly ObjectGeneratorSolarPublicState publicState;

        public ViewModelWindowGeneratorSolar(
            IStaticWorldObject objectGenerator,
            ObjectGeneratorSolarPublicState publicState)
        {
            this.objectGenerator = objectGenerator;
            this.protoGenerator = (ProtoObjectGeneratorSolar)objectGenerator.ProtoGameObject;
            this.publicState = publicState;

            this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(
                    publicState.PanelsContainer,
                    callbackTakeAllItemsSuccess: null)
                {
                    IsContainerTitleVisible = false,
                    IsManagementButtonsVisible = false
                };

            this.Refresh();
        }

        public string ElectricityProductionInfoText
        {
            get
            {
                this.protoGenerator.SharedGetElectricityProduction(this.objectGenerator,
                                                                   out var currentProduction,
                                                                   out var maxProduction);

                var rate = ProtoObjectGeneratorSolar.SharedGetCurrentLightFraction();

                return string.Format(
                    ElectricityProductionInfoTextFormat,
                    currentProduction.ToString("F1"),
                    // efficiency percent
                    (int)Math.Round(rate * 100, MidpointRounding.AwayFromZero),
                    maxProduction.ToString("F1"));
            }
        }

        public IItemsContainer PanelsItemsContainer => this.publicState.PanelsContainer;

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientContainersExchangeManager.Unregister(this);
        }

        private void Refresh()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.ElectricityProductionInfoText));

            ClientTimersSystem.AddAction(
                delaySeconds: 0.5,
                this.Refresh);
        }
    }
}