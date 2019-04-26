namespace AtomicTorch.CBND.CoreMod
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public static class ServerRates
    {
        private static readonly ICoreServerService ServerCore = Api.IsServer
                                                                    ? Api.Server.Core
                                                                    : null;

        public static double Get(
            string key,
            double defaultValue,
            string description)
        {
            if (Api.IsClient)
            {
                return defaultValue;
            }

            return ServerCore.GetValueFromConfig(
                key,
                defaultValue,
                description);
        }

        public static int Get(
            string key,
            int defaultValue,
            string description)
        {
            if (Api.IsClient)
            {
                return defaultValue;
            }

            return ServerCore.GetValueFromConfig(
                key,
                defaultValue,
                description);
        }
    }
}