namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    public class GeneralOptionDisplayHealthbarAboveCurrentCharacter : ProtoOptionCheckbox<GeneralOptionsCategory>
    {
        public static bool IsDisplay { get; private set; }

        public override bool Default => false;

        public override string Description =>
            "Normally the game displays healthbars only over the other characters. You can enable this option to display it over your character, as well.";

        public override string Name =>
            @"Show healthbar
              [br]above my character";

        public override IProtoOption OrderAfterOption
            => GetOption<GeneralOptionDisplayObjectInteractionTooltip>();

        public override bool ValueProvider
        {
            get => IsDisplay;
            set => IsDisplay = value;
        }
    }
}