namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class Party
        : ProtoGameObject<ILogicObject, Party.PartyPrivateState, Party.PartyPublicState, EmptyClientState>,
          IProtoLogicObject
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue; // never

        [NotLocalizable]
        public override string Name => "Party";

        public override double ServerUpdateIntervalSeconds => double.MaxValue; // never

        protected override void ServerInitialize(ServerInitializeData data)
        {
            if (data.IsFirstTimeInit)
            {
                data.PrivateState.Members = new NetworkSyncList<string>();
                data.PrivateState.ServerPartyChatHolder = ChatSystem.ServerCreateChatRoom(
                    new ChatRoomParty(party: data.GameObject));
            }

            PartySystem.ServerRegisterParty(data.GameObject);
        }

        public class PartyPrivateState : BasePrivateState
        {
            [SyncToClient]
            public NetworkSyncList<string> Members { get; set; }

            // please note - this instance is not available via party private state
            public ILogicObject ServerPartyChatHolder { get; set; }
        }

        public class PartyPublicState : BasePublicState
        {
        }
    }
}