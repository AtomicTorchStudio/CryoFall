namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelPowerGridState : BaseViewModel
    {
        private readonly PowerGridPublicState state;

        public ViewModelPowerGridState(PowerGridPublicState state)
        {
            this.state = state;

            state.ClientSubscribe(
                _ => _.EfficiencyMultiplier,
                _ => this.NotifyPropertyChanged(nameof(this.EfficiencyPercent)),
                this);

            state.ClientSubscribe(
                _ => _.ElectricityAmount,
                _ =>
                {
                    this.NotifyPropertyChanged(nameof(this.ElectricityAmount));
                    this.NotifyPropertyChanged(nameof(this.DepletedInText));
                },
                this);

            state.ClientSubscribe(
                _ => _.ElectricityCapacity,
                _ =>
                {
                    this.NotifyPropertyChanged(nameof(this.ElectricityCapacity));
                    this.NotifyPropertyChanged(nameof(this.DepletedInText));
                },
                this);

            state.ClientSubscribe(
                _ => _.ElectricityConsumptionCurrent,
                _ =>
                {
                    this.NotifyPropertyChanged(nameof(this.ElectricityConsumptionCurrent));
                    this.NotifyPropertyChanged(nameof(this.DepletedInText));
                },
                this);

            state.ClientSubscribe(
                _ => _.ElectricityConsumptionTotalDemand,
                _ => this.NotifyPropertyChanged(nameof(this.ElectricityConsumptionTotalDemand)),
                this);

            state.ClientSubscribe(
                _ => _.ElectricityProductionCurrent,
                _ =>
                {
                    this.NotifyPropertyChanged(nameof(this.ElectricityProductionCurrent));
                    this.NotifyPropertyChanged(nameof(this.DepletedInText));
                },
                this);

            state.ClientSubscribe(
                _ => _.ElectricityProductionTotalAvailable,
                _ => this.NotifyPropertyChanged(nameof(this.ElectricityProductionTotalAvailable)),
                this);

            state.ClientSubscribe(
                _ => _.NumberConsumers,
                _ => this.NotifyPropertyChanged(nameof(this.NumberConsumers)),
                this);

            state.ClientSubscribe(
                _ => _.NumberConsumersActive,
                _ => this.NotifyPropertyChanged(nameof(this.NumberConsumersActive)),
                this);

            state.ClientSubscribe(
                _ => _.NumberProducers,
                _ => this.NotifyPropertyChanged(nameof(this.NumberProducers)),
                this);

            state.ClientSubscribe(
                _ => _.NumberProducersActive,
                _ => this.NotifyPropertyChanged(nameof(this.NumberProducersActive)),
                this);

            state.ClientSubscribe(
                _ => _.NumberStorages,
                _ => this.NotifyPropertyChanged(nameof(this.NumberStorages)),
                this);
        }

        public string DepletedInText
        {
            get
            {
                var energySurplusPerSecond = this.state.ElectricityProductionCurrent
                                             - this.state.ElectricityConsumptionCurrent;
                if (energySurplusPerSecond >= 0
                    || double.IsNaN(energySurplusPerSecond)
                    || double.IsInfinity(energySurplusPerSecond))
                {
                    return CoreStrings.PowerGridState_DepletedIn_DurationNever;
                }

                var timeRemainingSeconds = this.state.ElectricityAmount / (-energySurplusPerSecond);
                if (timeRemainingSeconds >= TimeSpan.FromDays(31).TotalSeconds)
                {
                    return CoreStrings.PowerGridState_DepletedIn_DurationNever;
                }

                return ClientTimeFormatHelper.FormatTimeDuration(timeRemainingSeconds);
            }
        }

        public byte EfficiencyPercent => (byte)Math.Round(100 * this.state.EfficiencyMultiplier,
                                                          MidpointRounding.AwayFromZero);

        public double ElectricityAmount => this.state.ElectricityAmount;

        public double ElectricityCapacity => this.state.ElectricityCapacity;

        public double ElectricityConsumptionCurrent => this.state.ElectricityConsumptionCurrent;

        public double ElectricityConsumptionTotalDemand => this.state.ElectricityConsumptionTotalDemand;

        public double ElectricityProductionCurrent => this.state.ElectricityProductionCurrent;

        public double ElectricityProductionTotalAvailable => this.state.ElectricityProductionTotalAvailable;

        public ushort NumberConsumers => this.state.NumberConsumers;

        public ushort NumberConsumersActive => this.state.NumberConsumersActive;

        public byte NumberLandClaims => this.state.NumberLandClaims;

        public ushort NumberProducers => this.state.NumberProducers;

        public ushort NumberProducersActive => this.state.NumberProducersActive;

        public ushort NumberStorages => this.state.NumberStorages;
    }
}