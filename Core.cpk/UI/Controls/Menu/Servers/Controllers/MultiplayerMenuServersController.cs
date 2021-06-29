namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class MultiplayerMenuServersController : BaseMultiplayerMenuServersController
    {
        public const string DialogRemoveServerFromList = "Are you sure you want to remove the server from this list?";

        public readonly ActionCommandWithParameter CommandEdit;

        public readonly ActionCommandWithParameter CommandRemove;

        private IServersListProvider serversListProvider;

        public MultiplayerMenuServersController(
            IServersListProvider serversListProvider,
            ServerViewModelsProvider serverViewModelsProvider)
            : base(serverViewModelsProvider)
        {
            this.CommandEdit = new ActionCommandWithParameter(
                server => this.ExecuteCommandEdit((ViewModelServerInfo)server));
            this.CommandRemove = new ActionCommandWithParameter(
                server => this.ExecuteCommandRemove((ViewModelServerInfo)server));
            this.serversListProvider = serversListProvider;
            this.serversListProvider.Updated += this.ListUpdatedHandler;
        }

        public override bool ContainsServerAddress(ServerAddress address)
        {
            return this.serversListProvider.Contains(address);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (this.serversListProvider is not null)
            {
                this.serversListProvider.Updated -= this.ListUpdatedHandler;
                this.serversListProvider = null;
            }
        }

        public void ListUpdatedHandler()
        {
            this.ReloadServersList();
        }

        public override void ReloadServersList()
        {
            this.RebuildServersList();
        }

        protected void ExecuteCommandEdit(ViewModelServerInfo viewModelServerInfo)
        {
            var oldServerAddress = viewModelServerInfo.Address;
            var editWindow = new WindowAddOrEditServer();
            editWindow.WindowTitle = CoreStrings.MenuServers_Button_EditAddress;
            editWindow.ActionTitle = CoreStrings.Button_Save;
            editWindow.TextAddress = oldServerAddress.HostAddress;

            editWindow.OkAction = newServerAddress =>
                                  {
                                      if (oldServerAddress.HostAddress.Equals(newServerAddress,
                                                                              StringComparison.Ordinal))
                                      {
                                          return;
                                      }

                                      Api.Logger.Important(
                                          $"Server renamed: from {oldServerAddress} to {newServerAddress}");
                                      this.serversListProvider.Remove(oldServerAddress);
                                      this.serversListProvider.Add(new ServerAddress(newServerAddress));
                                      this.serversListProvider.Save();
                                  };

            Api.Client.UI.LayoutRootChildren.Add(editWindow);
        }

        protected void ExecuteCommandRemove(ViewModelServerInfo viewModelServerInfo)
        {
            var address = viewModelServerInfo.Address;
            DialogWindow.ShowDialog(
                CoreStrings.MenuServers_Button_RemoveServer,
                DialogRemoveServerFromList,
                okAction: () =>
                          {
                              this.serversListProvider.Remove(address);
                              this.serversListProvider.Save();
                          },
                cancelAction: () => { },
                okText: CoreStrings.Yes,
                cancelText: CoreStrings.No,
                closeByEscapeKey: true,
                zIndexOffset: 100000);
        }

        protected override void PopulateServersList()
        {
            var servers = this.serversListProvider.CurrentList;
            foreach (var address in servers)
            {
                var item = this.ServerViewModelsProvider.GetServerInfoViewModel(address);
                if (this.serversListProvider.IsUserCanRemove)
                {
                    item.CommandRemove = this.CommandRemove;
                }

                if (this.serversListProvider.IsUserCanEdit)
                {
                    item.CommandEdit = this.CommandEdit;
                }

                this.ServerAddressToServerViewModel.Add((address, item));
            }

            this.IsListAvailable = true;
        }
    }
}