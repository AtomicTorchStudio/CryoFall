namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Turrets
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Turrets.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class TurretNoAmmoOverlay : BaseUserControl
    {
        private ViewModelTurretNoAmmoOverlay viewModel;

        private IStaticWorldObject worldObject;

        public static void CreateFor(
            IStaticWorldObject worldObject)
        {
            var control = new TurretNoAmmoOverlay();
            control.Setup(worldObject);

            var positionOffset = worldObject.ProtoStaticWorldObject
                                            .SharedGetObjectCenterWorldOffset(worldObject)
                                 + (0, 0.415);

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
            this.viewModel = new ViewModelTurretNoAmmoOverlay(this.worldObject);
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