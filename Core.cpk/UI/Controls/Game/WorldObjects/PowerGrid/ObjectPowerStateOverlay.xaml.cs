namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid
{
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ObjectPowerStateOverlay : BaseUserControl
    {
        private ViewModelPowerStateOverlay viewModel;

        private IStaticWorldObject worldObject;

        public static void CreateFor(
            IStaticWorldObject worldObject)
        {
            var control = new ObjectPowerStateOverlay();
            control.Setup(worldObject);

            var positionOffset = worldObject.ProtoStaticWorldObject
                                            .SharedGetObjectCenterWorldOffset(worldObject)
                                 + (0, 0.815);

            Api.Client.UI.AttachControl(
                worldObject,
                control,
                positionOffset: positionOffset,
                isFocusable: false);
        }

        public void Setup(IStaticWorldObject worldObject)
        {
            this.worldObject = worldObject;
        }

        protected override void OnLoaded()
        {
            switch (this.worldObject.ProtoStaticWorldObject)
            {
                case IProtoObjectElectricityProducer:
                {
                    var publicState = this.worldObject.GetPublicState<IObjectElectricityProducerPublicState>();
                    this.viewModel = new ViewModelPowerStateOverlay(publicState);
                    break;
                }

                case IProtoObjectElectricityConsumer:
                {
                    var publicState = this.worldObject.GetPublicState<IObjectElectricityConsumerPublicState>();
                    this.viewModel = new ViewModelPowerStateOverlay(publicState);
                    break;
                }

                default:
                    Api.Logger.Error("The object is not an electricity consumer or producer: " + this.worldObject);
                    return;
            }

            this.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }
    }
};