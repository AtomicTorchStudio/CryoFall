namespace AtomicTorch.CBND.CoreMod.Helpers.Primitives
{
    public static class CliffTextureRegion
    {
        public static readonly ColumnRow BottomCenter = new ColumnRow(1, 2);

        public static readonly ColumnRow BottomLeft = new ColumnRow(0, 2);

        public static readonly ColumnRow BottomLeftInnerCorner = new ColumnRow(4, 0);

        /// <summary>
        /// Both inner corner at left and right bottom.
        /// </summary>
        public static readonly ColumnRow BottomLeftRightInnerCorner = new ColumnRow(5, 0);

        public static readonly ColumnRow BottomRight = new ColumnRow(2, 2);

        public static readonly ColumnRow BottomRightInnerCorner = new ColumnRow(3, 0);

        /// <summary>
        /// Corner from all sides.
        /// </summary>
        public static readonly ColumnRow FourSidesInnerCorner = new ColumnRow(5, 2);

        public static readonly ColumnRow MiddleTiledLeft = new ColumnRow(0, 1);

        public static readonly ColumnRow MiddleTiledRight = new ColumnRow(2, 1);

        public static readonly ColumnRow SlopeBottomLeft = new ColumnRow(0, 3);

        public static readonly ColumnRow SlopeBottomMiddleTiled = new ColumnRow(1, 3);

        public static readonly ColumnRow SlopeBottomRight = new ColumnRow(2, 3);

        public static readonly ColumnRow SlopeTopLeft = new ColumnRow(3, 3);

        public static readonly ColumnRow SlopeTopMiddleTiled = new ColumnRow(4, 3);

        public static readonly ColumnRow SlopeTopRight = new ColumnRow(5, 3);

        /// <summary>
        /// Both inner corner at left top and bottom.
        /// </summary>
        public static readonly ColumnRow TopBottomLeftInnerCorner = new ColumnRow(4, 2);

        /// <summary>
        /// Both inner corner at right top and bottom.
        /// </summary>
        public static readonly ColumnRow TopBottomRightInnerCorner = new ColumnRow(3, 2);

        public static readonly ColumnRow TopCenter = new ColumnRow(1, 0);

        public static readonly ColumnRow TopLeftCorner = new ColumnRow(0, 0);

        public static readonly ColumnRow TopLeftInnerCorner = new ColumnRow(4, 1);

        /// <summary>
        /// Both inner corner at left and right top.
        /// </summary>
        public static readonly ColumnRow TopLeftRightInnerCorner = new ColumnRow(5, 1);

        public static readonly ColumnRow TopRightCorner = new ColumnRow(2, 0);

        public static readonly ColumnRow TopRightInnerCorner = new ColumnRow(3, 1);
    }
}