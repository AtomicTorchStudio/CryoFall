namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    // Please note: the values here are not constants to allow customized behavior for modding 
    public static class MedicineCooldownDuration
    {
        public const double None = 0;

        public static double Long => 7;

        /// <summary>
        /// This is just a synonym for the most long value.
        /// Used as max value for the status effect.
        /// Please note: cooldown duration for any medicine cannot exceed this duration.
        /// </summary>
        public static double Maximum => VeryLong;

        public static double Medium => 5;

        public static double Short => 3;

        public static double VeryLong => 10;
    }
}