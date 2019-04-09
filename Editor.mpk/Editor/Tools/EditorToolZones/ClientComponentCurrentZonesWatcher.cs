namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// This component watches for mouse cursor position changes in the Editor mode and refreshes list of currently visible
    /// zones.
    /// </summary>
    public class ClientComponentCurrentZonesWatcher : ClientComponent
    {
        private readonly HashSet<IProtoZone> zonesAtCurrentBrush = new HashSet<IProtoZone>();

        private ClientComponentEditorToolActiveTileBrush brushComponent;

        private EditorActiveToolZones editorActiveToolZones;

        private Vector2Ushort lastTilePosition;

        private EditorActiveToolZones.OnPointedZonesChangedDelegate onPointedZonesChanged;

        public void ForceRefresh()
        {
            var tilePosition = Client.Input.MousePointedTilePosition;
            this.lastTilePosition = tilePosition;

            var tilePositionInt = (Vector2Int)tilePosition;
            var offsets = EditorTileOffsetsHelper.GenerateOffsets(
                this.brushComponent.Brush,
                this.brushComponent.BrushSize,
                isForPreview: true);
            var worldBounds = Client.World.WorldBounds;

            // determine zones at current brush positions
            this.zonesAtCurrentBrush.Clear();
            foreach (var offset in offsets)
            {
                var p = tilePositionInt + offset;
                if (worldBounds.Contains(p))
                {
                    this.editorActiveToolZones.AddZonesRenderedAtPosition(
                        p.ToVector2Ushort(),
                        this.zonesAtCurrentBrush);
                }
            }

            this.onPointedZonesChanged(this.zonesAtCurrentBrush);
        }

        public void Setup(
            ClientComponentEditorToolActiveTileBrush brushComponent,
            EditorActiveToolZones editorActiveToolZones,
            EditorActiveToolZones.OnPointedZonesChangedDelegate onPointedZonesChanged)
        {
            this.brushComponent = brushComponent;
            this.editorActiveToolZones = editorActiveToolZones;
            this.onPointedZonesChanged = onPointedZonesChanged;
        }

        public override void Update(double deltaTime)
        {
            var tilePosition = Client.Input.MousePointedTilePosition;
            if (this.lastTilePosition == tilePosition)
            {
                return;
            }

            this.ForceRefresh();
        }
    }
}