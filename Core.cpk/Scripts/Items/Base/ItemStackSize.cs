namespace AtomicTorch.CBND.CoreMod.Items
{
    /// <summary>
    /// Standard stack sizes used in the game for all items.
    /// </summary>
    public static class ItemStackSize
    {
        /// <summary>
        /// Big item stack - 250 max.
        /// Used for ores and other things that are acquired in large quantities.
        /// </summary>
        public const ushort Big = 250;

        /// <summary>
        /// Highest possible size for the item stack (1000).
        /// Used for things like coins.
        /// </summary>
        public const ushort Huge = 1000;

        /// <summary>
        /// Medium item stack - 100 max.
        /// Used as default stack size for most item types.
        /// </summary>
        public const ushort Medium = 100;

        /// <summary>
        /// Only one item per stack.
        /// Please do NOT change this otherwise it will cause issues with items that have durability, ammo, etc.
        /// </summary>
        public const ushort Single = 1;

        /// <summary>
        /// Small item stack - 10 max.
        /// Used for items that we want to be stored in smaller numbers, such as food, meds, etc.
        /// </summary>
        public const ushort Small = 10;
    }
}