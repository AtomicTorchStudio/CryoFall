namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorActiveToolZones : EditorActiveToolTileBrush
    {
        private readonly ClientComponentCurrentZonesWatcher currentZonesWatcher;

        private readonly EditorToolZonePointedZoneNamesControl pointedZoneNamesControl;

        private readonly Dictionary<ViewModelProtoZone, EditorToolZoneRenderer> zoneRenderers
            = new();

        public EditorActiveToolZones(
            ApplyToolDelegate onSelected,
            OnPointedZonesChangedDelegate onPointedZonesChanged,
            Action onReleaseInput = null)
            : base(onSelected, onReleaseInput: onReleaseInput, isReactOnRightMouseButton: true)
        {
            this.currentZonesWatcher = this.SceneObject.AddComponent<ClientComponentCurrentZonesWatcher>();
            this.currentZonesWatcher.Setup(this.Component, this, onPointedZonesChanged);

            this.pointedZoneNamesControl = new EditorToolZonePointedZoneNamesControl();
            this.pointedZoneNamesControl.HorizontalAlignment = HorizontalAlignment.Center;
            this.pointedZoneNamesControl.VerticalAlignment = VerticalAlignment.Bottom;
            Api.Client.UI.LayoutRootChildren.Add(this.pointedZoneNamesControl);

            Instance = this;

            // ensure that zones are synced otherwise the zone detection for the pointed tile will not work
            foreach (var protoZone in Api.FindProtoEntities<IProtoZone>())
            {
                ClientZoneProvider.Get(protoZone);
            }
        }

        public delegate void OnPointedZonesChangedDelegate(HashSet<IProtoZone> pointedZones);

        public static EditorActiveToolZones Instance { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var zoneRenderer in this.zoneRenderers.Values)
            {
                zoneRenderer.Dispose();
            }

            this.zoneRenderers.Clear();
            Api.Client.UI.LayoutRootChildren.Remove(this.pointedZoneNamesControl);

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public Color GetForeground(IProtoZone protoZone)
        {
            foreach (var editorToolZoneRenderer in this.zoneRenderers)
            {
                if (editorToolZoneRenderer.Key.Zone == protoZone)
                {
                    return editorToolZoneRenderer.Key.Color;
                }
            }

            return Colors.White;
        }

        public void RefreshCurrentZonesList()
        {
            this.currentZonesWatcher.ForceRefresh();
            this.pointedZoneNamesControl.ForceRefresh();
        }

        public void RefreshZoneRenderers(List<ViewModelProtoZone> renderedZones)
        {
            var addList = renderedZones.Except(this.zoneRenderers.Keys).ToList();
            var removeList = this.zoneRenderers.Keys.Except(renderedZones).ToList();

            // remove not rendered anymore zones
            foreach (var protoZone in removeList)
            {
                this.zoneRenderers[protoZone].Dispose();
                this.zoneRenderers.Remove(protoZone);
            }

            // add renderers for new zones
            foreach (var viewModelProtoZone in addList)
            {
                var zoneProvider = ClientZoneProvider.Get(viewModelProtoZone.Zone);
                this.zoneRenderers[viewModelProtoZone] = new EditorToolZoneRenderer(zoneProvider);
            }

            // establish the zone indexes (determines the draw order)
            var usedIndexes = this.zoneRenderers.Values.Select(zr => zr.ZoneIndex).Where(zi => zi >= 0).ToList();
            usedIndexes.Sort();

            foreach (var protoZone in renderedZones)
            {
                var zoneRenderer = this.zoneRenderers[protoZone];
                if (zoneRenderer.ZoneIndex != -1)
                {
                    // zone index is already assigned - do not change
                    continue;
                }

                sbyte selectedZoneIndex = 0;
                var isIndexSelected = false;
                sbyte previousIndex = -1;

                for (var i = 0; i < usedIndexes.Count; i++)
                {
                    // iterate over all the used indexes and find "holes"
                    var zoneIndex = usedIndexes[i];
                    if (zoneIndex - previousIndex > 1)
                    {
                        selectedZoneIndex = (sbyte)(previousIndex + 1);
                        usedIndexes.Insert(i + 1, selectedZoneIndex);
                        isIndexSelected = true;
                        break;
                    }

                    previousIndex = zoneIndex;
                }

                if (!isIndexSelected)
                {
                    selectedZoneIndex = usedIndexes.Count > 0
                                            ? (sbyte)(usedIndexes.Last() + 1)
                                            : (sbyte)0;

                    usedIndexes.Add(selectedZoneIndex);
                }

                zoneRenderer.ZoneIndex = selectedZoneIndex;
            }

            foreach (var pair in this.zoneRenderers)
            {
                pair.Key.Color = pair.Value.Color;
            }
        }

        internal void AddZonesRenderedAtPosition(Vector2Ushort tilePosition, HashSet<IProtoZone> protoZones)
        {
            foreach (var zoneProvider in ClientZoneProvider.AllProviders)
            {
                if (zoneProvider.IsFilledPosition(tilePosition))
                {
                    protoZones.Add(zoneProvider.ProtoZone);
                }
            }

            /*// enumerate only the rendered zones
            foreach (var zoneRenderer in this.zoneRenderers.Values)
            {
                if (zoneRenderer.ZoneProvider.IsFilledPosition(tilePosition))
                {
                    protoZones.Add(zoneRenderer.ZoneProvider.ProtoZone);
                }
            }*/
        }
    }
}