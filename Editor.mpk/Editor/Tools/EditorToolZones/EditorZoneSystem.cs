namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Structures;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [RemoteAuthorizeServerOperator]
    public class EditorZoneSystem : ProtoEntity
    {
        public static EditorZoneSystem Instance { get; private set; }

        public override string Name => "Server zone helper";

        public void ClientClearSelectedZone(IProtoZone zone)
        {
            Logger.Important("Cleaning zone completely: " + zone.ShortId);
            var zoneProvider = ClientZoneProvider.Get(zone);
            var root = zoneProvider.GetQuadTree();
            if (root.IsEmpty)
            {
                // no need to clean
                return;
            }

            zoneProvider.ClearZone();
            zoneProvider.ApplyClientChanges(forcePushChangesImmediately: true);
        }

        public void ClientDeleteSelectedZoneObjects(IProtoZone zone)
        {
            var clientZoneProvider = ClientZoneProvider.Get(zone);
            var worldService = Api.Client.World;
            var zoneQuadTree = clientZoneProvider.GetQuadTree();
            var zoneBounds = zoneQuadTree.Bounds;

            var worldBoundsToDeleteObjects =
                worldService.GetStaticObjectsAtBounds(zoneBounds)
                            .Where(o => zoneQuadTree.IsPositionFilled(o.TilePosition))
                            .ToList();

            EditorStaticObjectsRemovalHelper.ClientDelete(worldBoundsToDeleteObjects);

            this.CallServer(_ => _.ServerRemote_DeleteZoneMobs(zone));
        }

        public void ClientInvokeZoneScripts(IProtoZone protoZone, bool isInitialSpawn)
        {
            this.CallServer(_ => _.ServerRemote_InvokeZoneScripts(protoZone, isInitialSpawn));
        }

        public Task<QuadTreeSnapshot> ClientRequestZoneData(IProtoZone protoZone)
        {
            Logger.Important($"Zone quadtree requested: {protoZone}");
            return this.CallServer(_ => _.ServerRemote_GetServerZoneData(protoZone));
        }

        public void ClientSendZoneModifications(IProtoZone protoZone, QuadTreeDiff diff)
        {
            Logger.Important($"Zone quadtree diff sent: {diff} for {protoZone}");
            this.CallServer(_ => _.ServerRemote_ApplyZoneModififications(protoZone, diff));
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();
            Instance = this;
        }

        [RemoteCallSettings(DeliveryMode.Default,
                            timeInterval: 0,
                            clientMaxSendQueueSize: byte.MaxValue)]
        private void ServerRemote_ApplyZoneModififications(IProtoZone protoZone, QuadTreeDiff diff)
        {
            var character = ServerRemoteContext.Character;
            var zoneInstance = protoZone.ServerZoneInstance;
            zoneInstance.ApplyDiff(diff);
            Logger.Important($"Zone quadtree diff applied: {diff} for {protoZone} by {character}");
        }

        [RemoteCallSettings(DeliveryMode.Default,
                            timeInterval: 0,
                            clientMaxSendQueueSize: byte.MaxValue)]
        private void ServerRemote_DeleteZoneMobs(IProtoZone zone)
        {
            Logger.Important("Destroying all spawned mobs at zone: " + zone.ShortId);

            var serverZone = zone.ServerZoneInstance;
            var worldService = Server.World;

            foreach (var zoneAttachedScript in zone.AttachedScripts.Where(s => s.ZoneScript is ProtoZoneSpawnScript))
            {
                var tracker = SpawnedMobsTrackingManagersStore.Get(
                    (ProtoZoneSpawnScript)zoneAttachedScript.ZoneScript,
                    serverZone);

                foreach (var character in tracker.EnumerateAll().ToList())
                {
                    worldService.DestroyObject(character);
                }

                tracker.Clear();
            }
        }

        private QuadTreeSnapshot ServerRemote_GetServerZoneData(IProtoZone protoZone)
        {
            var character = ServerRemoteContext.Character;
            var quadTreeSnapshot = protoZone.ServerZoneInstance.GetPositionsSnapshot();
            Logger.Important($"Zone quadtree sent: {protoZone} to {character}");
            return quadTreeSnapshot;
        }

        private void ServerRemote_InvokeZoneScripts(IProtoZone protoZone, bool isInitialSpawn)
        {
            var serverZoneInstance = protoZone.ServerZoneInstance;
            if (protoZone.AttachedScripts.Count == 0)
            {
                Api.Logger.Dev("No scripts attached to the zone: " + protoZone.ShortId);
                return;
            }

            var trigger = isInitialSpawn
                              ? (ProtoTrigger)Api.GetProtoEntity<TriggerWorldInit>()
                              : (ProtoTrigger)Api.GetProtoEntity<TriggerTimeInterval>();

            using var tempList = Api.Shared.GetTempList<IZoneScriptConfig>();
            // process attached scripts in the random order (same as real triggers in the game)
            tempList.AddRange(protoZone.AttachedScripts);
            // Disabled because it break some spawn scripts order!
            //tempList.Shuffle();

            foreach (var script in tempList.AsList())
            {
                script.ServerInvoke(trigger, serverZoneInstance);
            }
        }
    }
}