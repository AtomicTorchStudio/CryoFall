namespace AtomicTorch.CBND.CoreMod.ClientComponents.Input
{
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public struct ButtonMapping
    {
        public static readonly ButtonMapping None = default;

        public readonly InputKey PrimaryKey;

        public readonly InputKey SecondaryKey;

        public ButtonMapping(InputKey primaryKey, InputKey secondaryKey)
        {
            this.PrimaryKey = primaryKey;
            this.SecondaryKey = secondaryKey;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}: {3}",
                                 nameof(this.PrimaryKey),
                                 this.PrimaryKey,
                                 nameof(this.SecondaryKey),
                                 this.SecondaryKey);
        }
    }
}