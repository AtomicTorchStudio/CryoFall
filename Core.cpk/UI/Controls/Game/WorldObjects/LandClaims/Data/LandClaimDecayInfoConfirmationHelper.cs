namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.LandClaims.Data
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class LandClaimDecayInfoConfirmationHelper
    {
        private static readonly IClientStorage Storage;

        static LandClaimDecayInfoConfirmationHelper()
        {
            Storage = Api.Client.Storage.GetStorage("Servers/ConfirmedDecayInfo");
            Storage.RegisterType(typeof(ServerAddress));
            Storage.RegisterType(typeof(AtomicGuid));
        }

        public static bool IsConfirmedForCurrentServer
        {
            get
            {
                if (Api.IsEditor)
                {
                    return true;
                }

                if (!Storage.TryLoad(
                        out Dictionary<ServerAddress, bool> dictionary))
                {
                    return false;
                }

                var serverAddress = Api.Client.CurrentGame.ServerInfo.ServerAddress;
                return dictionary.TryGetValue(serverAddress, out var isConfirmed)
                       && isConfirmed;
            }
        }

        public static void SetConfirmedForCurrentServer()
        {
            if (!Storage.TryLoad(
                    out Dictionary<ServerAddress, bool> dictionary))
            {
                dictionary = new Dictionary<ServerAddress, bool>();
            }

            var serverAddress = Api.Client.CurrentGame.ServerInfo.ServerAddress;
            dictionary[serverAddress] = true;
            Storage.Save(dictionary);
        }
    }
}