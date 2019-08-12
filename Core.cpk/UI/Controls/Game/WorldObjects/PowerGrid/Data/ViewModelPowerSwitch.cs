namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelPowerSwitch : BaseViewModel
    {
        private readonly IStaticWorldObject worldObject;

        private bool isPowerOn;

        public ViewModelPowerSwitch(IStaticWorldObject worldObject)
        {
            this.worldObject = worldObject;

            if (worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer)
            {
                // producer
                var publicState = worldObject.GetPublicState<IObjectElectricityProducerPublicState>();
                publicState.ClientSubscribe(
                    _ => _.ElectricityProducerState,
                    _ => this.RefreshValue(),
                    this);

                this.ViewModelPowerStateOverlay = new ViewModelPowerStateOverlay(publicState);
            }
            else if (worldObject.ProtoStaticWorldObject is IProtoObjectElectricityConsumer)
            {
                // consumer
                var publicState = worldObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                publicState.ClientSubscribe(
                    _ => _.ElectricityConsumerState,
                    _ => this.RefreshValue(),
                    this);

                this.ViewModelPowerStateOverlay = new ViewModelPowerStateOverlay(publicState);
            }
            else
            {
                throw new Exception("The object is not an electricity consumer or producer: " + this.worldObject);
            }

            this.RefreshValue();
            this.RefreshPowerAmountText();
        }

        public BaseCommand CommandPowerOff
            => new ActionCommand(
                () => this.IsPowerOn = false);

        public BaseCommand CommandPowerOn
            => new ActionCommand(
                () => this.IsPowerOn = true);

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
                               .ContinueWith(_ => this.RefreshValue(),
                                             TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public string PowerAmountText
        {
            get
            {
                if (this.worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer protoProducer)
                {
                    // producer
                    protoProducer.SharedGetElectricityProduction(this.worldObject, out var currentProduction, out _);
                    if (currentProduction > 0)
                    {
                        return "+" + currentProduction.ToString("F1");
                    }
                    else
                    {
                        return "0";
                    }
                }

                if (this.worldObject.ProtoStaticWorldObject is IProtoObjectElectricityConsumer protoConsumer)
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

                    return "-" + consumption.ToString("F1");
                }

                throw new InvalidOperationException();
            }
        }

        public ViewModelPowerStateOverlay ViewModelPowerStateOverlay { get; }

        private void RefreshPowerAmountText()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.PowerAmountText));
            ClientTimersSystem.AddAction(
                delaySeconds: 0.5,
                this.RefreshPowerAmountText);
        }

        private void RefreshValue()
        {
            bool currentValueIsOn;

            if (this.worldObject.ProtoStaticWorldObject is IProtoObjectElectricityProducer)
            {
                // producer
                var publicState = this.worldObject.GetPublicState<IObjectElectricityProducerPublicState>();
                currentValueIsOn = publicState.ElectricityProducerState != ElectricityProducerState.PowerOff;
            }
            else if (this.worldObject.ProtoStaticWorldObject is IProtoObjectElectricityConsumer)
            {
                // consumer
                var publicState = this.worldObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                currentValueIsOn = publicState.ElectricityConsumerState == ElectricityConsumerState.PowerOn;
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (this.isPowerOn == currentValueIsOn)
            {
                return;
            }

            this.isPowerOn = currentValueIsOn;
            this.NotifyPropertyChanged(nameof(this.IsPowerOn));
        }
    }
}