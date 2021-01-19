namespace AtomicTorch.CBND.CoreMod.Editor.Scripts
{
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    [RemoteAuthorizeServerOperator]
    public class EditorSystem : ProtoSystem<EditorSystem>
    {
        public override string Name => "Editor system";

        public static void ClientTeleport(Vector2D worldPosition)
        {
            Client.World.SetPosition(Client.Characters.CurrentPlayerCharacter, worldPosition, forceReset: true);
            Instance.CallServer(_ => _.ServerRemote_Teleport(worldPosition));
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, avoidBuffer: true)]
        private void ServerRemote_Teleport(Vector2D worldPosition)
        {
            Server.World.SetPosition(ServerRemoteContext.Character, worldPosition);
        }
    }
}