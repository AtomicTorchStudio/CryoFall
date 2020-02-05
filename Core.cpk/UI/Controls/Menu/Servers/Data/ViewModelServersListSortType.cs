namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelServersListSortType : BaseViewModel
    {
        public static readonly ReadOnlyCollection<ServersListSortType> AllSortTypes =
            new List<ServersListSortType>
            {
                ServersListSortType.None,
                ServersListSortType.OnlinePlayersCount,
                ServersListSortType.Ping,
                ServersListSortType.Title,
                ServersListSortType.LastWipe,
                ServersListSortType.Featured
            }.AsReadOnly();

        public static readonly Dictionary<ServersListSortType, ViewModelServersListSortType>
            DictionarySortTypeToViewModel;

        static ViewModelServersListSortType()
        {
            DictionarySortTypeToViewModel = EnumExtensions.GetValues<ServersListSortType>()
                                                          .ToDictionary(
                                                              p => p,
                                                              p => new ViewModelServersListSortType(p));
        }

        public ViewModelServersListSortType(ServersListSortType sortType)
        {
            this.SortType = sortType;
        }

        public ServersListSortType SortType { get; }

        public string Title => this.SortType.GetDescription();

        public static ViewModelServersListSortType GetViewModel(ServersListSortType sortType)
        {
            return DictionarySortTypeToViewModel[sortType];
        }
    }
}