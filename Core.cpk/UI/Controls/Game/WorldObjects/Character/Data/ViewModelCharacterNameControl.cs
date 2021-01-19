namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data
{
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelCharacterNameControl : BaseViewModel
    {
        public static readonly Brush BrushAllianceMember
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorGreen6");

        public static readonly Brush BrushCurrentFactionMember
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorGreen7");

        public static readonly Brush BrushEnemy
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed7");

        public static readonly Brush BrushNewbie
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorAlt7");

        public static readonly Brush BrushStranger
            = new SolidColorBrush(Color.FromArgb(0xFF, 0xF1, 0xF1, 0xF1));

        private readonly ICharacter character;

        private readonly PlayerCharacterPublicState publicState;

        public ViewModelCharacterNameControl(ICharacter character)
        {
            this.character = character;

            this.publicState = PlayerCharacter.GetPublicState(this.character);
            this.publicState.ClientSubscribe(_ => _.IsOnline,
                                             callback: _ => this.Refresh(),
                                             this);

            this.publicState.ClientSubscribe(_ => _.IsNewbie,
                                             callback: _ => this.Refresh(),
                                             this);

            this.publicState.ClientSubscribe(_ => _.IsPveDuelModeEnabled,
                                             callback: _ => this.Refresh(),
                                             this);

            this.publicState.ClientSubscribe(_ => _.ClanTag,
                                             callback: _ => this.Refresh(),
                                             this);

            PartySystem.ClientCurrentPartyMemberAddedOrRemoved += this.PartyMemberAddedOrRemovedHandler;
            FactionSystem.ClientCurrentFactionMemberAddedOrRemoved += this.FactionMemberAddedOrRemovedHandler;
            FactionSystem.ClientFactionDiplomacyStatusChanged += this.FactionDiplomacyStatusChangedHandler;
            this.Refresh();
        }

        public Brush Brush
        {
            get
            {
                if (this.character.IsCurrentClientCharacter)
                {
                    return BrushStranger;
                }

                if (this.IsCurrentFactionMember)
                {
                    return BrushCurrentFactionMember;
                }

                if (this.IsAllianceFactionMember)
                {
                    return BrushAllianceMember;
                }

                if (this.IsEnemyFactionMember)
                {
                    return BrushEnemy;
                }

                if (this.IsNewbieAndNotPartyMember)
                {
                    return BrushNewbie;
                }

                return BrushStranger;
            }
        }

        public string ClanTag { get; private set; }

        public bool IsAllianceFactionMember { get; private set; }

        public bool IsCurrentFactionMember { get; private set; }

        public bool IsDuelModeAndNotPartyMember { get; private set; }

        public bool IsEnemyFactionMember { get; private set; }

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
                if (!string.IsNullOrEmpty(this.publicState.ClanTag))
                {
                    result = string.Format(CoreStrings.ClanTag_FormatWithName, this.publicState.ClanTag, result);
                }

                if (DevelopersListHelper.IsDeveloper(name))
                {
                    result = ViewModelChatEntryControl.ChatNamePrefix_Developer + "\n" + result;
                }

                if (!this.publicState.IsOnline)
                {
                    result = CoreStrings.NicknameOfflinePlayer + "\n" + result;
                }

                return result;
            }
        }

        protected override void DisposeViewModel()
        {
            PartySystem.ClientCurrentPartyMemberAddedOrRemoved -= this.PartyMemberAddedOrRemovedHandler;
            FactionSystem.ClientCurrentFactionMemberAddedOrRemoved -= this.FactionMemberAddedOrRemovedHandler;
            FactionSystem.ClientFactionDiplomacyStatusChanged -= this.FactionDiplomacyStatusChangedHandler;
        }

        private void FactionDiplomacyStatusChangedHandler((string clanTag, FactionDiplomacyStatus status) obj)
        {
            this.Refresh();
        }

        private void FactionMemberAddedOrRemovedHandler((FactionMemberEntry entry, bool isAdded) obj)
        {
            this.Refresh();
        }

        private void PartyMemberAddedOrRemovedHandler((string name, bool isAdded) obj)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            if (this.character.IsCurrentClientCharacter)
            {
                return;
            }

            var members = PartySystem.ClientGetCurrentPartyMembers();
            var characterName = this.character.Name;

            this.IsPartyMember = members.Contains(characterName);
            this.IsNewbieAndNotPartyMember = !this.IsPartyMember
                                             && this.publicState.IsNewbie;

            this.IsDuelModeAndNotPartyMember = !this.IsPartyMember
                                               && this.publicState.IsPveDuelModeEnabled;

            var clanTag = !string.IsNullOrEmpty(this.publicState.ClanTag)
                              ? this.publicState.ClanTag
                              : null;
            this.ClanTag = clanTag;

            this.IsCurrentFactionMember = clanTag is not null
                                          && FactionSystem.ClientCurrentFactionClanTag == clanTag;

            if (this.IsCurrentFactionMember
                || clanTag is null)
            {
                // could be a member of the current faction, or a non-faction player
                this.IsAllianceFactionMember = false;
                this.IsEnemyFactionMember = false;
            }
            else
            {
                var status = FactionSystem.ClientGetCurrentFactionDiplomacyStatus(otherFactionClanTag: clanTag);
                this.IsAllianceFactionMember = status == FactionDiplomacyStatus.Ally;
                this.IsEnemyFactionMember
                    = status switch
                    {
                        FactionDiplomacyStatus.EnemyMutual                   => true,
                        FactionDiplomacyStatus.EnemyDeclaredByCurrentFaction => true,
                        FactionDiplomacyStatus.EnemyDeclaredByOtherFaction   => true,
                        _                                                    => false
                    };
            }

            this.NotifyPropertyChanged(nameof(this.Brush));
            this.NotifyPropertyChanged(nameof(this.Text));
        }
    }
}