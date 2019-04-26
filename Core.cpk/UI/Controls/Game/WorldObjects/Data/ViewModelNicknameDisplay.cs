namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelNicknameDisplay : BaseViewModel
    {
        private static readonly Brush BrushNewbie
            = new SolidColorBrush(Color.FromArgb(0xFF, 0x66, 0xDD, 0xFF));

        private static readonly Brush BrushPartyMember
            = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xFA, 0x33));

        private static readonly Brush BrushStranger
            = new SolidColorBrush(Color.FromArgb(0xFF, 0xF1, 0xF1, 0xF1));

        private readonly ICharacter character;

        private readonly bool isOnline;

        public ViewModelNicknameDisplay(ICharacter character, bool isOnline)
        {
            this.character = character;
            this.isOnline = isOnline;

            PlayerCharacter.GetPublicState(this.character)
                           .ClientSubscribe(_ => _.IsNewbie,
                                            callback: _ => this.Refresh(),
                                            this);

            PartySystem.ClientCurrentPartyMemberAddedOrRemoved += this.ClientCurrentPartyMemberAddedOrRemovedHandler;

            this.Refresh();
        }

        public Brush Brush
        {
            get
            {
                if (this.IsPartyMember)
                {
                    return BrushPartyMember;
                }

                if (this.IsNewbieAndNotPartyMember)
                {
                    return BrushNewbie;
                }

                return BrushStranger;
            }
        }

        public bool IsNewbieAndNotPartyMember { get; private set; }

        public bool IsPartyMember { get; private set; }

        public string Name => this.character.Name;

        public string Text
        {
            get
            {
                var name = this.Name;
                if (string.IsNullOrEmpty(name))
                {
                    // should be impossible
                    return "Nickname";
                }

                var result = name;
                if (DevelopersListHelper.IsDeveloper(name))
                {
                    result = ViewModelChatEntryControl.ChatNamePrefix_Developer + "\n" + result;
                }

                if (!this.isOnline)
                {
                    result = "Offline Zzz...\n" + result;
                }

                return result;
            }
        }

        protected override void DisposeViewModel()
        {
            PartySystem.ClientCurrentPartyMemberAddedOrRemoved -= this.ClientCurrentPartyMemberAddedOrRemovedHandler;
        }

        private void ClientCurrentPartyMemberAddedOrRemovedHandler((string name, bool isAdded) obj)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            var members = PartySystem.ClientGetCurrentPartyMembers();
            this.IsPartyMember = members.Contains(this.character.Name);
            this.IsNewbieAndNotPartyMember = !this.IsPartyMember
                                             && PlayerCharacter.GetPublicState(this.character)
                                                               .IsNewbie;

            this.NotifyPropertyChanged(nameof(this.Brush));
        }
    }
}