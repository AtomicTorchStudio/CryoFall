namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowFridge : BaseViewModel
    {
        public const string PerishableItemsStorageDurationFormat =
            "Perishable items store ~{0} times longer.";

        private IObjectElectricityConsumerPublicState publicStatePowerConsumer;

        public ViewModelWindowFridge(
            IStaticWorldObject worldObjectFridge,
            ObjectCratePrivateState privateState,
            Action callbackCloseWindow)
        {
            this.WorldObjectFridge = worldObjectFridge;
            this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(
                privateState.ItemsContainer,
                callbackTakeAllItemsSuccess: callbackCloseWindow);

            if (this.WorldObjectFridge.ProtoStaticWorldObject is IProtoObjectElectricityConsumer)
            {
                this.publicStatePowerConsumer = this.WorldObjectFridge
                                                    .GetPublicState<IObjectElectricityConsumerPublicState>();
                this.publicStatePowerConsumer?.ClientSubscribe(
                    _ => _.ElectricityConsumerState,
                    _ => this.NotifyPropertyChanged(nameof(this.IsActive)),
                    this);
            }
        }

        public bool IsActive
        {
            get
            {
                if (this.publicStatePowerConsumer is null)
                {
                    return true; // not a power consumer - always on
                }

                return this.publicStatePowerConsumer.ElectricityConsumerState == ElectricityConsumerState.PowerOnActive;
            }
        }

        public string PerishableItemsStorageDurationText
        {
            get
            {
                // get actual number
                //var rate = ((IProtoItemsContainerFridge)this
                //                                        .ViewModelItemsContainerExchange.Container.ProtoItemsContainer)
                //    .SharedGetCurrentFoodFreshnessDecreaseCoefficient(this.ViewModelItemsContainerExchange.Container);
                //var resultMult = Math.Round(1.0 / rate,
                //                        digits: 2,
                //                        MidpointRounding.AwayFromZero)
                //                 .ToString("0.##");

                // get max number
                var protoFridge = ((IProtoObjectFridge)this.WorldObjectFridge.ProtoStaticWorldObject);
                var resultMult = protoFridge.FreshnessDurationMultiplier;

                var resultText = Math.Round(resultMult,
                                            digits: 2,
                                            MidpointRounding.AwayFromZero)
                                     .ToString("0.##");

                resultText = string.Format(PerishableItemsStorageDurationFormat, resultText);
                return resultText;
            }
        }

        public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }

        public IStaticWorldObject WorldObjectFridge { get; }
    }
}