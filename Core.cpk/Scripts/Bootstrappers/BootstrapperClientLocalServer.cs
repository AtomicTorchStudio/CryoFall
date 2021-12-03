namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class BootstrapperClientLocalServer : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            Client.CurrentGame.ConnectionStateChanged += CurrentGameConnectionStateChangedHandler;
            Client.CurrentGame.ServerInfoChanged += CurrentGameConnectionStateChangedHandler;
            Client.LocalServer.StatusChanged += ClientLocalServerStatusChangedHandler;
        }

        private static void ClientLocalServerStatusChangedHandler()
        {
            Logger.Important("Local server status changed: " + Client.LocalServer.Status);

            if (Client.LocalServer.Status
                    is LocalServerStatus.Stopped
                    or LocalServerStatus.Crashed
                && (Client.CurrentGame.ServerInfo is null
                    || Client.CurrentGame.ServerInfo.ServerAddress.IsLocalServer))
            {
                // stop connection attempts (to not stuck on the loading splash screen)
                Client.CurrentGame.Disconnect();
                MainMenuOverlay.IsHidden = false;
            }
        }

        private static void CurrentGameConnectionStateChangedHandler()
        {
            if (Client.LocalServer.Status != LocalServerStatus.Stopped
                && Client.CurrentGame.ConnectionState == ConnectionState.Disconnected)
            {
                // disconnected or connected to another server
                if (Client.Core.IsCompiling)
                {
                    // currently recompiling the core/mods for the local server, do not stop it
                    return;
                }

                Logger.Important("Stopping local server as disconnected from it");
                Client.LocalServer.Stop();
            }
        }
    }
}