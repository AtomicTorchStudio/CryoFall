namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelPowerStateOverlay : BaseViewModel
    {
        private readonly IObjectElectricityConsumerPublicState consumerPublicState;

        private readonly IObjectElectricityProducerPublicState producerPublicState;

        public ViewModelPowerStateOverlay(IObjectElectricityConsumerPublicState consumerPublicState)
        {
            this.consumerPublicState = consumerPublicState;
            this.consumerPublicState.ClientSubscribe(_ => _.ElectricityConsumerState,
                                                     _ => this.Refresh(),
                                                     this);

            this.Refresh();
            this.Update();
            ClientUpdateHelper.UpdateCallback += this.Update;
        }

        public ViewModelPowerStateOverlay(IObjectElectricityProducerPublicState producerPublicState)
        {
            this.producerPublicState = producerPublicState;
            this.producerPublicState.ClientSubscribe(_ => _.ElectricityProducerState,
                                                     _ => this.Refresh(),
                                                     this);

            this.Refresh();
            this.Update();
            ClientUpdateHelper.UpdateCallback += this.Update;
        }

        public bool IsPowerOff { get; private set; }

        public bool IsPowerOn { get; private set; } = true;

        public bool IsPowerOutage { get; private set; }

        public bool IsVisible { get; private set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private void Refresh()
        {
            if (this.consumerPublicState != null)
            {
                switch (this.consumerPublicState.ElectricityConsumerState)
                {
                    case ElectricityConsumerState.PowerOff:
                        this.IsPowerOn = false;
                        this.IsPowerOff = true;
                        this.IsPowerOutage = false;
                        return;

                    case ElectricityConsumerState.PowerOffOutage:
                        this.IsPowerOn = false;
                        this.IsPowerOff = false;
                        this.IsPowerOutage = true;
                        return;

                    case ElectricityConsumerState.PowerOn:
                        this.IsPowerOn = true;
                        this.IsPowerOff = false;
                        this.IsPowerOutage = false;
                        return;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            switch (this.producerPublicState.ElectricityProducerState)
            {
                case ElectricityProducerState.PowerOff:
                    this.IsPowerOn = false;
                    this.IsPowerOff = true;
                    this.IsPowerOutage = false;
                    return;

                case ElectricityProducerState.PowerOnIdle:
                case ElectricityProducerState.PowerOnActive:
                    this.IsPowerOn = true;
                    this.IsPowerOff = false;
                    this.IsPowerOutage = false;
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            if (ClientComponentObjectInteractionHelper.MouseOverObject
                == (this.consumerPublicState?.GameObject ?? this.producerPublicState?.GameObject))
            {
                // mouse over - always display the power consumer status icon
                this.IsVisible = true;
                return;
            }

            if (this.IsPowerOutage)
            {
                // power outage - flicker every half second
                var time = Client.Core.ClientRealTime % 1.0;
                this.IsVisible = time < 0.5;
                return;
            }

            this.IsVisible = false;
        }
    }
}