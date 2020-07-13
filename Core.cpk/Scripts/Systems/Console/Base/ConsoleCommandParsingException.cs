namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;

    public class ConsoleCommandParsingException : Exception
    {
        public ConsoleCommandParsingException(string message) : base(message)
        {
        }
    }
}