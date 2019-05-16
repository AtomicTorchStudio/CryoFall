namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    public class GeneralOptionDisplayObjectInteractionTooltip : ProtoOptionCheckbox<GeneralOptionsCategory>
    {
        public static bool IsDisplay { get; private set; }

        public override bool Default => true;

        public override string Name =>
            "Display object[br]interaction tooltip";

        public override IProtoOption OrderAfterOption => GetOption<GeneralOptionDisplayObjectNameTooltip>();

        public override bool ValueProvider
        {
            get => IsDisplay;
            set => IsDisplay = value;
        }
    }
}