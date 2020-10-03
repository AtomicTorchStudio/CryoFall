namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System.ComponentModel;

    public enum ServersListSortType
    {
        None = 0,

        [Description(CoreStrings.ServerPing)]
        Ping = 1,

        [Description(CoreStrings.Title)]
        Title = 2,

        [Description(CoreStrings.WindowSocial_TitleOnlinePlayersList)]
        OnlinePlayersCount = 3,

        [Description(CoreStrings.ServerWipedDate)]
        LastWipe = 4,

        [Description(CoreStrings.MenuServers_ListTitleFeatured)]
        Featured = 5
    }
}