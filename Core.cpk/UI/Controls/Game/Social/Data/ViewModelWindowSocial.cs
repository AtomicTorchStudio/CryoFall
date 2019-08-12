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
        public ViewModelWindowSocial()
        {
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
                       .ExceptOne(currentCharacterName)
                       .Select(name => new ViewModelPlayerEntry(name))
                       .ToList();
            list.Sort();
            this.PlayersOnline = new SuperObservableCollection<ViewModelPlayerEntry>(list);

            OnlinePlayersSystem.ClientOnPlayerAddedOrRemoved += this.OnPlayerAddedOrRemovedHandler;
            OnlinePlayersSystem.ClientTotalServerPlayersCountChanged += this.TotalServerPlayersCountChangedHandler;
            ClientChatBlockList.CharacterBlockStatusChanged += this.CharacterBlockStatusChangedHandler;
        }

        public SuperObservableCollection<ViewModelPlayerEntry> PlayersOnline { get; }

        // add current player to the total online players count
        public int PlayersOnlineCount => this.PlayersOnline.Count + 1;

        public int PlayersTotalCount => OnlinePlayersSystem.ClientTotalServerPlayersCount;

        public Visibility PlayersTotalCountVisibility => this.PlayersTotalCount > 0
                                                             ? Visibility.Visible
                                                             : Visibility.Collapsed;

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            OnlinePlayersSystem.ClientOnPlayerAddedOrRemoved -= this.OnPlayerAddedOrRemovedHandler;
            OnlinePlayersSystem.ClientTotalServerPlayersCountChanged -= this.TotalServerPlayersCountChangedHandler;
            ClientChatBlockList.CharacterBlockStatusChanged -= this.CharacterBlockStatusChangedHandler;
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

        private void OnPlayerAddedOrRemovedHandler(string name, bool isOnline)
        {
            if (Client.Characters.CurrentPlayerCharacter?.Name == name)
            {
                return;
            }

            if (isOnline)
            {
                this.PlayersOnline.Add(new ViewModelPlayerEntry(name));
                this.PlayersOnline.Sort();
                this.NotifyPropertyChanged(nameof(this.PlayersOnlineCount));
                return;
            }

            // player went offline - find and remove it from the list
            for (var index = 0; index < this.PlayersOnline.Count; index++)
            {
                var vm = this.PlayersOnline[index];
                if (vm.Name != name)
                {
                    continue;
                }

                this.PlayersOnline.RemoveAt(index);
                vm.Dispose();
                this.PlayersOnline.Sort();
                this.NotifyPropertyChanged(nameof(this.PlayersOnlineCount));
                return;
            }
        }

        private void TotalServerPlayersCountChangedHandler(int obj)
        {
            this.NotifyPropertyChanged(nameof(this.PlayersTotalCount));
            this.NotifyPropertyChanged(nameof(this.PlayersTotalCountVisibility));
        }
    }
}