namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ChatRoomLocal : BaseChatRoom
    {
        public const string Title = "Local";

        public override string ClientGetTitle()
        {
            return "! " + Title;
        }

        public override void ClientOnMessageReceived(in ChatEntry chatEntry)
        {
            if (!(Api.Client.Characters.FindCharacter(chatEntry.From)
                      is ICharacter characterInstance))
            {
                // the character not in the scope
                return;
            }

            // have this character in scope - add the chat message to the log
            base.ClientOnMessageReceived(in chatEntry);

            // and display it over the speaking character
            CharacterLocalChatMessageDisplay.ShowOn(characterInstance, chatEntry.ClientGetFilteredMessage());
        }

        public override void ServerAddMessageToLog(in ChatEntry chatEntry)
        {
            // do nothing as this is an universal local chat room for all players on the server
        }

        public override IEnumerable<ICharacter> ServerEnumerateMessageRecepients(ICharacter forPlayer)
        {
            using var tempList = Api.Shared.GetTempList<ICharacter>();
            Api.Server.World.GetScopedByPlayers(forPlayer, tempList);
            foreach (var character in tempList.AsList())
            {
                yield return character;
            }
        }
    }
}