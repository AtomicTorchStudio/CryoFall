namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public class ChatRoomFaction : BaseChatRoom
    {
        private static readonly ICharactersServerService CharactersServerService
            = Api.IsServer
                  ? Api.Server.Characters
                  : null;

        public ChatRoomFaction(ILogicObject faction)
        {
            this.Faction = faction;
        }

        // no need to replicate this property to client
        // as client could have only a single faction
        public ILogicObject Faction { get; }

        public override string ClientGetTitle()
        {
            return CoreStrings.Faction_Title;
        }

        public override IEnumerable<ICharacter> ServerEnumerateMessageRecepients(ICharacter forPlayer)
        {
            var members = FactionSystem.ServerGetFactionMemberNames(this.Faction);
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