namespace AtomicTorch.CBND.CoreMod.Editor.Scripts
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    [RemoteAuthorizeServerOperator]
    public class WorldObjectsEditingSystem : ProtoSystem<WorldObjectsEditingSystem>
    {
        [RemoteCallSettings(DeliveryMode.Default,
                            timeInterval: 0,
                            clientMaxSendQueueSize: byte.MaxValue)]
        public void ServerRemote_DeleteObjects(List<uint> objectIdsToDelete)
        {
            var worldService = Server.World;
            foreach (var objectId in objectIdsToDelete)
            {
                worldService.DestroyObject(worldService.GetStaticObject(objectId));
            }
        }

        /// <summary>
        /// Tradeoff: we cannot restore deleted objects, but we can spawn the same objects again.
        /// Of course their IDs and state will be new.
        /// </summary>
        /// <returns>An array of spawned object IDs.</returns>
        [RemoteCallSettings(DeliveryMode.Default,
                            timeInterval: 0,
                            clientMaxSendQueueSize: byte.MaxValue)]
        public uint[] ServerRemote_SpawnObjects(IReadOnlyList<SpawnObjectRequest> list)
        {
            var result = new uint[list.Count];
            for (var index = 0; index < list.Count; index++)
            {
                var request = list[index];
                var prototype = request.Prototype;
                var obj = Server.World.CreateStaticWorldObject(
                    prototype,
                    request.TilePosition);
                result[index] = obj.Id;
            }

            return result;
        }
    }
}