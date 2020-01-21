namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;

    public class GeneralOptionTooltipDelay
        : ProtoOptionCombobox<GeneralOptionsCategory,
            GeneralOptionTooltipDelay.TooltipDuration>
    {
        public enum TooltipDuration : byte
        {
            [Description(CoreStrings.Duration_Instant)]
            Instant = 0,

            [Description(CoreStrings.Duration_VeryQuick)]
            VeryQuick = 10,

            [Description(CoreStrings.Duration_Quick)]
            Quick = 20,

            [Description(CoreStrings.Duration_Slow)]
            Slow = 30,

            [Description(CoreStrings.Duration_VerySlow)]
            VerySlow = 40
        }

        public override TooltipDuration DefaultEnumValue => TooltipDuration.Instant;

        public override string Name => "Tooltip delay";

        public override IProtoOption OrderAfterOption
            => GetOption<GeneralOptionMouseScrollWheelMode>();

        public override TooltipDuration ValueProvider { get; set; }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            Client.Input.TooltipDelay = GetTooltipDelay(this.CurrentValue);
        }

        private static double GetTooltipDelay(TooltipDuration value)
        {
            switch (value)
            {
                default:
                    return 0.092; // instant

                case TooltipDuration.VeryQuick:
                    return 0.2;

                case TooltipDuration.Quick:
                    return 0.3;

                case TooltipDuration.Slow:
                    return 0.5;

                case TooltipDuration.VerySlow:
                    return 0.75;
            }
        }
    }
}