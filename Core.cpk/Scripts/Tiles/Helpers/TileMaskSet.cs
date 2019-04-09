namespace AtomicTorch.CBND.CoreMod.Tiles
{
    public struct TileMaskSet
    {
        public readonly bool IsFlipHorizontally;

        public readonly bool IsFlipVertically;

        public readonly ushort TextureMaskArraySlice;

        public TileMaskSet(
            ushort textureMaskArraySlice,
            bool isFlipHorizontally = false,
            bool isFlipVertically = false)
        {
            this.TextureMaskArraySlice = textureMaskArraySlice;
            this.IsFlipHorizontally = isFlipHorizontally;
            this.IsFlipVertically = isFlipVertically;
        }
    }
}