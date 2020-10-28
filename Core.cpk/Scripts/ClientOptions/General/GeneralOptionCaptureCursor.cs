namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    public class GeneralOptionCaptureCursor
        : ProtoOptionCheckbox<GeneralOptionsCategory>
    {
        public override bool Default => false;

        public override string Description =>
            "When enabled, it will ensure that the mouse cursor cannot be moved outside of the game window while the game window is focused unless the Ctrl key is held. Useful for multi-monitor setup or when playing in a windowed mode.";

        public override string Name => "Capture cursor";

        public override IProtoOption OrderAfterOption
            => GetOption<GeneralOptionDisplayObjectInteractionTooltip>();

        public override bool ValueProvider
        {
            get => Client.Input.IsCursorCaptured;
            set => Client.Input.IsCursorCaptured = value;
        }
    }
}