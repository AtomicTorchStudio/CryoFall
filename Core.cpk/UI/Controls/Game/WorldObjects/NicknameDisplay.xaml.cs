namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class NicknameDisplay : BaseUserControl
    {
        private ViewModelNicknameDisplay viewModel;

        public void Setup(string name, bool isOnline)
        {
            this.viewModel = new ViewModelNicknameDisplay(name, isOnline);
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel;
            PartySystem.ClientCurrentPartyMemberAddedOrRemoved += this.ClientCurrentPartyMemberAddedOrRemovedHandler;

            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;

            this.viewModel?.Dispose();
            this.viewModel = null;

            PartySystem.ClientCurrentPartyMemberAddedOrRemoved -= this.ClientCurrentPartyMemberAddedOrRemovedHandler;
        }

        private void ClientCurrentPartyMemberAddedOrRemovedHandler((string name, bool isAdded) obj)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            var members = PartySystem.ClientGetCurrentPartyMembers();
            var isPartyMember = members.Contains(this.viewModel.Name);
            this.viewModel.IsPartyMember = isPartyMember;
        }
    }
}