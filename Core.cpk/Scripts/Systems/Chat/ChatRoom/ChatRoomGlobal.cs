namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ChatRoomGlobal : BaseChatRoom
    {
        public const string Title = "Global";

        public override string ClientGetTitle()
        {
            return "% " + Title;
        }

        public override IEnumerable<ICharacter> ServerEnumerateMessageRecepients(ICharacter forPlayer)
        {
            return Api.Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
        }
    }
}