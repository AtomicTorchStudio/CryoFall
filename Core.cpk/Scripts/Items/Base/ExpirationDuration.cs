namespace AtomicTorch.CBND.CoreMod.Items
{
    using System;

    public static class ExpirationDuration
    {
        /// <summary>
        /// Should be used for things that last a bit longer than normal.
        /// </summary>
        public static readonly TimeSpan Lasting = TimeSpan.FromHours(12);

        /// <summary>
        /// Should be used for food that naturally lasts a long time.
        /// </summary>
        public static readonly TimeSpan LongLasting = TimeSpan.FromDays(1.5);

        /// <summary>
        /// Should be used for all normal food, such as vegetables, prepared meals, etc.
        /// </summary>
        public static readonly TimeSpan Normal = TimeSpan.FromHours(5);

        /// <summary>
        /// Must be used for food that spoils very quickly such as fresh meat, picked berries, etc.
        /// </summary>
        public static readonly TimeSpan Perishable = TimeSpan.FromHours(2);

        /// <summary>
        /// Should be used for food that has been preserved in some way to last much much longer.
        /// </summary>
        public static readonly TimeSpan Preserved = TimeSpan.FromDays(3);

        /// <summary>
        /// Should be used for food that doesn't spoil at all.
        /// </summary>
        public static readonly TimeSpan Unlimited = TimeSpan.Zero;
    }
}