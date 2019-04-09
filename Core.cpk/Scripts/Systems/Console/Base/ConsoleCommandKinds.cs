namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;

    [Flags]
    public enum ConsoleCommandKinds : byte
    {
        Client = 1 << 0,

        /// <summary>
        /// Require server operator access right from the command executing player.
        /// </summary>
        ServerOperator = 1 << 1,

        /// <summary>
        /// Server command which could be executed by anyone.
        /// </summary>
        ServerEveryone = 1 << 2,

        ClientAndServerEveryone = Client | ServerEveryone,

        ClientAndServerOperatorOnly = Client | ServerOperator,
    }
}