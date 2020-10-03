namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.OnlinePlayers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
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
                            || OnlinePlayersSystem.ClientIsOnline(name);

            OnlinePlayersSystem.ClientPlayerAddedOrRemoved += this.OnlinePlayersSystemPlayerAddedOrRemovedHandler;
        }

        public BaseCommand CommandCopyName => new ActionCommand(this.ExecuteCommandCopyName);

        public BaseCommand CommandOpenPrivateChat => new ActionCommand(this.ExecuteCommandOpenPrivateChat);

        public BaseCommand CommandRemove { get; }

        public bool IsOnline { get; private set; }

        public string Name { get; }

        public Visibility RemoveButtonVisibility { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            OnlinePlayersSystem.ClientPlayerAddedOrRemoved -= this.OnlinePlayersSystemPlayerAddedOrRemovedHandler;
        }

        private void ExecuteCommandCopyName()
        {
            Api.Client.Core.CopyToClipboard(this.Name);
        }

        private void ExecuteCommandOpenPrivateChat()
        {
            ChatSystem.ClientOpenPrivateChat(withCharacterName: this.Name);
        }

        private void OnlinePlayersSystemPlayerAddedOrRemovedHandler(OnlinePlayersSystem.Entry entry, bool isOnline)
        {
            if (entry.Name == this.Name)
            {
                this.IsOnline = isOnline;
            }
        }
    }
}