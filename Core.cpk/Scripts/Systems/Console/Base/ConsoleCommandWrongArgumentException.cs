namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;

    public class ConsoleCommandWrongArgumentException : Exception
    {
        public readonly int Index;

        public readonly bool IsMissing;

        public ConsoleCommandWrongArgumentException(int index, bool isMissing)
        {
            this.Index = index;
            this.IsMissing = isMissing;
        }
    }
}