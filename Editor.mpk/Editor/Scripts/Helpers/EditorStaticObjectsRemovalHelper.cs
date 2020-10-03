namespace AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    [RemoteAuthorizeServerOperator]
    public class EditorStaticObjectsRemovalHelper : ProtoEntity
    {
        private static EditorStaticObjectsRemovalHelper instance;

        public override string Name => "Editor static objects removal helper";

        public static void ClientDelete(IReadOnlyCollection<IStaticWorldObject> worldObjectsToDelete)
        {
            instance.ClientDeleteInternal(worldObjectsToDelete);
        }

        public static void ClientDelete(BoundsUshort worldBoundsToDeleteObjects)
        {
            var staticObjects = Client.World.GetStaticObjectsAtBounds(worldBoundsToDeleteObjects);
            instance.ClientDeleteInternal(staticObjects);
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();
            instance = this;
        }

        private void ClientDeleteInternal(IReadOnlyCollection<IStaticWorldObject> worldObjectsToDelete)
        {
            worldObjectsToDelete = worldObjectsToDelete.Distinct().ToList();

            var restoreRequests = worldObjectsToDelete.Select(o => new RestoreObjectRequest(o))
                                                      .Batch(20000)
                                                      .Select(b => b.ToList())
                                                      .ToList();

            var tilePositions = worldObjectsToDelete
                                .GroupBy(_ => _.TilePosition)
                                .Select(g => g.Key)
                                .Batch(20000)
                                .Select(b => b.ToList())
                                .ToList();

            EditorClientSystem.DoAction(
                "Delete objects",
                onDo: () =>
                      {
                          foreach (var batch in tilePositions)
                          {
                              this.CallServer(_ => _.ServerRemote_DeleteObjects(batch));
                          }
                      },
                onUndo: () =>
                        {
                            foreach (var batch in restoreRequests)
                            {
                                this.CallServer(_ => _.ServerRemote_RestoreObjects(batch));
                            }
                        },
                canGroupWithPreviousAction: false);
        }

        [RemoteCallSettings(DeliveryMode.Default, clientMaxSendQueueSize: byte.MaxValue)]
        private void ServerRemote_DeleteObjects(List<Vector2Ushort> positionsToDeleteObjects)
        {
            var worldService = Server.World;

            var worldObjectsToDelete = new List<IStaticWorldObject>(
                capacity: positionsToDeleteObjects.Count * 2);

            foreach (var tilePosition in positionsToDeleteObjects)
            {
                worldObjectsToDelete.AddRange(worldService.GetStaticObjects(tilePosition));
            }

            foreach (var worldObject in worldObjectsToDelete)
            {
                worldService.DestroyObject(worldObject);
            }
        }

        /// <summary>
        /// Tradeoff: we cannot restore deleted objects, but we can spawn the same objects again.
        /// Of course their IDs and state will be new.
        /// </summary>
        [RemoteCallSettings(DeliveryMode.Default, clientMaxSendQueueSize: byte.MaxValue)]
        private void ServerRemote_RestoreObjects(IReadOnlyList<RestoreObjectRequest> request)
        {
            foreach (var restoreObjectRequest in request)
            {
                var prototype = restoreObjectRequest.Prototype;
                Server.World.CreateStaticWorldObject(
                    prototype,
                    restoreObjectRequest.TilePosition);
            }
        }

        internal struct RestoreObjectRequest : IRemoteCallParameter
        {
            public readonly IProtoStaticWorldObject Prototype;

            public readonly Vector2Ushort TilePosition;

            public RestoreObjectRequest(IStaticWorldObject staticWorldObject)
            {
                this.Prototype = staticWorldObject.ProtoStaticWorldObject;
                this.TilePosition = staticWorldObject.TilePosition;
            }
        }
    }
}