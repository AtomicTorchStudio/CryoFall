namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.OnlinePlayers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelPartyMember : BaseViewModel
    {
        public ViewModelPartyMember(
            string name,
            BaseCommand commandRemove,
            Visibility removeButtonVisibility)
        {
            this.Name = name;
            this.CommandRemove = commandRemove;
            this.RemoveButtonVisibility = removeButtonVisibility;

            this.IsOnline = name == ClientCurrentCharacterHelper.Character.Name
                            || OnlinePlayersSystem.ClientEnumerateOnlinePlayers()
                                                  .Contains(name);

            OnlinePlayersSystem.ClientOnPlayerAddedOrRemoved += this.OnlinePlayersSystemPlayerAddedOrRemovedHandler;
        }

        public BaseCommand CommandRemove { get; }

        public bool IsOnline { get; private set; }

        public string Name { get; }

        public Visibility RemoveButtonVisibility { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            OnlinePlayersSystem.ClientOnPlayerAddedOrRemoved -= this.OnlinePlayersSystemPlayerAddedOrRemovedHandler;
        }

        private void OnlinePlayersSystemPlayerAddedOrRemovedHandler(string name, bool isOnline)
        {
            if (name == this.Name)
            {
                this.IsOnline = isOnline;
            }
        }
    }
}