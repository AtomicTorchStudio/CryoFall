namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;

    public static class CraftingDuration
    {
        /// <summary>
        /// 0.5 seconds
        /// </summary>
        public static readonly TimeSpan Instant = TimeSpan.FromSeconds(0.5);

        /// <summary>
        /// 1.5 minutes
        /// </summary>
        public static readonly TimeSpan Long = TimeSpan.FromSeconds(90); // 1.5 minutes

        /// <summary>
        /// 30 seconds
        /// </summary>
        public static readonly TimeSpan Medium = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 1 second
        /// </summary>
        public static readonly TimeSpan Second = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 10 seconds
        /// </summary>
        public static readonly TimeSpan Short = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 5 minutes
        /// </summary>
        public static readonly TimeSpan VeryLong = TimeSpan.FromSeconds(300); // 5 minutes

        /// <summary>
        /// 5 seconds
        /// </summary>
        public static readonly TimeSpan VeryShort = TimeSpan.FromSeconds(5);
    }
}