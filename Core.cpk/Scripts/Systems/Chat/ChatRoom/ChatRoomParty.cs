namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public class ChatRoomParty : BaseChatRoom
    {
        private static readonly ICharactersServerService CharactersServerService
            = Api.IsServer
                  ? Api.Server.Characters
                  : null;

        public ChatRoomParty(ILogicObject party)
        {
            this.Party = party;
        }

        // no need to replicate this property to client
        // as client could have only a single party
        public ILogicObject Party { get; }

        public override string ClientGetTitle()
        {
            return "# " + CoreStrings.PartyManagement_Title;
        }

        public override IEnumerable<ICharacter> ServerEnumerateMessageRecepients(ICharacter forPlayer)
        {
            var members = PartySystem.ServerGetPartyMembersReadOnly(this.Party);
            foreach (var member in members)
            {
                var character = CharactersServerService.GetPlayerCharacter(member);
                if (character is not null)
                {
                    yield return character;
                }
            }
        }
    }
}