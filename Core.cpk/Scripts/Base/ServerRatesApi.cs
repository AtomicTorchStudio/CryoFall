namespace AtomicTorch.CBND.CoreMod
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    /// <summary>
    /// Please don't use it directly and instead implement a rate class
    /// (see Scripts/Rates folder for examples).
    /// </summary>
    public static class ServerRatesApi
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

            return ServerCore.ServerRatesConfig.Get(key, defaultValue, description);
        }

        public static string Get(
            string key,
            string defaultValue,
            string description)
        {
            if (Api.IsClient)
            {
                return defaultValue;
            }

            return ServerCore.ServerRatesConfig.Get(key, defaultValue, description);
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

            return ServerCore.ServerRatesConfig.Get(key, defaultValue, description);
        }

        public static void Reset(
            string key,
            string defaultValue,
            string description)
        {
            ServerCore.ServerRatesConfig.Reset(key, defaultValue, description);
        }

        public static void Reset(
            string key,
            int defaultValue,
            string description)
        {
            ServerCore.ServerRatesConfig.Reset(key, defaultValue, description);
        }

        public static void Reset(
            string key,
            double defaultValue,
            string description)
        {
            ServerCore.ServerRatesConfig.Reset(key, defaultValue, description);
        }
    }
}