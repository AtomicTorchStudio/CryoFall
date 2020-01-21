namespace AtomicTorch.CBND.CoreMod.Helpers.Physics
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SharedEditorPhysicsDebugger : ProtoEntity
    {
        private static SharedEditorPhysicsDebugger instance;

        public override string Name => nameof(SharedEditorPhysicsDebugger);

        public static void ServerSendDebugPhysicsTesting(IPhysicsShape physicsShape)
        {
            Api.Assert(Api.IsEditor, "This is Editor-only server code");

            var wrappedShape = PhysicsShapeRemoteDataHelper.Wrap(physicsShape);
            var allPlayers = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true,
                                                                            exceptSpectators: false);
            instance.CallClient(
                allPlayers,
                _ => _.ClientRemote_ProcessServerDebugPhysicsTesting(wrappedShape));
        }

        public static void SharedVisualizeTestResults(
            ITempList<TestResult> physicsTestResults,
            CollisionGroup collisionGroup)
        {
            if (IsClient)
            {
                using var testResults = Api.Shared.GetTempList<Vector2D>();
                AddTestResults(physicsTestResults.AsList(), testResults.AsList());
                ClientComponentPhysicsSpaceVisualizer.VisualizeTestResults(testResults.AsList(),
                                                                           collisionGroup,
                                                                           isClient: true);
            }
            else // if server
            {
                Api.Assert(Api.IsEditor, "This is Editor-only server code");
                var allPlayers = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true,
                                                                                exceptSpectators: false);

                var testResults = new List<Vector2D>();
                AddTestResults(physicsTestResults.AsList(), testResults);

                var collisionGroupId = CollisionGroups.GetCollisionGroupId(collisionGroup);
                instance.CallClient(
                    allPlayers,
                    // ReSharper disable once AccessToDisposedClosure
                    _ => _.ClientRemote_ProcessServerTestResults(testResults, collisionGroupId));
            }
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();
            instance = this;
        }

        private static void AddTestResults(IList<TestResult> sourceList, IList<Vector2D> resultList)
        {
            foreach (var testResult in sourceList)
            {
                resultList.Add(testResult.PhysicsBody.Position
                               + testResult.Penetration);
            }
        }

        [RemoteCallSettings(DeliveryMode.Unreliable, maxCallsPerSecond: 120, avoidBuffer: true)]
        private void ClientRemote_ProcessServerDebugPhysicsTesting(BasePhysicsShapeRemoteData wrappedShape)
        {
            var shape = PhysicsShapeRemoteDataHelper.Unwrap(wrappedShape);
            ClientComponentPhysicsSpaceVisualizer.ProcessServerDebugPhysicsTesting(shape);
        }

        private void ClientRemote_ProcessServerTestResults(
            List<Vector2D> testResults,
            CollisionGroupId collisionGroupId)
        {
            var collisionGroup = CollisionGroups.GetCollisionGroup(collisionGroupId);
            ClientComponentPhysicsSpaceVisualizer.VisualizeTestResults(testResults,
                                                                       collisionGroup,
                                                                       isClient: false);
        }
    }
}