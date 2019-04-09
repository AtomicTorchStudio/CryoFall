namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    public class GeneralOptionUIScale : ProtoOptionSlider<GeneralOptionsCategory>
    {
        public override double Default => 1;

        public override double Maximum => 1.2;

        public override double Minimum => 0.7;

        public override string Name => "UI scale";

        public override double StepSize => 0.05;

        public override double ValueProvider
        {
            get => Client.UI.Scale;
            set => Client.UI.Scale = (float)value;
        }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            Client.UI.Scale = (float)this.CurrentValue;
        }
    }
}