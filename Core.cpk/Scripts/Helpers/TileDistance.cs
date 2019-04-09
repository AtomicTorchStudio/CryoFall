namespace AtomicTorch.CBND.CoreMod.Helpers
{
    public static class TileDistance
    {
        /// <summary>
        /// The distance of diagonal neighbor cells will be calculated as sqrt(2)==1.414. That's why we're using 1.5 units.
        /// </summary>
        public const double OneCellAround = 1.5;
    }
}