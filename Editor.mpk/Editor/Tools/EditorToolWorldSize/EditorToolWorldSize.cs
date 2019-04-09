namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolWorldSize
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.Structures;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolWorldSize : BaseEditorTool
    {
        private ViewModelEditorToolWorldSizeSettings settingsViewModel;

        public override string Name => "World size tool";

        public override int Order => 50;

        public override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            // there are no brush or any other active tool
            return null;
        }

        public override FrameworkElement CreateSettingsControl()
        {
            var control = new EditorToolWorldSizeSettings();
            if (this.settingsViewModel == null)
            {
                this.settingsViewModel = new ViewModelEditorToolWorldSizeSettings(
                    this.ClientApplyWorldSizeExtension,
                    this.ClientApplyWorldSizeOptimization,
                    this.ClientApplyWorldSizeSliceExpansion);
            }

            control.Setup(this.settingsViewModel);
            return control;
        }

        private static BoundsUshort CalculateNewWorldBounds(BoundsUshort locationBounds, BoundsUshort oldWorldBounds)
        {
            var newSize = oldWorldBounds.Size.ToVector2Int() + locationBounds.Size.ToVector2Int();
            var newMaxPosition = oldWorldBounds.Offset.ToVector2Int() + newSize;
            if (newMaxPosition.X > ushort.MaxValue
                || newMaxPosition.Y > ushort.MaxValue)
            {
                throw new Exception(
                    $"Max world size exceeded: please ensure that the world size is not higher than {ushort.MaxValue}x{ushort.MaxValue} (including the world offset)");
            }

            return new BoundsUshort(oldWorldBounds.Offset,
                                    newSize.ToVector2Ushort());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2Ushort ConvertPosition(
            BoundsUshort insertedArea,
            ushort oldX,
            ushort oldY)
        {
            var newX = oldX < insertedArea.Offset.X
                           ? oldX
                           : (ushort)(oldX + insertedArea.Size.X);

            var newY = oldY <= insertedArea.Offset.Y
                           ? oldY
                           : (ushort)(oldY + insertedArea.Size.Y);

            var newPosition = new Vector2Ushort(newX, newY);
            return newPosition;
        }

        private void ClientApplyWorldSizeExtension()
        {
            this.CallServer(
                _ => _.ServerRemote_ApplyWorldSizeExtension(
                    this.settingsViewModel.ExtensionSelectedSide.Value,
                    this.settingsViewModel.ExtensionSelectedSize.Value));
        }

        private void ClientApplyWorldSizeOptimization()
        {
            this.CallServer(
                _ => _.ServerRemote_ApplyWorldSizeOptimization(this.settingsViewModel.SelectedPaddingSize.Value));
        }

        private void ClientApplyWorldSizeSliceExpansion()
        {
            var location = this.settingsViewModel.ViewModelSelectWorldSizeSliceExpansionLocation;
            var insertedArea = new BoundsUshort(new Vector2Ushort(location.OffsetX, location.OffsetY),
                                                new Vector2Ushort(location.SizeX,   location.SizeY));

            if (insertedArea.Size == Vector2Ushort.Zero)
            {
                NotificationSystem.ClientShowNotification("Please enter the size of expansion",
                                                          color: NotificationColor.Bad);
                return;
            }

            if (insertedArea.Size.X % ScriptingConstants.WorldChunkSize != 0
                || insertedArea.Size.Y % ScriptingConstants.WorldChunkSize != 0)
            {
                NotificationSystem.ClientShowNotification(
                    $"Please ensure that the size of expansion can be divided on the world chunk size ({ScriptingConstants.WorldChunkSize}) without the remainder",
                    color: NotificationColor.Bad);
                return;
            }

            this.CallServer(_ => _.ServerRemote_ApplyWorldSizeSliceExpansion(insertedArea));
        }

        private void ServerRemote_ApplyWorldSizeExtension(Side side, ushort sizeTiles)
        {
            Server.World.ExtendWorldSize(side, sizeTiles, GetProtoEntity<TileWaterSea>());
        }

        private void ServerRemote_ApplyWorldSizeOptimization(ushort sizeTiles)
        {
            Server.World.OptimizeWorldSize(sizeTiles, GetProtoEntity<TileWaterSea>());
        }

        private void ServerRemote_ApplyWorldSizeSliceExpansion(BoundsUshort insertedArea)
        {
            Logger.Info("Slice-expanding the world map - inserting an area: " + insertedArea);

            var oldWorldBounds = Server.World.WorldBounds;
            var oldOffset = oldWorldBounds.Offset;
            var oldSize = oldWorldBounds.Size;

            var oldMap = new Tile[oldSize.X, oldSize.Y];
            for (var x = 0; x < oldSize.X; x++)
            for (var y = 0; y < oldSize.Y; y++)
            {
                oldMap[x, y] = Server.World.GetTile(x + oldOffset.X,
                                                    y + oldOffset.Y);
            }

            Logger.Info("Tile height data gathered");

            var oldStaticObjects = Server.World.EditorEnumerateAllStaticObjects()
                                         .Select(o => new EditorStaticObjectsRemovalHelper.RestoreObjectRequest(o))
                                         .ToList();

            Logger.Info("Static objects gathered");

            // gather zones
            var oldZones = (from protoZone in Api.FindProtoEntities<IProtoZone>()
                            select new { protoZone, snapshot = protoZone.ServerZoneInstance.QuadTree.SaveQuadTree() })
                .ToList();

            // create new world
            var newWorldBounds = CalculateNewWorldBounds(insertedArea, oldWorldBounds);
            Server.World.CreateWorld(protoTile: Api.GetProtoEntity<TileWaterSea>(),
                                     newWorldBounds);

            Logger.Info("Server zones data gathered");

            // copy old map data to the new world
            for (var x = 0; x < oldSize.X; x++)
            for (var y = 0; y < oldSize.Y; y++)
            {
                var oldData = oldMap[x, y];
                var oldX = (ushort)(x + oldOffset.X);
                var oldY = (ushort)(y + oldOffset.Y);

                var newPosition = ConvertPosition(insertedArea, oldX, oldY);
                Server.World.SetTileData(newPosition,
                                         oldData.ProtoTile,
                                         oldData.Height,
                                         oldData.IsSlope,
                                         oldData.IsCliff);
            }

            Server.World.FixMapTilesAll();

            Logger.Info("Map slice-expansion finished");

            // restore old static objects to the new world
            foreach (var request in oldStaticObjects)
            {
                var newPosition = ConvertPosition(insertedArea, request.TilePosition.X, request.TilePosition.Y);
                Server.World.CreateStaticWorldObject(
                    request.Prototype,
                    newPosition);
            }

            Logger.Info("Static objects restored after the slice-expansion");

            // restore zones
            foreach (var oldZoneSnapshot in oldZones)
            {
                var oldQuadTree = QuadTreeNodeFactory.Create(oldZoneSnapshot.snapshot);
                var newQuadTree = oldZoneSnapshot.protoZone.ServerZoneInstance.QuadTree;
                foreach (var oldPosition in oldQuadTree)
                {
                    var newPosition = ConvertPosition(insertedArea, oldPosition.X, oldPosition.Y);
                    newQuadTree.SetFilledPosition(newPosition);
                }
            }

            Logger.Info("Server zones data restored after the slice-expansion");
        }
    }
}