namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Camera;
    using AtomicTorch.CBND.CoreMod.ClientComponents.InputListeners;

    public class GeneralOptionMouseScrollWheelMode
        : ProtoOptionCombobox<GeneralOptionsCategory,
            GeneralOptionMouseScrollWheelMode.Mode>
    {
        public enum Mode : byte
        {
            [Description("Camera zoom")]
            CameraZoom = 0,

            [Description("Hotbar item selection")]
            HotbarItemSelection = 1
        }

        public override Mode DefaultEnumValue => Mode.CameraZoom;

        public override string Name => "Use scroll wheel for";

        public override IProtoOption OrderAfterOption 
            => GetOption<GeneralOptionScreenshotResolution>();

        public override Mode ValueProvider { get; set; }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            var isCameraZoom = this.CurrentValue == Mode.CameraZoom;
            ClientComponentWorldCameraZoomManager.IsProcessingMouseWheel = isCameraZoom;
            ClientComponentHotbarHelper.IsProcessingMouseWheel = !isCameraZoom;
        }
    }
}