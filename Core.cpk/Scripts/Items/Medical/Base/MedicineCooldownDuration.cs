namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    // Please note: the values here are not constants to allow customized behavior for modding 
    public static class MedicineCooldownDuration
    {
        public const double None = 0;

        public static double Long => 8;

        /// <summary>
        /// This is just a synonym for the most long value.
        /// Used as max value for the status effect.
        /// Please note: cooldown duration for any medicine cannot exceed this duration.
        /// </summary>
        public static double Maximum => VeryLong;

        public static double Medium => 6;

        public static double Short => 4;

        public static double VeryLong => 11;
    }
}