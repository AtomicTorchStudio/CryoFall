namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowGeneratorWithFuel : ViewModelWindowManufacturer
    {
        public const string ElectricityProductionInfoTextFormat =
            @"Current generation {0} EU/s.
              [br]Maximum output {1} EU/s.";

        private readonly IProtoObjectGeneratorWithFuel protoGenerator;

        public ViewModelWindowGeneratorWithFuel(
            IStaticWorldObject worldObjectGenerator,
            ObjectManufacturerPrivateState privateState,
            ManufacturingConfig manufacturingConfig,
            LiquidContainerState liquidContainerState,
            LiquidContainerConfig liquidContainerConfig)
            : base(
                worldObjectGenerator,
                privateState,
                manufacturingConfig)
        {
            this.protoGenerator = (IProtoObjectGeneratorWithFuel)worldObjectGenerator.ProtoStaticWorldObject;
            this.ViewModelLiquidContainerState = new ViewModelLiquidContainerState(
                liquidContainerState,
                liquidContainerConfig,
                liquidType: this.protoGenerator.LiquidType);

            this.Refresh();
        }

        public virtual string ElectricityProductionInfoText
        {
            get
            {
                this.protoGenerator.SharedGetElectricityProduction(this.WorldObjectManufacturer,
                                                                   out var currentProduction,
                                                                   out var maxProduction);

                return string.Format(
                    ElectricityProductionInfoTextFormat,
                    currentProduction.ToString("F1"),
                    maxProduction.ToString("F1"));
            }
        }

        public ViewModelLiquidContainerState ViewModelLiquidContainerState { get; }

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