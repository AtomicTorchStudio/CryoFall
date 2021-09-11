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
            Client.LocalServer.StatusChanged += ClientLocalServerStatusChangedHandler;
        }

        private static void ClientLocalServerStatusChangedHandler()
        {
            Logger.Important("Local server status changed: " + Client.LocalServer.Status);
            if (Client.LocalServer.Status
                    is LocalServerStatus.Stopped
                    or LocalServerStatus.Crashed)
            {
                // stop connection attempts (to not stuck on the loading splash screen)
                Client.CurrentGame.Disconnect();
                MainMenuOverlay.IsHidden = false;
            }
        }

        private static void CurrentGameConnectionStateChangedHandler()
        {
            if (Client.CurrentGame.ConnectionState == ConnectionState.Disconnected)
            {
                Client.LocalServer.Stop();
            }
        }
    }
}