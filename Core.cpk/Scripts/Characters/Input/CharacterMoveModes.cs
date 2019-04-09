namespace AtomicTorch.CBND.CoreMod.Characters.Input
{
    using System;

    [Flags]
    public enum CharacterMoveModes : byte
    {
        None = 0,

        Up = 1 << 0,

        Down = 1 << 1,

        Left = 1 << 2,

        Right = 1 << 3,

        ModifierRun = 1 << 4
    }
}