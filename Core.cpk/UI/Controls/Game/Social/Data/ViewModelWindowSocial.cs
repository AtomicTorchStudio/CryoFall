namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social.Data
{
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.OnlinePlayers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowSocial : BaseViewModel
    {
        private bool isActive;

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                if (this.isActive)
                {
                    OnlinePlayersSystem.ClientPlayerAddedOrRemoved -= this.PlayerAddedOrRemovedHandler;
                    OnlinePlayersSystem.ClientOnlinePlayerClanTagChanged -= this.OnlinePlayerClanTagChangedHandler;
                    OnlinePlayersSystem.ClientTotalServerPlayersCountChanged -= this.TotalPlayersCountChangedHandler;
                    ClientChatBlockList.CharacterBlockStatusChanged -= this.CharacterBlockStatusChangedHandler;
                }

                this.isActive = value;
                this.NotifyThisPropertyChanged();

                if (!this.isActive)
                {
                    var oldList = this.PlayersOnline;
                    this.PlayersOnline = null;
                    this.DisposeCollection(oldList);
                    return;
                }

                OnlinePlayersSystem.ClientPlayerAddedOrRemoved += this.PlayerAddedOrRemovedHandler;
                OnlinePlayersSystem.ClientOnlinePlayerClanTagChanged += this.OnlinePlayerClanTagChangedHandler;
                OnlinePlayersSystem.ClientTotalServerPlayersCountChanged += this.TotalPlayersCountChangedHandler;
                ClientChatBlockList.CharacterBlockStatusChanged += this.CharacterBlockStatusChangedHandler;

                var currentCharacterName = Client.Characters.CurrentPlayerCharacter?.Name;
                var onlinePlayers = OnlinePlayersSystem.ClientEnumerateOnlinePlayers();

                //// uncomment to test long fake users list
                //{
                //    onlinePlayers = new List<string>()
                //        { "Test1", "Test2", "Test33333333333333333333", "Test444444", "Test5" };
                //    for (var i = 0; i < 4; i++)
                //    {
                //        onlinePlayers = onlinePlayers.Concat(onlinePlayers.ToList());
                //    }
                //}

                var list = onlinePlayers
                           .ExceptOne(new OnlinePlayersSystem.Entry(currentCharacterName, null))
                           .ToList();
                list.Sort(OnlinePlayersSystem.Entry.CompareWithTag);

                this.PlayersOnline = new SuperObservableCollection<ViewModelPlayerEntry>(
                    list.Select(entry => new ViewModelPlayerEntry(entry.Name, entry.ClanTag))
                        .ToList());

                this.NotifyPropertyChanged(nameof(this.PlayersOnlineCount));
                this.NotifyPropertyChanged(nameof(this.PlayersTotalCountVisibility));
                this.NotifyPropertyChanged(nameof(this.PlayersTotalCount));
            }
        }

        public SuperObservableCollection<ViewModelPlayerEntry> PlayersOnline { get; private set; }
            = new SuperObservableCollection<ViewModelPlayerEntry>();

        // add current player to the total online players count
        public int PlayersOnlineCount => this.PlayersOnline.Count + 1;

        public int PlayersTotalCount => OnlinePlayersSystem.ClientTotalServerPlayersCount;

        public Visibility PlayersTotalCountVisibility => this.PlayersTotalCount > 0
                                                             ? Visibility.Visible
                                                             : Visibility.Collapsed;

        protected override void DisposeViewModel()
        {
            this.IsActive = false;
            base.DisposeViewModel();
        }

        private void CharacterBlockStatusChangedHandler((string name, bool isBlocked) obj)
        {
            var name = obj.name;
            foreach (var vm in this.PlayersOnline)
            {
                if (vm.Name != name)
                {
                    continue;
                }

                vm.RefreshBlockedStatus();
                return;
            }
        }

        private void OnlinePlayerClanTagChangedHandler(OnlinePlayersSystem.Entry entry)
        {
            this.PlayerAddedOrRemovedHandler(entry, isOnline: false);
            this.PlayerAddedOrRemovedHandler(entry, isOnline: true);
        }

        private void PlayerAddedOrRemovedHandler(OnlinePlayersSystem.Entry entry, bool isOnline)
        {
            if (Client.Characters.CurrentPlayerCharacter?.Name == entry.Name)
            {
                return;
            }

            try
            {
                var list = this.PlayersOnline;
                if (isOnline)
                {
                    // player went online - try to insert the entry (ordered by player name)
                    for (var index = 0; index < list.Count; index++)
                    {
                        var vm = list[index];
                        if (OnlinePlayersSystem.Entry.CompareWithTag(
                                new OnlinePlayersSystem.Entry(vm.Name, vm.ClanTag),
                                entry)
                            <= 0)
                        {
                            continue;
                        }

                        // found a location to insert this entry
                        list.Insert(index, new ViewModelPlayerEntry(entry.Name, entry.ClanTag));
                        return;
                    }

                    // add a new entry to the end
                    list.Add(new ViewModelPlayerEntry(entry.Name, entry.ClanTag));
                }
                else
                {
                    // player went offline - find and remove it from the list
                    for (var index = 0; index < list.Count; index++)
                    {
                        var vm = list[index];
                        if (vm.Name != entry.Name)
                        {
                            continue;
                        }

                        // entry found - remove it
                        list.RemoveAt(index);
                        vm.Dispose();
                        return;
                    }
                }
            }
            finally

            {
                this.NotifyPropertyChanged(nameof(this.PlayersOnlineCount));
            }
        }

        private void TotalPlayersCountChangedHandler(int obj)
        {
            this.NotifyPropertyChanged(nameof(this.PlayersTotalCount));
            this.NotifyPropertyChanged(nameof(this.PlayersTotalCountVisibility));
        }
    }
}