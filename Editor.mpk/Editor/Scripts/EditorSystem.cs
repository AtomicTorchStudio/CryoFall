namespace AtomicTorch.CBND.CoreMod.Editor.Scripts
{
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    [RemoteAuthorizeServerOperator]
    public class EditorSystem : ProtoEntity
    {
        private static EditorSystem instance;

        public override string Name => "Editor system";

        public static void ClientTeleport(Vector2D worldPosition)
        {
            Client.World.SetPosition(Client.Characters.CurrentPlayerCharacter, worldPosition, forceReset: true);
            instance.CallServer(_ => _.ServerRemote_ServerTeleport(worldPosition));
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();
            instance = this;
        }

        private void ServerRemote_ServerTeleport(Vector2D worldPosition)
        {
            Server.World.SetPosition(ServerRemoteContext.Character, worldPosition);
        }
    }
}