namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ChatRoomPrivate : BaseChatRoom
    {
        public ChatRoomPrivate(string characterA, string characterB, string clanTagCharacterA, string clanTagCharacterB)
        {
            this.CharacterA = characterA;
            this.CharacterB = characterB;
            this.ClanTagCharacterA = clanTagCharacterA;
            this.ClanTagCharacterB = clanTagCharacterB;
        }

        [SyncToClient]
        public string CharacterA { get; private set; }

        [SyncToClient]
        public string CharacterB { get; private set; }

        [SyncToClient]
        public string ClanTagCharacterA { get; private set; }

        [SyncToClient]
        public string ClanTagCharacterB { get; private set; }

        [SyncToClient(isAllowClientSideModification: true)]
        public bool IsClosedByCharacterA { get; set; } = true;

        [SyncToClient(isAllowClientSideModification: true)]
        public bool IsClosedByCharacterB { get; set; } = true;

        [SyncToClient(isSendChanges: false)]
        public bool IsReadByCharacterA { get; set; } = true;

        [SyncToClient(isSendChanges: false)]
        public bool IsReadByCharacterB { get; set; } = true;

        public override string ClientGetTitle()
        {
            var isCurrentCharacterA = this.ClientIsCurrentCharacterA();
            var otherCharacterName = isCurrentCharacterA
                                         ? this.CharacterB
                                         : this.CharacterA;

            var otherCharacterClanTag = isCurrentCharacterA
                                            ? this.ClanTagCharacterB
                                            : this.ClanTagCharacterA;

            if (!string.IsNullOrEmpty(otherCharacterClanTag))
            {
                return string.Format(CoreStrings.ClanTag_FormatWithName,
                                     otherCharacterClanTag, 
                                     "@" + otherCharacterName);
            }

            return "@" + otherCharacterName;
        }

        public bool ClientIsClosedByCurrentCharacter()
        {
            var isCurrentCharacterA = this.ClientIsCurrentCharacterA();
            return isCurrentCharacterA
                       ? this.IsClosedByCharacterA
                       : this.IsClosedByCharacterB;
        }

        public bool ClientIsCurrentCharacterA()
        {
            var currentCharacterName = ClientCurrentCharacterHelper.Character.Name;
            return this.CharacterA == currentCharacterName;
        }

        public bool ClientIsUnreadByCurrentCharacter()
        {
            var isCurrentCharacterA = this.ClientIsCurrentCharacterA();
            return isCurrentCharacterA
                       ? !this.IsReadByCharacterA
                       : !this.IsReadByCharacterB;
        }

        public void ClientSetOpenedOrClosedForCurrentCharacter(bool isClosed)
        {
            if (this.ClientIsCurrentCharacterA())
            {
                this.IsClosedByCharacterA = isClosed;
            }
            else
            {
                this.IsClosedByCharacterB = isClosed;
            }
        }

        public override void ServerAddMessageToLog(in ChatEntry chatEntry)
        {
            if (!chatEntry.IsService)
            {
                this.IsClosedByCharacterA = false;
                this.IsClosedByCharacterB = false;

                if (chatEntry.From == this.CharacterA)
                {
                    this.IsReadByCharacterB = false;
                    this.ClanTagCharacterA = chatEntry.ClanTag;
                }
                else
                {
                    this.IsReadByCharacterA = false;
                    this.ClanTagCharacterB = chatEntry.ClanTag;
                }
            }

            base.ServerAddMessageToLog(in chatEntry);
        }

        public override IEnumerable<ICharacter> ServerEnumerateMessageRecepients(ICharacter forPlayer)
        {
            var characterA = Api.Server.Characters.GetPlayerCharacter(this.CharacterA);
            if (characterA is not null)
            {
                yield return characterA;
            }

            var characterB = Api.Server.Characters.GetPlayerCharacter(this.CharacterB);
            if (characterB is not null)
            {
                yield return characterB;
            }
        }

        public void ServerReplaceCharacterName(string newName, bool isCharacterA)
        {
            Api.ValidateIsServer();

            if (isCharacterA)
            {
                this.CharacterA = newName;
            }
            else
            {
                this.CharacterB = newName;
            }
        }

        public void ServerSetCloseByCharacter(ICharacter character)
        {
            if (this.CharacterA == character.Name)
            {
                this.IsClosedByCharacterA = true;
                return;
            }

            if (this.CharacterB == character.Name)
            {
                this.IsClosedByCharacterB = true;
                return;
            }

            throw new Exception(
                $"{character} is not available in chat room {this} with characters {this.CharacterA} and {this.CharacterB}");
        }

        public void ServerSetReadByCharacter(ICharacter character)
        {
            if (this.CharacterA == character.Name)
            {
                this.IsReadByCharacterA = true;
                return;
            }

            if (this.CharacterB == character.Name)
            {
                this.IsReadByCharacterB = true;
                return;
            }

            throw new Exception(
                $"{character} is not available in chat room {this} with characters {this.CharacterA} and {this.CharacterB}");
        }

        public void UpdateClanTag(string name, string clanTag)
        {
            if (this.CharacterA == name)
            {
                this.ClanTagCharacterA = clanTag;
            }
            else if (this.CharacterB == name)
            {
                this.ClanTagCharacterB = clanTag;
            }
            else
            {
                Api.Logger.Error(
                    $"This character is not a participant in private chat: {name} in private chat where only {this.CharacterA} and {this.CharacterB} are present");
            }
        }
    }
}