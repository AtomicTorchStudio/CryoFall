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

    public class ClientWorldMapLastVehicleVisualizer : IWorldMapVisualizer
    {
        private readonly WorldMapController worldMapController;

        private PlayerCharacterPrivateState characterPrivateState;

        private PlayerCharacterPublicState characterPublicState;

        private bool isEnabled;

        private FrameworkElement marker;

        private IStateSubscriptionOwner subscriptionStorage;

        public ClientWorldMapLastVehicleVisualizer(WorldMapController worldMapController)
        {
            this.worldMapController = worldMapController;
        }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.isEnabled = value;

                if (this.isEnabled)
                {
                    this.Initialize();
                }
                else
                {
                    this.Reset();
                }
            }
        }

        public void Dispose()
        {
            this.Reset();
        }

        private void AddOrUpdateMarker(LastDismountedVehicleMapMark vehicleMapMark)
        {
            var mapControl = new WorldMapMarkLastVehicle();
            var canvasPosition = this.worldMapController.WorldToCanvasPosition(vehicleMapMark.Position);
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);
            Panel.SetZIndex(mapControl, 9);

            mapControl.ImageSource = Api.Client.UI.GetTextureBrush(vehicleMapMark.ProtoVehicle.MapIcon)
                                        .TextureSource;

            this.worldMapController.AddControl(mapControl);
            this.marker = mapControl;
        }

        private void Initialize()
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

        private void Refresh()
        {
            if (!this.isEnabled)
            {
                return;
            }

            if (this.characterPublicState.CurrentVehicle is not null)
            {
                this.RemoveMarker();
                return;
            }

            var lastUsedVehicleMapMark = this.characterPrivateState.LastDismountedVehicleMapMark;
            if (lastUsedVehicleMapMark.ProtoVehicle is null)
            {
                this.RemoveMarker();
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

            this.worldMapController.RemoveControl(this.marker);
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