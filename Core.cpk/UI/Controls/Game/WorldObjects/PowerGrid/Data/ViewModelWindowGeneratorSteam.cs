namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowGeneratorSteam : ViewModelWindowGeneratorWithFuel
    {
#pragma warning disable 108,114
        public const string ElectricityProductionInfoTextFormat =
            @"Current generation {0} EU/s at {1}°C ({2}% efficiency).
              [br]Maximum output {3} EU/s.";
#pragma warning restore 108,114

        private readonly ObjectGeneratorSteam.PrivateState privateState;

        private readonly IProtoObjectGeneratorWithFuel protoGenerator;

        public ViewModelWindowGeneratorSteam(
            IStaticWorldObject worldObjectGenerator,
            ObjectGeneratorSteam.PrivateState privateState,
            ManufacturingConfig manufacturingConfig,
            LiquidContainerState liquidContainerState,
            LiquidContainerConfig liquidContainerConfig)
            : base(worldObjectGenerator,
                   privateState,
                   manufacturingConfig,
                   liquidContainerState,
                   liquidContainerConfig)
        {
            this.privateState = privateState;
            this.protoGenerator = (IProtoObjectGeneratorWithFuel)worldObjectGenerator.ProtoStaticWorldObject;
        }

        public override string ElectricityProductionInfoText
        {
            get
            {
                this.protoGenerator.SharedGetElectricityProduction(this.WorldObjectManufacturer,
                                                                   out var currentProduction,
                                                                   out var maxProduction);

                var rate = ObjectGeneratorSteam.SharedGetElectricityProductionRate(this.WorldObjectManufacturer,
                                                                                   out var temperature);

                this.NotifyPropertyChanged(nameof(this.TemperatureCurrent));

                return string.Format(
                    ElectricityProductionInfoTextFormat,
                    currentProduction.ToString("F1"),
                    // temperature in degrees
                    Math.Round(temperature,
                               digits: 1,
                               MidpointRounding.AwayFromZero).ToString("F1"),
                    // efficiency percent
                    (int)Math.Round(rate * 100, MidpointRounding.AwayFromZero),
                    maxProduction.ToString("F1"));
            }
        }

        public ushort TemperatureCurrent =>
            (ushort)Math.Round(this.privateState.CurrentTemperature,
                               MidpointRounding.AwayFromZero);

        public ushort TemperatureMax => ObjectGeneratorSteam.SteamTemperatureMax;
    }
}