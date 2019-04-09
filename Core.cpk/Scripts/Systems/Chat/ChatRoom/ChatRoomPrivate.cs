namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ChatRoomPrivate : BaseChatRoom
    {
        public ChatRoomPrivate(string characterA, string characterB)
        {
            this.CharacterA = characterA;
            this.CharacterB = characterB;
        }

        [SyncToClient]
        public string CharacterA { get; }

        [SyncToClient]
        public string CharacterB { get; }

        public override string ClientGetTitle()
        {
            var currentCharacterName = ClientCurrentCharacterHelper.Character.Name;
            var otherCharacterName = this.CharacterA != currentCharacterName
                                         ? this.CharacterA
                                         : this.CharacterB;
            return "@" + otherCharacterName;
        }

        public override IEnumerable<ICharacter> ServerEnumerateMessageRecepients(ICharacter forPlayer)
        {
            var characterA = Api.Server.Characters.GetPlayerCharacter(this.CharacterA);
            if (characterA != null)
            {
                yield return characterA;
            }

            var characterB = Api.Server.Characters.GetPlayerCharacter(this.CharacterB);
            if (characterB != null)
            {
                yield return characterB;
            }
        }
    }
}