namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;

    public class MultiplayerMenuServersHistoryController : MultiplayerMenuServersController
    {
        public MultiplayerMenuServersHistoryController(
            ServerViewModelsProvider serverViewModelsProvider)
            : base(Api.Client.MasterServer.ServersProvider.History, serverViewModelsProvider)
        {
        }

        protected override void ApplyListFilters(List<ServerAddress> servers)
        {
            var localServer = Api.Client.LocalServer;

            for (var index = 0; index < servers.Count; index++)
            {
                var address = servers[index];
                if (address.IsLocalServer
                    && !localServer.IsSavegameExist(address.LocalServerSlotId))
                {
                    //Api.Logger.Info("Savegame missing for recent servers list: " + address);
                    servers.RemoveAt(index--);
                }
            }
        }
    }
}