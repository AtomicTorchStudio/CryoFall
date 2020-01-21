namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    public class GeneralOptionDisplayObjectNameTooltip : ProtoOptionCheckbox<GeneralOptionsCategory>
    {
        public static bool IsDisplay { get; private set; }

        public override bool Default => true;

        public override string Name => "Display object[br]name tooltip";

        public override IProtoOption OrderAfterOption
            => GetOption<GeneralOptionTooltipDelay>();

        public override bool ValueProvider
        {
            get => IsDisplay;
            set => IsDisplay = value;
        }
    }
}