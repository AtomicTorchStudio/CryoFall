namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
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
                ServersListSortType.OnlinePlayersCount,
                ServersListSortType.Ping,
                ServersListSortType.Title
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
            string title;
            switch (sortType)
            {
                case ServersListSortType.None:
                    title = string.Empty;
                    break;
                case ServersListSortType.Ping:
                    title = CoreStrings.ServerPing;
                    break;
                case ServersListSortType.Title:
                    title = CoreStrings.Title;
                    break;
                case ServersListSortType.OnlinePlayersCount:
                    title = CoreStrings.WindowSocial_TitleOnlinePlayersList;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortType));
            }

            this.Title = title;
            this.SortType = sortType;
        }

        public ServersListSortType SortType { get; }

        public string Title { get; }

        public static ViewModelServersListSortType GetViewModel(ServersListSortType sortType)
        {
            return DictionarySortTypeToViewModel[sortType];
        }
    }
}