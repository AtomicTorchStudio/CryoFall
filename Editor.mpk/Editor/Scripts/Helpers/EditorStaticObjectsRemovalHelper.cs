namespace AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class EditorStaticObjectsRemovalHelper
    {
        private const int RecentlyDeletedObjectIdsMaxEntriesCount = 5000;

        private static readonly List<uint> RecentlyDeletedObjectIds
            = new();

        public static void ClientDelete(IReadOnlyCollection<IStaticWorldObject> worldObjectsToDelete)
        {
            ClientDeleteInternal(worldObjectsToDelete);
        }

        public static void ClientDelete(BoundsUshort worldBoundsToDeleteObjects)
        {
            var staticObjects = Api.Client.World.GetStaticObjectsAtBounds(worldBoundsToDeleteObjects);
            ClientDeleteInternal(staticObjects);
        }

        private static void ClientDeleteInternal(IReadOnlyCollection<IStaticWorldObject> worldObjectsToDelete)
        {
            worldObjectsToDelete = worldObjectsToDelete.Where(o => !RecentlyDeletedObjectIds.Contains(o.Id))
                                                       .ToList();
            if (worldObjectsToDelete.Count == 0)
            {
                return;
            }

            worldObjectsToDelete = worldObjectsToDelete.Distinct().ToList();
            var storageEntry = new ActionStorageEntry(
                undoDeleteBatches: worldObjectsToDelete.Select(o => new SpawnObjectRequest(o))
                                                       .Batch(20000)
                                                       .Select(b => b.ToList())
                                                       .ToList(),
                deleteBatches: worldObjectsToDelete.Select(o => o.Id)
                                                   .Batch(20000)
                                                   .Select(b => b.ToList())
                                                   .ToList());

            EditorClientActionsHistorySystem.DoAction(
                "Delete objects",
                onDo: () =>
                      {
                          var countDeleted = 0;
                          foreach (var batch in storageEntry.DeleteBatches)
                          {
                              countDeleted += batch.Count;
                              RecentlyDeletedObjectIds.AddRange(batch);
                              WorldObjectsEditingSystem.Instance.CallServer(
                                  _ => _.ServerRemote_DeleteObjects(batch));
                              // the delete batch is no longer valid
                              batch.Clear();
                          }

                          NotificationSystem.ClientShowNotification(
                              title: null,
                              message: countDeleted + " objects(s) removed!",
                              color: NotificationColor.Good);

                          // trim the list
                          if (RecentlyDeletedObjectIds.Count > RecentlyDeletedObjectIdsMaxEntriesCount)
                          {
                              RecentlyDeletedObjectIds.RemoveRange(
                                  0,
                                  RecentlyDeletedObjectIds.Count - RecentlyDeletedObjectIdsMaxEntriesCount);
                          }
                      },
                onUndo: async () =>
                        {
                            var spawnBatches = storageEntry.UndoDeleteBatches;
                            for (var index = 0; index < spawnBatches.Count; index++)
                            {
                                var batch = spawnBatches[index];
                                var spawnedObjectsIds =
                                    await WorldObjectsEditingSystem.Instance.CallServer(
                                        _ => _.ServerRemote_SpawnObjects(batch));
                                // record spawned objects IDs so they could be removed on undo
                                var undoSpawnBatch = storageEntry.DeleteBatches[index];
                                undoSpawnBatch.Clear();
                                undoSpawnBatch.AddRange(spawnedObjectsIds);
                            }

                            NotificationSystem.ClientShowNotification(
                                title: null,
                                message: spawnBatches.Sum(r => r.Count) + " object(s) restored!",
                                color: NotificationColor.Good);
                        },
                canGroupWithPreviousAction: false);
        }

        private class ActionStorageEntry
        {
            public ActionStorageEntry(
                List<List<SpawnObjectRequest>> undoDeleteBatches,
                List<List<uint>> deleteBatches)
            {
                this.UndoDeleteBatches = undoDeleteBatches;
                this.DeleteBatches = deleteBatches;
            }

            public List<List<uint>> DeleteBatches { get; }

            public List<List<SpawnObjectRequest>> UndoDeleteBatches { get; }
        }
    }
}