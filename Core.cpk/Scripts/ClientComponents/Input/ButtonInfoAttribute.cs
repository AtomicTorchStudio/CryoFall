namespace AtomicTorch.CBND.CoreMod.ClientComponents.Input
{
    using System;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ButtonInfoAttribute : Attribute
    {
        public readonly InputKey KeyPrimary;

        public readonly InputKey KeySecondary;

        public ButtonInfoAttribute(
            InputKey keyPrimary = InputKey.None,
            InputKey keySecondary = InputKey.None)
        {
            this.KeyPrimary = keyPrimary;
            this.KeySecondary = keySecondary;
        }

        public string Category { get; set; }

        public ButtonMapping DefaultButtonMapping => new ButtonMapping(this.KeyPrimary, this.KeySecondary);

        public string Title { get; set; }
    }
}