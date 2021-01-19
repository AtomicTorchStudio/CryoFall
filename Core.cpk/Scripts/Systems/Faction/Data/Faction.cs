namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class Faction
        : ProtoGameObject<ILogicObject, FactionPrivateState, FactionPublicState, EmptyClientState>,
          IProtoLogicObject
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue; // never

        [NotLocalizable]
        public override string Name => "Faction";

        public override double ServerUpdateIntervalSeconds => double.MaxValue; // never

        protected override void ServerInitialize(ServerInitializeData data)
        {
            data.PrivateState.Init();

            if (data.IsFirstTimeInit)
            {
                data.PrivateState.ServerFactionChatHolder = ChatSystem.ServerCreateChatRoom(
                    new ChatRoomFaction(faction: data.GameObject));
            }

            FactionSystem.ServerRegisterFaction(data.GameObject);
        }
    }
}