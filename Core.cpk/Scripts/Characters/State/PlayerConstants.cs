namespace AtomicTorch.CBND.CoreMod.Characters
{
    public static class PlayerConstants
    {
        /// <summary>
        /// The number of the inventory slots is configured here.
        /// Please note that it doesn't apply to the already registered players (their inventory container is already created).
        /// </summary>
        public const int InventorySlotsCount = 40;
        /// <summary>
        /// The number of the hotbar slots is configured here.
        /// Please note that it doesn't apply to the already registered players (their hotbar container is already created).
        /// Also note if this value is odd then it will create a graphical bug in the display of the hotbar. See https://i.imgur.com/Dl6sRBi.png
        /// </summary>
        public const int HotbarSlotsCount = 10;
    }
}