namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    public static class MedicineCooldownDuration
    {
        public const double Long = 7;

        /// <summary>
        /// This is just a synonym for the most long value.
        /// Used as max value for the status effect.
        /// </summary>
        public const double Maximum = VeryLong;

        public const double Medium = 5;

        public const double None = 0;

        public const double Short = 3;

        public const double VeryLong = 10;
    }
}