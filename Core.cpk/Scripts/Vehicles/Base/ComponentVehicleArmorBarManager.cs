namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ComponentVehicleArmorBarManager : ClientComponent
    {
        private static readonly ICoreClientService Core = Api.Client.Core;

        private static readonly IInputClientService Input = Api.Client.Input;

        private readonly IStateSubscriptionOwner subscriptionHolder = new StateSubscriptionStorage();

        private IComponentAttachedControl componentHealthbar;

        private double lastStructurePointsValueUpdateTimestamp;

        private IDynamicWorldObject vehicle;

        public bool IsDisplayedOnlyOnMouseOver { get; set; }

        public void Setup(IDynamicWorldObject vehicle)
        {
            this.Unsubscribe();
            this.vehicle = vehicle;
            this.Subscribe();
            this.Refresh();
        }

        public override void Update(double deltaTime)
        {
            this.Refresh();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.Unsubscribe();
            this.DestroyControl();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.Subscribe();
            this.Refresh();
        }

        private bool CheckIsBarShouldBeVisible()
        {
            if (this.vehicle is null)
            {
                return false;
            }

            if (ClientInputManager.IsButtonHeld(GameButton.DisplayLandClaim)
                || Input.IsKeyHeld(InputKey.Alt, evenIfHandled: true)
                || Api.IsEditor && Client.Characters.IsCurrentPlayerCharacterSpectator
                || (this.lastStructurePointsValueUpdateTimestamp > 0
                    && Core.ClientRealTime - this.lastStructurePointsValueUpdateTimestamp
                    < ClientComponentAutoDisplayStructurePointsBar.SecondsToDisplayHealthbarAfterDamage))
            {
                return true;
            }

            if (this.IsDisplayedOnlyOnMouseOver
                && (!ReferenceEquals(ClientComponentObjectInteractionHelper.MouseOverObject,
                                     this.vehicle)))
            {
                return false;
            }

            return true;
        }

        private void DestroyControl()
        {
            if (this.componentHealthbar is null)
            {
                return;
            }

            this.componentHealthbar.Destroy();
            this.componentHealthbar = null;
        }

        private void Refresh()
        {
            var isBarVisible = this.CheckIsBarShouldBeVisible();
            if (!isBarVisible)
            {
                this.DestroyControl();
                return;
            }

            // health bar should be visible
            if (!(this.componentHealthbar is null))
            {
                return;
            }

            // create control
            var protoVehicle = (IProtoVehicle)this.vehicle.ProtoGameObject;
            var vehiclePublicState = this.vehicle.GetPublicState<VehiclePublicState>();

            var structurePointsBarControl = new VehicleArmorBarControl();
            structurePointsBarControl.Setup(
                new ObjectStructurePointsData(this.vehicle, protoVehicle.SharedGetStructurePointsMax(this.vehicle)),
                vehiclePublicState.StructurePointsCurrent);

            this.componentHealthbar = Api.Client.UI.AttachControl(
                this.vehicle,
                structurePointsBarControl,
                positionOffset: protoVehicle.SharedGetObjectCenterWorldOffset(
                                    this.vehicle)
                                + (0, 0.545),
                isFocusable: false);
        }

        private void Subscribe()
        {
            if (!this.IsEnabled
                || this.vehicle is null)
            {
                return;
            }

            var state = this.vehicle.GetPublicState<VehiclePublicState>();
            state.ClientSubscribe(
                _ => _.StructurePointsCurrent,
                _ =>
                {
                    this.lastStructurePointsValueUpdateTimestamp = Core.ClientRealTime;
                    this.Refresh();
                },
                this.subscriptionHolder);
        }

        private void Unsubscribe()
        {
            this.subscriptionHolder.ReleaseSubscriptions();
            this.lastStructurePointsValueUpdateTimestamp = 0;
        }
    }
}