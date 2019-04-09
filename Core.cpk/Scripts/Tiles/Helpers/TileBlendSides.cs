namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System;

    [Flags]
    public enum TileBlendSides : byte
    {
        None = 0,

        Left = 1,

        Right = 2,

        Up = 4,

        Down = 8,

        UpLeft = 16,

        UpRight = 32,

        DownLeft = 64,

        DownRight = 128
    }
}