namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleRepairKitSystem;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class VehicleRepairKitTooltip : BaseUserControl
    {
        public static readonly DependencyProperty CanInteractProperty =
            DependencyProperty.Register(nameof(CanInteract),
                                        typeof(bool),
                                        typeof(VehicleRepairKitTooltip),
                                        new PropertyMetadata(default(bool)));

        public bool CanInteract
        {
            get => (bool)this.GetValue(CanInteractProperty);
            set => this.SetValue(CanInteractProperty, value);
        }

        public IDynamicWorldObject WorldObject { get; private set; }

        public static IComponentAttachedControl CreateAndAttach(IDynamicWorldObject vehicle)
        {
            var control = new VehicleRepairKitTooltip();
            control.WorldObject = vehicle;

            var centerOffset = vehicle.ProtoWorldObject.SharedGetObjectCenterWorldOffset(vehicle);

            return Api.Client.UI.AttachControl(
                vehicle,
                control,
                positionOffset: centerOffset + (0, 0.56),
                isFocusable: true);
        }

        protected override void OnLoaded()
        {
            ClientUpdateHelper.UpdateCallback += this.Update;
            this.Update();
        }

        protected override void OnUnloaded()
        {
            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private void Update()
        {
            this.CanInteract = VehicleRepairKitSystem.SharedCheckCanInteract(
                Api.Client.Characters.CurrentPlayerCharacter,
                this.WorldObject,
                writeToLog: false);
        }
    }
}