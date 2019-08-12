namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.GameApi.Data.Structures;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientZoneProvider
    {
        public const double CommitZoneChangesDelaySeconds = 0.5;

        private static readonly IQuadTreeNode[] EmptyNodes = new IQuadTreeNode[0];

        private static readonly Dictionary<IProtoZone, ClientZoneProvider> Providers =
            new Dictionary<IProtoZone, ClientZoneProvider>();

        public readonly IProtoZone ProtoZone;

        private bool isNeedSyncToServer;

        private QuadTreeSnapshot lastClientSnapshot;

        private double lastModificationTime;

        private QuadTreeSnapshot lastServerSnapshot;

        private IQuadTreeNode quadTree;

        static ClientZoneProvider()
        {
            Api.Client.World.WorldBoundsChanged += WorldBoundsChangedHandler;
        }

        private ClientZoneProvider(IProtoZone protoZone)
        {
            this.ProtoZone = protoZone;
            this.SyncFromServer();
        }

        public delegate void ZoneModifiedEventArgs(QuadTreeDiff diff);

        public event ZoneModifiedEventArgs ZoneModified;

        public event Action ZoneReset;

        public bool IsDataReceived => this.quadTree != null;

        public static ClientZoneProvider Get(IProtoZone protoZone)
        {
            if (Providers.TryGetValue(protoZone, out var provider))
            {
                return provider;
            }

            provider = new ClientZoneProvider(protoZone);
            Providers[protoZone] = provider;
            return provider;
        }

        public void ApplyClientChanges(bool forcePushChangesImmediately)
        {
            this.ValidateIsDataReceived();

            var newSnapshot = this.quadTree.SaveQuadTree();
            var diffDo = QuadTreeDiff.Create(
                newSnapshot,
                this.lastClientSnapshot);
            if (diffDo.IsEmpty)
            {
                // quad trees are equal
                return;
            }

            var redo = false;
            EditorClientSystem.DoAction(
                "Modify zone " + this.ProtoZone.Id,
                onDo: () =>
                      {
                          if (redo)
                          {
                              this.quadTree.ApplyDiff(diffDo);
                          }

                          OnDiffApplied(diffDo);
                      },
                onUndo: () =>
                        {
                            // set flag for next "do" call to make it "redo"
                            redo = true;
                            var diffRedo = diffDo.ReverseDiff();
                            this.quadTree.ApplyDiff(diffRedo);
                            OnDiffApplied(diffRedo);
                        });

            // helper local function
            void OnDiffApplied(QuadTreeDiff appliedDiff)
            {
                this.isNeedSyncToServer = true;
                this.lastModificationTime = Api.Client.Core.ClientRealTime;

                this.lastClientSnapshot = this.quadTree.SaveQuadTree();
                this.ZoneModified?.Invoke(appliedDiff);

                if (forcePushChangesImmediately)
                {
                    this.SyncToServer(forceImmediate: true);
                }
                else
                {
                    this.ScheduleSyncToServer();
                }
            }
        }

        public void ClearZone()
        {
            this.ValidateIsDataReceived();
            this.quadTree.ResetNode();
        }

        public IEnumerable<IQuadTreeNode> EnumerateFilledNodes()
        {
            return this.quadTree?.EnumerateFilledNodes() ?? EmptyNodes;
        }

        public IQuadTreeNode GetQuadTree()
        {
            this.ValidateIsDataReceived();
            return this.quadTree;
        }

        public bool IsFilledPosition(Vector2Ushort tilePosition)
        {
            if (!this.IsDataReceived)
            {
                return false;
            }

            return this.quadTree.IsPositionFilled(tilePosition);
        }

        public void ResetFilledPosition(Vector2Ushort tilePosition)
        {
            this.ValidateIsDataReceived();
            this.quadTree.ResetFilledPosition(tilePosition);
        }

        public void SetFilledPosition(Vector2Ushort tilePosition)
        {
            this.ValidateIsDataReceived();
            this.quadTree.SetFilledPosition(tilePosition);
        }

        private static void WorldBoundsChangedHandler()
        {
            foreach (var clientZoneProvider in Providers.Values)
            {
                clientZoneProvider.Dispose();
            }

            Providers.Clear();
        }

        private void Dispose()
        {
            if (this.IsDataReceived)
            {
                this.ClearZone();
            }

            this.ZoneReset?.Invoke();
        }

        private void ScheduleSyncToServer()
        {
            ClientTimersSystem.AddAction(
                CommitZoneChangesDelaySeconds / 4.0,
                () => this.SyncToServer(forceImmediate: false));
        }

        private async void SyncFromServer()
        {
            var snapshot = await EditorZoneSystem.Instance.ClientRequestZoneData(this.ProtoZone);
            this.lastClientSnapshot = this.lastServerSnapshot = snapshot;
            this.quadTree = QuadTreeNodeFactory.Create(snapshot);
            this.ZoneReset?.Invoke();
        }

        private void SyncToServer(bool forceImmediate)
        {
            this.ValidateIsDataReceived();

            if (!this.isNeedSyncToServer)
            {
                return;
            }

            if (!forceImmediate)
            {
                if (Api.Client.Core.ClientRealTime - this.lastModificationTime
                    < CommitZoneChangesDelaySeconds)
                {
                    // commit delay is not yet exceeded - schedule commit next time
                    this.ScheduleSyncToServer();
                    return;
                }
            }

            this.isNeedSyncToServer = false;

            var newSnapshot = this.quadTree.SaveQuadTree();
            var diff = QuadTreeDiff.Create(newSnapshot, this.lastServerSnapshot);
            if (diff.IsEmpty)
            {
                // quad trees are equal
                return;
            }

            this.lastServerSnapshot = newSnapshot;
            EditorZoneSystem.Instance.ClientSendZoneModifications(this.ProtoZone, diff);

            (EditorActiveToolManager.ActiveTool as EditorActiveToolZones)?.RefreshCurrentZonesList();
        }

        private void ValidateIsDataReceived()
        {
            if (this.quadTree == null)
            {
                throw new Exception("The zone quadtree data is not received from the server yet.");
            }
        }
    }
}