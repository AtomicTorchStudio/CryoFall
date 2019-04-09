namespace AtomicTorch.CBND.CoreMod.Helpers.Primitives
{
    using System;

    [Flags]
    public enum NeighborsPattern : ushort
    {
        None = 0,

        Left = 1 << 0,

        Top = 1 << 1,

        Right = 1 << 2,

        Bottom = 1 << 3,

        TopLeft = 1 << 4,

        TopRight = 1 << 5,

        BottomLeft = 1 << 6,

        BottomRight = 1 << 7
    }
}