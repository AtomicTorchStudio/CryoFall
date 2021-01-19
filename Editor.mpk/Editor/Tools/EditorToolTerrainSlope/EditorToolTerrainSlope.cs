namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrainSlope
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolTerrainSlope : BaseEditorTool
    {
        public override string Name => "Terrain slope tool";

        public override int Order => 20;

        public override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            var tool = new EditorActiveToolTileBrush(
                onSelected: this.ClientPlaceAt,
                validateCallback: tilePositions => this.ValidateCallback(tilePositions, out var notUsed),
                isRepeatOnMove: false);

            tool.SetCustomBrush(
                Vector2Int.Zero,
                new Vector2Int(1, 0));
            return tool;
        }

        public override FrameworkElement CreateSettingsControl()
        {
            return new FormattedTextBlock()
            {
                Content =
                    @"Click on a cliff to create a slope.
                      [br]Click on a slope to remove it.",
                Foreground = Brushes.White,
                FontSize = 11
            };
        }

        private void ClientPlaceAt(List<Vector2Ushort> tilePositions, bool isRepeat)
        {
            this.ValidateCallback(tilePositions, out var tilePosition);

            var tile = Client.World.GetTile(tilePosition);
            var previousIsSlope = tile.IsSlope;
            var newIsSlope = !previousIsSlope;

            EditorClientActionsHistorySystem.DoAction(
                "Toggle terrain slope",
                onDo: () => this.CallServer(_ => _.ServerRemote_PlaceAt(tilePosition,   newIsSlope)),
                onUndo: () => this.CallServer(_ => _.ServerRemote_PlaceAt(tilePosition, previousIsSlope)),
                canGroupWithPreviousAction: false);
        }

        [RemoteCallSettings(DeliveryMode.Default,
                            timeInterval: 0,
                            clientMaxSendQueueSize: byte.MaxValue)]
        private void ServerRemote_PlaceAt(Vector2Ushort tilePosition, bool isSlope)
        {
            var worldService = Server.World;
            var tile = worldService.GetTile(tilePosition);
            worldService.SetTileData(
                tilePosition,
                tile.ProtoTile,
                tileHeight: tile.Height,
                isSlope: isSlope,
                isCliff: false);

            worldService.SetTileData(
                tilePosition.AddAndClamp(new Vector2Ushort(1, 0)),
                tile.ProtoTile,
                tileHeight: tile.Height,
                isSlope: isSlope,
                isCliff: false);

            worldService.FixMapTilesRecentlyModified();
        }

        private bool ValidateCallback(List<Vector2Ushort> tilePositions, out Vector2Ushort tilePosition)
        {
            tilePosition = tilePositions[0];
            var tileLeft = Client.World.GetTile(tilePosition);
            var tileRight = tileLeft.NeighborTileRight;

            if (!tileLeft.IsSlope
                && tileRight.IsSlope)
            {
                tileLeft = tileRight;
                tilePosition = tileLeft.Position;
                tileRight = tileLeft.NeighborTileRight;
            }

            if (!tileLeft.IsSlope)
            {
                var tileLeftUp = tileLeft.NeighborTileUp;
                var tileLeftDown = tileLeft.NeighborTileDown;
                var tileRightUp = tileRight.NeighborTileUp;
                var tileRightDown = tileRight.NeighborTileDown;

                if (!(tileLeft.IsCliff && tileRight.IsCliff)
                    && !(tileLeft.IsSlope && tileRight.IsSlope))
                {
                    // not a slope or cliff
                    return false;
                }

                if (!tileLeft.NeighborTileLeft.IsCliff
                    || !tileRight.NeighborTileRight.IsCliff)
                {
                    // not surrounded by cliffs
                    return false;
                }

                if (tileLeftUp.IsCliff
                    || tileRightUp.IsCliff)
                {
                    // up tiles are cliffs
                    return false;
                }

                if (tileLeftDown.IsCliff
                    || tileRightDown.IsCliff)
                {
                    // down tiles are cliffs
                    return false;
                }

                if (tileLeftUp.Height == tileLeftDown.Height
                    || tileRightUp.Height == tileRightDown.Height)
                {
                    // this is a cliff between the same heights
                    return false;
                }
            }
            else if (tileLeft.NeighborTileLeft.IsSlope)
            {
                tileLeft = tileLeft.NeighborTileLeft;
                tilePosition = tileLeft.Position;
            }

            return true;
        }
    }
}