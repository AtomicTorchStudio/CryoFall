namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolZones : BaseEditorTool
    {
        private byte capturedHeight;

        private IProtoTile capturedTileProto;

        private ClientZoneProvider lastUsedZoneProvider;

        private ViewModelEditorToolZonesSettings settings;

        public override string Name => "Zone tool";

        public override int Order => 40;

        public override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            var tool = new EditorActiveToolZones(
                onSelected: this.ClientOnPaintZone,
                onPointedZonesChanged: this.ClientOnPointedZonesChanged);
            this.SetupActiveTool(tool);
            return tool;
        }

        public override FrameworkElement CreateSettingsControl()
        {
            var control = new EditorToolZonesSettings();
            if (this.settings is null)
            {
                this.settings = new ViewModelEditorToolZonesSettings();
                this.settings.BrushSettingsChanged += this.RefreshActiveTool;
                this.settings.SelectedZonesForRenderingChanged += this.RefreshActiveTool;
            }

            control.Setup(this.settings);
            return control;
        }

        private void ClientOnPaintZone(List<Vector2Ushort> tilePositions, bool isRepeat)
        {
            var selectedZoneForBrush = this.settings.SelectedZoneForBrush;
            if (selectedZoneForBrush is null
                || !selectedZoneForBrush.IsRendered)
            {
                // no zone selected for brush or zone is not visible
                return;
            }

            var protoZone = selectedZoneForBrush.Zone;
            var zoneProvider = ClientZoneProvider.Get(protoZone);
            if (!zoneProvider.IsDataReceived)
            {
                return;
            }

            var world = Client.World;
            var onlyOnTheSameHeight = this.settings.IsAllowZoneChangeOnlyOnTheSameHeight;
            if (this.settings.IsFillZoneMode)
            {
                if (isRepeat)
                {
                    // fill doesn't support repeat
                    return;
                }

                tilePositions = EditorTileHelper.GatherAllTilePositionsOfTheSameProtoTile(
                    tilePositions[0],
                    onlyOnTheSameHeight,
                    ignoreCliffsAndSlopes: true);
            }

            this.lastUsedZoneProvider = zoneProvider;

            if (onlyOnTheSameHeight
                && !isRepeat)
            {
                // capture height when starting painting the zone
                this.capturedHeight = this.settings.IsFillZoneMode
                                          ? world.GetTile(tilePositions[0]).Height
                                          : EditorTileHelper.CalculateAverageHeight(tilePositions);
            }

            var onlyOnTheSameTileProto = this.settings.IsAllowZoneChangeOnlyOnTheSameTileProto;
            if (onlyOnTheSameTileProto
                && !isRepeat)
            {
                // capture tile proto when starting painting the zone
                this.capturedTileProto = this.settings.IsFillZoneMode
                                             ? world.GetTile(tilePositions[0]).ProtoTile
                                             : EditorTileHelper.CalculateMostFrequentTileProto(tilePositions);
            }

            // determine the mode - adding points to zone or removing them
            var isAddMode = ClientInputManager.IsButtonHeld(GameButton.ActionUseCurrentItem, evenIfHandled: true)
                            || ClientInputManager.IsButtonUp(GameButton.ActionUseCurrentItem, evenIfHandled: true);

            if (isAddMode)
            {
                // remove points which are already added to the zone
                tilePositions.RemoveAll(tilePosition => zoneProvider.IsFilledPosition(tilePosition));
            }
            else
            {
                // remove points which are not presented in the zone
                tilePositions.RemoveAll(tilePosition => !zoneProvider.IsFilledPosition(tilePosition));
            }

            if (tilePositions.Count == 0)
            {
                // nothing to add/remove
                return;
            }

            if (onlyOnTheSameHeight)
            {
                // remove tiles with different height
                tilePositions.RemoveAll(
                    tilePosition =>
                    {
                        var tile = world.GetTile(tilePosition);
                        return tile.Height != this.capturedHeight
                               // also do not allow painting on cliffs and slopes
                               || tile.IsCliff
                               || tile.IsSlope;
                    });
                if (tilePositions.Count == 0)
                {
                    // nothing to add/remove
                    return;
                }
            }

            if (onlyOnTheSameTileProto)
            {
                // remove tiles with different proto
                var worldService = Client.World;
                tilePositions.RemoveAll(
                    tilePosition => worldService.GetTile(tilePosition).ProtoTile
                                    != this.capturedTileProto);

                if (tilePositions.Count == 0)
                {
                    // nothing to add/remove
                    return;
                }
            }

            foreach (var tilePosition in tilePositions)
            {
                if (isAddMode)
                {
                    // add point to the zone
                    zoneProvider.SetFilledPosition(tilePosition);
                }
                else
                {
                    // remove point from the zone
                    zoneProvider.ResetFilledPosition(tilePosition);
                }
            }

            this.lastUsedZoneProvider.ApplyClientChanges(forcePushChangesImmediately: tilePositions.Count > 10000);
        }

        private void ClientOnPointedZonesChanged(HashSet<IProtoZone> pointedZones)
        {
            foreach (var viewModelProtoZone in this.settings.Zones)
            {
                viewModelProtoZone.IsUnderCursor = pointedZones.Contains(viewModelProtoZone.Zone);
            }
        }

        private void RefreshActiveTool()
        {
            this.SetupActiveTool((EditorActiveToolZones)EditorActiveToolManager.ActiveTool);
        }

        private void SetupActiveTool(EditorActiveToolZones tool)
        {
            var brushSize = this.settings.SelectedBrushSize;
            var brush = this.settings.SelectedBrushShape.Value;

            tool.SetBrush(brush, brushSize);
            tool.RefreshZoneRenderers(
                this.settings.Zones
                    .Where(z => z.IsRendered)
                    .ToList());
        }
    }
}