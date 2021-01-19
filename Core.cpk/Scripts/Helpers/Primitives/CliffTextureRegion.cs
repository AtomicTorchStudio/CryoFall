namespace AtomicTorch.CBND.CoreMod.Helpers.Primitives
{
    public static class CliffTextureRegion
    {
        public static readonly ColumnRow BottomCenter = new(1, 2);

        public static readonly ColumnRow BottomLeft = new(0, 2);

        public static readonly ColumnRow BottomLeftInnerCorner = new(4, 0);

        /// <summary>
        /// Both inner corner at left and right bottom.
        /// </summary>
        public static readonly ColumnRow BottomLeftRightInnerCorner = new(5, 0);

        public static readonly ColumnRow BottomRight = new(2, 2);

        public static readonly ColumnRow BottomRightInnerCorner = new(3, 0);

        /// <summary>
        /// Corner from all sides.
        /// </summary>
        public static readonly ColumnRow FourSidesInnerCorner = new(5, 2);

        public static readonly ColumnRow MiddleTiledLeft = new(0, 1);

        public static readonly ColumnRow MiddleTiledRight = new(2, 1);

        public static readonly ColumnRow SlopeBottomLeft = new(0, 3);

        public static readonly ColumnRow SlopeBottomMiddleTiled = new(1, 3);

        public static readonly ColumnRow SlopeBottomRight = new(2, 3);

        public static readonly ColumnRow SlopeTopLeft = new(3, 3);

        public static readonly ColumnRow SlopeTopMiddleTiled = new(4, 3);

        public static readonly ColumnRow SlopeTopRight = new(5, 3);

        /// <summary>
        /// Both inner corner at left top and bottom.
        /// </summary>
        public static readonly ColumnRow TopBottomLeftInnerCorner = new(4, 2);

        /// <summary>
        /// Both inner corner at right top and bottom.
        /// </summary>
        public static readonly ColumnRow TopBottomRightInnerCorner = new(3, 2);

        public static readonly ColumnRow TopCenter = new(1, 0);

        public static readonly ColumnRow TopLeftCorner = new(0, 0);

        public static readonly ColumnRow TopLeftInnerCorner = new(4, 1);

        /// <summary>
        /// Both inner corner at left and right top.
        /// </summary>
        public static readonly ColumnRow TopLeftRightInnerCorner = new(5, 1);

        public static readonly ColumnRow TopRightCorner = new(2, 0);

        public static readonly ColumnRow TopRightInnerCorner = new(3, 1);
    }
}