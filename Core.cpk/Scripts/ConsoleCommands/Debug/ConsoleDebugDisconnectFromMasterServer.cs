// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ConsoleDebugDisconnectFromMasterServer : BaseConsoleCommand
    {
        public override string Description => "Disconnect from Master Server.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.Client;

        public override string Name => "debug.disconnectFromMasterServer";

        public string Execute()
        {
            if (Client.MasterServer.MasterServerConnectionState == ConnectionState.Disconnected)
            {
                return "Already disconnected from Master Server";
            }

            Client.MasterServer.Disconnect();
            return "Disconnected from Master Server";
        }
    }
}