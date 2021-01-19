namespace AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class EditorStaticObjectsCopyPasteHelper
    {
        private const string ObjectPlacementGuide
            = @"[br]Press ENTER to paste in the selected destination.
                [br]Press ESC or Ctrl+Z to cancel.";

        private static readonly IClientStorage ClientStorage;

        private static BufferEntry? lastBufferEntry;

        static EditorStaticObjectsCopyPasteHelper()
        {
            if (!Api.IsClient)
            {
                return;
            }

            ClientStorage = Api.Client.Storage.GetStorage("Editor/" + nameof(EditorStaticObjectsCopyPasteHelper));
            ClientStorage.RegisterType(typeof(Vector2Ushort));
            ClientStorage.RegisterType(typeof(SpawnObjectRequest));
            ClientStorage.RegisterType(typeof(BufferEntry));

            TryLoadBufferEntry();
        }

        public static void ClientCopy(IReadOnlyCollection<IStaticWorldObject> worldObjectsToCopy)
        {
            ClientCopyInternal(worldObjectsToCopy);
        }

        public static void ClientCopy(BoundsUshort worldBoundsToCopyObjects)
        {
            var staticObjects = Api.Client.World.GetStaticObjectsAtBounds(worldBoundsToCopyObjects);
            ClientCopyInternal(staticObjects);
        }

        public static void ClientPaste(Vector2Ushort tilePosition)
        {
            if (lastBufferEntry is null)
            {
                return;
            }

            var bufferEntry = lastBufferEntry.Value;
            if (ClientEditorAreaSelectorHelper.Instance is not null)
            {
                NotificationSystem.ClientShowNotification(
                    title: null,
                    message:
                    "You're already in object placement mode."
                    + ObjectPlacementGuide,
                    color: NotificationColor.Neutral);
                return;
            }

            NotificationSystem.ClientShowNotification(
                title: null,
                message:
                $"{bufferEntry.SpawnList.Count} objects ready for paste!"
                + ObjectPlacementGuide,
                color: NotificationColor.Good);

            var originalTileStartX = bufferEntry.TilePositions.Min(t => t.X);
            var originalTileStartY = bufferEntry.TilePositions.Min(t => t.Y);
            var originalTileEndX = bufferEntry.TilePositions.Max(t => t.X);
            var originalTileEndY = bufferEntry.TilePositions.Max(t => t.Y);

            var originalSize = new Vector2Ushort((ushort)(originalTileEndX - originalTileStartX + 1),
                                                 (ushort)(originalTileEndY - originalTileStartY + 1));

            // ReSharper disable once ObjectCreationAsStatement
            new ClientEditorAreaSelectorHelper(tilePosition,
                                               originalSize,
                                               selectedCallback: PlaceSelectedCallback);

            void PlaceSelectedCallback(Vector2Ushort selectedTilePosition)
            {
                var originalStartX = bufferEntry.SpawnList.Min(t => t.TilePosition.X);
                var originalStartY = bufferEntry.SpawnList.Min(t => t.TilePosition.Y);
                var offset = selectedTilePosition.ToVector2Int() - (originalStartX, originalStartY);

                var storageEntry = new ActionStorageEntry(
                    spawnBatches: bufferEntry.SpawnList
                                             .Select(t => new SpawnObjectRequest(
                                                         t.Prototype,
                                                         t.TilePosition + offset))
                                             .Batch(20000)
                                             .Select(b => b.ToList())
                                             .ToList());

                EditorClientActionsHistorySystem.DoAction(
                    "Paste objects",
                    onDo: async () =>
                          {
                              var spawnBatches = storageEntry.SpawnBatches;
                              for (var index = 0; index < spawnBatches.Count; index++)
                              {
                                  var batch = spawnBatches[index];
                                  var spawnedObjectsIds =
                                      await WorldObjectsEditingSystem.Instance.CallServer(
                                          _ => _.ServerRemote_SpawnObjects(batch));
                                  // record spawned objects IDs so they could be removed on undo
                                  var undoSpawnBatch = storageEntry.UndoSpawnBatches[index];
                                  undoSpawnBatch.Clear();
                                  undoSpawnBatch.AddRange(spawnedObjectsIds);
                              }

                              NotificationSystem.ClientShowNotification(
                                  title: null,
                                  message: spawnBatches.Sum(r => r.Count) + " object(s) pasted!",
                                  color: NotificationColor.Good);
                          },
                    onUndo: () =>
                            {
                                var countRemoved = 0;
                                foreach (var batch in storageEntry.UndoSpawnBatches)
                                {
                                    countRemoved += batch.Count;
                                    WorldObjectsEditingSystem.Instance.CallServer(
                                        _ => _.ServerRemote_DeleteObjects(batch));
                                    // the delete batch is no longer valid
                                    batch.Clear();
                                }

                                NotificationSystem.ClientShowNotification(
                                    title: null,
                                    message: countRemoved + " objects(s) removed.",
                                    color: NotificationColor.Neutral);
                            },
                    canGroupWithPreviousAction: false);
            }
        }

        public static void DestroyInstance()
        {
            ClientEditorAreaSelectorHelper.Instance?.Destroy();
        }

        private static void ClientCopyInternal(IReadOnlyCollection<IStaticWorldObject> worldObjectsToCopy)
        {
            if (worldObjectsToCopy.Count == 0)
            {
                return;
            }

            worldObjectsToCopy = worldObjectsToCopy.Distinct().ToList();
            var spawnRequests = worldObjectsToCopy.Select(o => new SpawnObjectRequest(o))
                                                  .ToList();

            var tilePositions = worldObjectsToCopy
                                .SelectMany(t => t.OccupiedTilePositions)
                                .GroupBy(t => t)
                                .Select(g => g.Key)
                                .ToList();

            lastBufferEntry = new BufferEntry(spawnRequests, tilePositions);

            NotificationSystem.ClientShowNotification(title: null,
                                                      message: worldObjectsToCopy.Count + " objects copied!",
                                                      color: NotificationColor.Good);

            SaveLastBufferEntry();
        }

        private static void SaveLastBufferEntry()
        {
            if (lastBufferEntry.HasValue)
            {
                ClientStorage.Save(lastBufferEntry.Value);
            }
        }

        private static void TryLoadBufferEntry()
        {
            if (ClientStorage.TryLoad(out BufferEntry loaded))
            {
                lastBufferEntry = loaded;
            }
        }

        private readonly struct ActionStorageEntry
        {
            public ActionStorageEntry(List<List<SpawnObjectRequest>> spawnBatches)
            {
                this.SpawnBatches = spawnBatches;
                this.UndoSpawnBatches = this.SpawnBatches
                                            .Select(b => new List<uint>(b.Count))
                                            .ToList();
            }

            public List<List<SpawnObjectRequest>> SpawnBatches { get; }

            public List<List<uint>> UndoSpawnBatches { get; }
        }

        private readonly struct BufferEntry
        {
            public BufferEntry(
                List<SpawnObjectRequest> spawnList,
                List<Vector2Ushort> tilePositions)
            {
                this.SpawnList = spawnList;
                this.TilePositions = tilePositions;
            }

            public List<SpawnObjectRequest> SpawnList { get; }

            public List<Vector2Ushort> TilePositions { get; }
        }
    }
}