namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ApiState : BaseBootstrapper
    {
        public static bool IsShutdown { get; private set; } = true;

        public override void ClientInitialize()
        {
            Register();
        }

        public override void ServerInitialize(IServerConfiguration serverConfiguration)
        {
            Register();
        }

        private static void ApiShutdownHandler()
        {
            IsShutdown = true;
        }

        private static void Register()
        {
            IsShutdown = false;
            Api.OnShutdown += ApiShutdownHandler;
        }
    }
}