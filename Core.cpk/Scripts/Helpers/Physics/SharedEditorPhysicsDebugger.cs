namespace AtomicTorch.CBND.CoreMod.Helpers.Physics
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Physics;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class SharedEditorPhysicsDebugger : ProtoEntity
    {
        private static SharedEditorPhysicsDebugger instance;

        public override string Name => nameof(SharedEditorPhysicsDebugger);

        public static void ServerSendDebugPhysicsTesting(IPhysicsShape physicsShape)
        {
            var wrappedShape = PhysicsShapeRemoteDataHelper.Wrap(physicsShape);
            var allPlayers = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true,
                                                                            exceptSpectators: false);
            instance.CallClient(
                allPlayers,
                _ => _.ClientRemote_ProcessServerDebugPhysicsTesting(wrappedShape));
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();
            instance = this;
        }

        [RemoteCallSettings(DeliveryMode.Unreliable, maxCallsPerSecond: 120, avoidBuffer: true)]
        private void ClientRemote_ProcessServerDebugPhysicsTesting(BasePhysicsShapeRemoteData wrappedShape)
        {
            var shape = PhysicsShapeRemoteDataHelper.Unwrap(wrappedShape);
            ClientComponentPhysicsSpaceVisualizer.ProcessServerDebugPhysicsTesting(shape);
        }
    }
}