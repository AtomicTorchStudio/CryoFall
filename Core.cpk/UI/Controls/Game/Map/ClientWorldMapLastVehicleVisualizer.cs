namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ClientWorldMapLastVehicleVisualizer : BaseWorldMapVisualizer
    {
        private PlayerCharacterPrivateState characterPrivateState;

        private PlayerCharacterPublicState characterPublicState;

        private FrameworkElement marker;

        private IStateSubscriptionOwner subscriptionStorage;

        public ClientWorldMapLastVehicleVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
        }

        protected override void DisposeInternal()
        {
        }

        protected override void OnDisable()
        {
            this.Reset();
        }

        protected override void OnEnable()
        {
            this.Reset();

            this.subscriptionStorage = new StateSubscriptionStorage();

            this.characterPrivateState = ClientCurrentCharacterHelper.PrivateState;
            this.characterPrivateState.ClientSubscribe(_ => _.LastDismountedVehicleMapMark,
                                                       _ => this.Refresh(),
                                                       this.subscriptionStorage);

            this.characterPublicState = ClientCurrentCharacterHelper.PublicState;
            this.characterPublicState.ClientSubscribe(_ => _.CurrentVehicle,
                                                      _ => this.Refresh(),
                                                      this.subscriptionStorage);

            this.Refresh();
        }

        private void AddOrUpdateMarker(LastDismountedVehicleMapMark vehicleMapMark)
        {
            var mapControl = new WorldMapMarkLastVehicle();
            var canvasPosition = this.WorldToCanvasPosition(vehicleMapMark.Position
                                                            + (0, vehicleMapMark.ProtoVehicle.VehicleWorldHeight));
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);
            Panel.SetZIndex(mapControl, 9);

            mapControl.ImageSource = Api.Client.UI.GetTextureBrush(vehicleMapMark.ProtoVehicle.MapIcon)
                                        .TextureSource;

            this.AddControl(mapControl);
            this.marker = mapControl;
        }

        private void Refresh()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            this.RemoveMarker();

            if (this.characterPublicState.CurrentVehicle is not null)
            {
                return;
            }

            var lastUsedVehicleMapMark = this.characterPrivateState.LastDismountedVehicleMapMark;
            if (lastUsedVehicleMapMark.ProtoVehicle is null)
            {
                return;
            }

            this.AddOrUpdateMarker(lastUsedVehicleMapMark);
        }

        private void RemoveMarker()
        {
            if (this.marker is null)
            {
                return;
            }

            this.RemoveControl(this.marker);
            this.marker = null;
        }

        private void Reset()
        {
            this.subscriptionStorage?.Dispose();
            this.subscriptionStorage = null;
            this.RemoveMarker();
        }
    }
}