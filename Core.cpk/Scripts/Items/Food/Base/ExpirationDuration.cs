namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    internal class ExpirationDuration
    {
        /// <summary>
        /// Should be used for things that last a bit longer than normal.
        /// </summary>
        public static readonly TimeSpan NonPerishable = TimeSpan.FromHours(8);

        /// <summary>
        /// Should be used for all normal food, such as vegetables, prepared meals, etc.
        /// </summary>
        public static readonly TimeSpan Normal = TimeSpan.FromHours(4);

        /// <summary>
        /// Must be used for food that spoils very quickly such as fresh meat, picked berries, etc.
        /// </summary>
        public static readonly TimeSpan Perishable = TimeSpan.FromHours(1.5);

        /// <summary>
        /// Should be used for food that has been preserved in some way to last much longer.
        /// </summary>
        public static readonly TimeSpan Preserved = TimeSpan.FromDays(2.5);

        /// <summary>
        /// Should be used for food that doesn't spoil at all.
        /// </summary>
        public static readonly TimeSpan Unlimited = TimeSpan.Zero;
    }
}