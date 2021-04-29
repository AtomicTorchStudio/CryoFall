namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;
    using static CoreStrings;

    public class ViewModelPowerSwitch : BaseViewModel
    {
        private readonly IObjectElectricityStructurePrivateState privateState;

        private readonly IStaticWorldObject worldObject;

        private bool isPowerOn;

        public ViewModelPowerSwitch(IStaticWorldObject worldObject)
        {
            this.worldObject = worldObject;
            this.privateState = this.worldObject.GetPrivateState<IObjectElectricityStructurePrivateState>();

            this.privateState.ClientSubscribe(
                _ => _.PowerGridChargePercent,
                _ => { this.RefreshPowerGridChargePercent(); },
                this);

            this.privateState.ClientSubscribe(
                _ => _.ElectricityThresholds,
                _ => this.RefreshThresholds(),
                this);

            switch (worldObject.ProtoStaticWorldObject)
            {
                case IProtoObjectElectricityProducer:
                {
                    // producer
                    var publicState = worldObject.GetPublicState<IObjectElectricityProducerPublicState>();

                    publicState.ClientSubscribe(
                        _ => _.ElectricityProducerState,
                        _ => this.RefreshCurrentState(),
                        this);

                    this.ViewModelPowerStateOverlay = new ViewModelPowerStateOverlay(publicState);
                    break;
                }

                case IProtoObjectElectricityConsumer:
                {
                    // consumer
                    var publicState = worldObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                    publicState.ClientSubscribe(
                        _ => _.ElectricityConsumerState,
                        _ => this.RefreshCurrentState(),
                        this);

                    this.ViewModelPowerStateOverlay = new ViewModelPowerStateOverlay(publicState);
                    break;
                }
            }

            this.RefreshCurrentState();
            this.RefreshPowerAmountText();
        }

        public BaseCommand CommandConfigureThresholds
            => new ActionCommand(this.ExecuteCommandConfigure);

        public BaseCommand CommandPowerOff
            => new ActionCommand(() => this.IsPowerOn = false);

        public BaseCommand CommandPowerOn
            => new ActionCommand(() => this.IsPowerOn = true);

        public string CurrentPowerModeExplanationText
        {
            get
            {
                var thresholds = this.privateState.ElectricityThresholds;

                switch (this.worldObject.ProtoStaticWorldObject)
                {
                    case IProtoObjectElectricityProducer:
                    {
                        // producer
                        var state = this.worldObject.GetPublicState<IObjectElectricityProducerPublicState>()
                                        .ElectricityProducerState;

                        switch (state)
                        {
                            case ElectricityProducerState.PowerOff:
                                return PowerProducerState_PowerOff_Title;

                            case ElectricityProducerState.PowerOnIdle:
                                return PowerProducerState_PowerOnIdle_Title
                                       + "[br][br]"
                                       + (this.privateState.PowerGridChargePercent >= thresholds.ShutdownPercent
                                              ? string.Format(
                                                  PowerProducerState_PowerOnIdle_AboveShutdownThreshold_Description_Format,
                                                  thresholds.ShutdownPercent,
                                                  thresholds.StartupPercent)
                                              : string.Format(
                                                  PowerProducerState_PowerOnIdle_AboveStartupThreshold_Description_Format,
                                                  thresholds.StartupPercent));

                            case ElectricityProducerState.PowerOnActive:
                                return PowerProducerState_PowerOn_Title
                                       + "[br][br]"
                                       + string.Format(PowerProducerState_PowerOn_Description_Format,
                                                       thresholds.ShutdownPercent);

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    case IProtoObjectElectricityConsumer:
                    {
                        // consumer
                        var state = this.worldObject.GetPublicState<IObjectElectricityConsumerPublicState>()
                                        .ElectricityConsumerState;
                        switch (state)
                        {
                            case ElectricityConsumerState.PowerOff:
                                return PowerConsumerState_PowerOff_Title;

                            case ElectricityConsumerState.PowerOnIdle:
                                return PowerConsumerState_PowerOnIdle_Title
                                       + "[br][br]"
                                       + (this.privateState.PowerGridChargePercent < thresholds.ShutdownPercent
                                              ? string.Format(
                                                  PowerConsumerState_PowerOnIdle_BelowShutdownThreshold_Description_Format,
                                                  thresholds.ShutdownPercent,
                                                  thresholds.StartupPercent)
                                              : string.Format(
                                                  PowerConsumerState_PowerOnIdle_BelowStartupThreshold_Description_Format,
                                                  thresholds.StartupPercent));

                            case ElectricityConsumerState.PowerOnActive:
                                return PowerConsumerState_PowerOn_Title
                                       + "[br][br]"
                                       + string.Format(PowerConsumerState_PowerOn_Description_Format,
                                                       thresholds.ShutdownPercent);

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public bool IsPowerOn
        {
            get => this.isPowerOn;
            set
            {
                if (this.isPowerOn == value)
                {
                    return;
                }

                this.isPowerOn = value;
                this.NotifyThisPropertyChanged();

                // assume the change is from the two-way binding
                PowerGridSystem.ClientSetPowerMode(isOn: value,
                                                   this.worldObject)
                               .ContinueWith(_ => this.RefreshCurrentState(),
                                             TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public double PowerAmount
        {
            get
            {
                switch (this.worldObject.ProtoStaticWorldObject)
                {
                    case IProtoObjectElectricityProducer protoProducer:
                    {
                        // producer
                        protoProducer.SharedGetElectricityProduction(this.worldObject,
                                                                     out var currentProduction,
                                                                     out _);
                        return currentProduction;
                    }

                    case IProtoObjectElectricityConsumer protoConsumer:
                    {
                        // consumer
                        var consumption = protoConsumer.ElectricityConsumptionPerSecondWhenActive;
                        if (protoConsumer is IProtoObjectElectricityConsumerWithCustomRate withCustomRate)
                        {
                            consumption *= MathHelper.Clamp(
                                withCustomRate.SharedGetCurrentElectricityConsumptionRate(this.worldObject),
                                0,
                                1);
                        }

                        return -consumption;
                    }

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public string PowerAmountText
        {
            get
            {
                var result = this.PowerAmount;
                var prefix = result > 0 ? "+" : string.Empty;
                return $"{prefix}{result:0.##} {EnergyUnitPerSecondAbbreviation}";
            }
        }

        public byte PowerGridChargePercent
            => this.privateState.PowerGridChargePercent;

        public byte ThresholdShutdownPercents
            => this.privateState.ElectricityThresholds.ShutdownPercent;

        public string ThresholdShutdownPercentsText
            => ElectricityThresholdsPreset.FormatShutdownThreshold(
                this.worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer,
                this.ThresholdShutdownPercents);

        public byte ThresholdStartupPercents
            => this.privateState.ElectricityThresholds.StartupPercent;

        public string ThresholdStartupPercentsText
            => ElectricityThresholdsPreset.FormatStartupThreshold(
                this.worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer,
                this.ThresholdStartupPercents);

        public ViewModelPowerStateOverlay ViewModelPowerStateOverlay { get; }

        protected override void DisposeViewModel()
        {
            WindowPowerThresholdsConfiguration.CloseWindowIfOpened();
            base.DisposeViewModel();
        }

        private void ExecuteCommandConfigure()
        {
            var window = new WindowPowerThresholdsConfiguration(this.worldObject);
            window.EventWindowClosing += WindowClosingHandler;
            Api.Client.UI.LayoutRootChildren.Add(window);

            void WindowClosingHandler()
            {
                window.EventWindowClosing -= WindowClosingHandler;
                if (window.DialogResult == DialogResult.OK)
                {
                    var viewModel = window.ViewModel;
                    var preset = new ElectricityThresholdsPreset(
                        startupPercent: viewModel.StartupPercent,
                        shutdownPercent: viewModel.ShutdownPercent);

                    PowerGridSystem.ClientSetElectricityThresholds(this.worldObject, preset);
                }
            }
        }

        private void RefreshCurrentState()
        {
            this.NotifyPropertyChanged(nameof(this.CurrentPowerModeExplanationText));

            bool currentValueIsOn;

            switch (this.worldObject.ProtoStaticWorldObject)
            {
                case IProtoObjectElectricityProducer:
                {
                    // producer
                    var publicState = this.worldObject.GetPublicState<IObjectElectricityProducerPublicState>();
                    currentValueIsOn = publicState.ElectricityProducerState != ElectricityProducerState.PowerOff;
                    break;
                }

                case IProtoObjectElectricityConsumer:
                {
                    // consumer
                    var publicState = this.worldObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                    currentValueIsOn = publicState.ElectricityConsumerState != ElectricityConsumerState.PowerOff;
                    break;
                }

                default:
                    throw new InvalidOperationException();
            }

            if (this.isPowerOn == currentValueIsOn)
            {
                return;
            }

            this.isPowerOn = currentValueIsOn;
            this.NotifyPropertyChanged(nameof(this.IsPowerOn));
        }

        private void RefreshPowerAmountText()
        {
            if (this.IsDisposed
                || !this.worldObject.ClientHasPrivateState)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.PowerAmountText));
            ClientTimersSystem.AddAction(
                delaySeconds: 0.5,
                this.RefreshPowerAmountText);
        }

        private void RefreshPowerGridChargePercent()
        {
            this.NotifyPropertyChanged(nameof(this.PowerGridChargePercent));
            this.NotifyPropertyChanged(nameof(this.CurrentPowerModeExplanationText));
        }

        private void RefreshThresholds()
        {
            this.NotifyPropertyChanged(nameof(this.ThresholdStartupPercents));
            this.NotifyPropertyChanged(nameof(this.ThresholdShutdownPercents));
            this.NotifyPropertyChanged(nameof(this.ThresholdStartupPercentsText));
            this.NotifyPropertyChanged(nameof(this.ThresholdShutdownPercentsText));
        }
    }
}