namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ChatRoomHolder
        : ProtoGameObject<ILogicObject, ChatRoomHolder.ChatRoomPrivateState, EmptyPublicState, EmptyClientState>,
          IProtoLogicObject
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue; // never

        [NotLocalizable]
        public override string Name => "Chat room holder";

        public override double ServerUpdateIntervalSeconds => double.MaxValue; // never

        public static void ServerSetup(ILogicObject chatRoomHolder, BaseChatRoom chatRoom)
        {
            chatRoomHolder.GetPrivateState<ChatRoomPrivateState>()
                          .ChatRoom = chatRoom;
        }

        public override void ClientDeinitialize(ILogicObject gameObject)
        {
            ChatSystem.ClientOnChatRoomRemoved(gameObject);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            ChatSystem.ClientOnChatRoomAdded(data.GameObject);
        }

        public class ChatRoomPrivateState : BasePrivateState
        {
            [SyncToClient]
            public BaseChatRoom ChatRoom { get; set; }
        }
    }
}